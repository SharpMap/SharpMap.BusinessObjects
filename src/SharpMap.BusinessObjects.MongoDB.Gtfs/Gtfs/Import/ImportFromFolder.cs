/*
 * Copyright © 2017 - Felix Obermaier, Ingenieurgruppe IVV GmbH & Co. KG
 * 
 * This file is part of SharpMap.BusinessObjects.MongoDB.Gtfs.
 *
 * SharpMap.BusinessObjects.MongoDB.Gtfs is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * SharpMap.BusinessObjects.MongoDB.Gtfs is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with SharpMap; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
 *
 */
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using MongoDB.Driver;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs.Import
{
    /// <summary>
    /// 
    /// </summary>
    public class ImportFromFolder
    {
        private static volatile int _tried;
        private const string GtfsConnection = "mongodb://IVV-T3S:27017";
        private const string GtfsLocalConnection = "mongodb://localhost";

        private const string StartMongoDbServerCommand = @"c:\mongodb\bin\mongod.exe";
        private const string StartMongoDbServerArguments = @"-directoryperdb -dbpath c:\mongodb\data --journal --noauth";

        static MongoClient GetClient()
        {
            try
            {
                return new MongoClient(GtfsConnection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return GetLocalClient();
            }
        }

        static MongoClient GetLocalClient()
        {
            try
            {
                var client = new MongoClient(GtfsLocalConnection);
                //var server = client.GetServer();
                //var databases = server.GetDatabaseNames();
                return client;
            }
            catch(Exception ex)
            {
                if (_tried == 0)
                    System.Diagnostics.Process.Start(StartMongoDbServerCommand, StartMongoDbServerArguments);
                else
                {
                    LogManager.GetCurrentClassLogger().Debug(fmh => fmh("MongoDB Server not found or running\n{0}", ex));
                    throw;
                }
                _tried++;
                Thread.Sleep(2000);
                return GetLocalClient();
            }
        }

        public static void Import(DirectoryInfo directory)
        {
            var client = GetClient();
            client.GetServer().DropDatabase(directory.Name);
            var database = client.GetServer().GetDatabase(directory.Name);

            // 
            UintIdGenerator.SetKeyTracker(database.GetCollection<UintIdGenerator.UintKeyTracker>("uintkeys"));

            // Required
            var t1 = Task.Factory.StartNew(ImportAgency, new object[] {database, directory.GetFiles("agency.txt")[0]});
            var t2 = Task.Factory.StartNew(ImportStop, new object[] { database, directory.GetFiles("stops.txt")[0] });
            var t3 = Task.Factory.StartNew(ImportRoute, new object[] { database, directory.GetFiles("routes.txt")[0] });
            var t4 = Task.Factory.StartNew(ImportTrip, new object[] { database, directory.GetFiles("trips.txt")[0] });
            var t5 = Task.Factory.StartNew(ImportStopTimes, new object[] { database, directory.GetFiles("stop_times.txt")[0] });
            var t6 = Task.Factory.StartNew(ImportOptional, new object[] { database, directory.GetFiles("calendar.txt"), (ImportToMongoDb)ImportCalendar });

            // Optional
            var t7 = Task.Factory.StartNew(ImportOptional, new object[] { database, directory.GetFiles("calendar_dates.txt"), (ImportToMongoDb)ImportCalendarDate });
            var t8 = Task.Factory.StartNew(ImportOptional, new object[] { database, directory.GetFiles("fare_attributes.txt"), (ImportToMongoDb)ImportFareAttribute });
            var t9 = Task.Factory.StartNew(ImportOptional, new object[] { database, directory.GetFiles("fare_rules.txt"), (ImportToMongoDb)ImportFareRule });
            var t10 = Task.Factory.StartNew(ImportOptional, new object[] { database, directory.GetFiles("shapes.txt"), (ImportToMongoDb)ImportShape });
            var t11 = Task.Factory.StartNew(ImportOptional, new object[] { database, directory.GetFiles("frequencies.txt"), (ImportToMongoDb)ImportFrequency });
            var t12 = Task.Factory.StartNew(ImportOptional, new object[] { database, directory.GetFiles("transfers.txt"), (ImportToMongoDb)ImportTransfer });
            var t13 = Task.Factory.StartNew(ImportOptional, new object[] { database, directory.GetFiles("feed_info.txt"), (ImportToMongoDb)ImportFeedInfo });

            Task.WaitAll(t1, t2, t3, t4, t5, t6, 
                         t7, t8, t9, t10, t11, t12, t13);
        }

        /// <summary>
        /// Delegate function to import a text file to a MongoDB
        /// </summary>
        /// <param name="db">The mongo database</param>
        /// <param name="fi">The file information</param>
        private delegate void ImportToMongoDb(MongoDatabase db, FileInfo fi);

        private static void ImportOptional(object param)
        {
            var paras = (object[])param;
            var fileInfo = (FileInfo[])paras[1];
            if (fileInfo != null && fileInfo.Length > 0)
            {
                var action = (ImportToMongoDb) paras[2];
                var database = (MongoDatabase)paras[0];
                action(database, fileInfo[0]);
            }
        }

        private static void ImportFeedInfo(MongoDatabase database, FileInfo getFeedInfo)
        {
            var feedInfo = database.GetCollection<FeedInfo>(getFeedInfo.Name);
            feedInfo.InsertBatch(Associator<FeedInfo>.Read(new StreamReader(getFeedInfo.OpenRead())));
        }

        private static void ImportTransfer(MongoDatabase database, FileInfo getTransfer)
        {
            var shape = database.GetCollection<Transfer>(getTransfer.Name);
            shape.InsertBatch(Associator<Transfer>.Read(new StreamReader(getTransfer.OpenRead())));
            shape.CreateIndex("from_stop_id", "to_stop_id");
        }

        private static void ImportFrequency(MongoDatabase database, FileInfo getFrequency)
        {
            var shape = database.GetCollection<Frequency>(getFrequency.Name);
            shape.InsertBatch(Associator<Frequency>.Read(new StreamReader(getFrequency.OpenRead())));
            shape.CreateIndex("trip_id");
        }

        private static void ImportShape(MongoDatabase database, FileInfo getShape)
        {
            var shape = database.GetCollection<Shape>(getShape.Name);
            shape.InsertBatch(Associator<Shape>.Read(new StreamReader(getShape.OpenRead())));
            shape.CreateIndex("shape_id", "shape_sequence");
        }

        private static void ImportFareRule(MongoDatabase database, FileInfo getFareRule)
        {
            var fareAttribute = database.GetCollection<FareRule>(getFareRule.Name);
            fareAttribute.InsertBatch(Associator<FareRule>.Read(new StreamReader(getFareRule.OpenRead())));
            fareAttribute.CreateIndex("fare_id", "route_id");
        }

        private static void ImportFareAttribute(MongoDatabase database, FileInfo getFareAttribute)
        {
            var fareAttribute = database.GetCollection<FareAttribute>(getFareAttribute.Name);
            fareAttribute.InsertBatch(Associator<FareAttribute>.Read(new StreamReader(getFareAttribute.OpenRead())));
            fareAttribute.CreateIndex("fare_id");
        }

        private static void ImportCalendarDate(MongoDatabase database, FileInfo getCalenderDate)
        {
            var calendar = database.GetCollection<CalendarDate>("calendar_date");
            calendar.InsertBatch(Associator<CalendarDate>.Read(new StreamReader(getCalenderDate.OpenRead())));
            calendar.CreateIndex("service_id");
        }

        private static void ImportCalendar(MongoDatabase database, FileInfo getFile)
        {
            var calendar = database.GetCollection<Calendar>("calendar");
            calendar.InsertBatch(Associator<Calendar>.Read(new StreamReader(getFile.OpenRead())));
            calendar.CreateIndex("service_id");
        }

        private static void ImportStopTimes(object param)
        {
            var paras = (object[])param;
            var database = (MongoDatabase)paras[0];
            var fileInfo = (FileInfo)paras[1];
            ImportStopTimes(database, fileInfo);
        }
        private static void ImportStopTimes(MongoDatabase database, FileInfo getFile)
        {
            var stopTimes = database.GetCollection<StopTime>("stop_times");
            stopTimes.InsertBatch(Associator<StopTime>.Read(new StreamReader(getFile.OpenRead())));
            stopTimes.CreateIndex("trip_id");
            stopTimes.CreateIndex("trip_id", "stop_id", "stop_sequence");
        }

        private static void ImportRoute(object param)
        {
            var paras = (object[])param;
            var database = (MongoDatabase)paras[0];
            var fileInfo = (FileInfo)paras[1];
            ImportRoute(database, fileInfo);
        }
        private static void ImportRoute(MongoDatabase database, FileInfo fileInfo)
        {
            var routes = database.GetCollection("routes");
            routes.InsertBatch(Associator<Route>.Read(new StreamReader(fileInfo.OpenRead())));
        }

        private static void ImportTrip(object param)
        {
            var paras = (object[])param;
            var database = (MongoDatabase)paras[0];
            var fileInfo = (FileInfo)paras[1];
            ImportTrip(database, fileInfo);
        }
        private static void ImportTrip(MongoDatabase database, FileInfo fileInfo)
        {
            var routes = database.GetCollection("trips");
            routes.InsertBatch(Associator<Trip>.Read(new StreamReader(fileInfo.OpenRead())));
        }

        public static Regex Csv { get { return new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)"); } }

        private static void ImportStop(object param)
        {
            var paras = (object[]) param;
            var database = (MongoDatabase) paras[0];
            var fileInfo = (FileInfo) paras[1];
            ImportStop(database,fileInfo);
        }

        private static void ImportStop(MongoDatabase database, FileInfo fileInfo)
        {
            var stops = database.GetCollection<Stop>("stops");
            stops.InsertBatch(Associator<Stop>.Read(new StreamReader(fileInfo.OpenRead())));
            stops.CreateIndex("StopId");
            stops.CreateIndex("StopLatLon");
        }

        private static void ImportAgency(object param)
        {
            var paras = (object[]) param;
            var database = (MongoDatabase)paras[0];
            var fileInfo = (FileInfo)paras[1];
            ImportAgency(database, fileInfo);
        }

        private static void ImportAgency(MongoDatabase database, FileInfo fileInfo)
        {
            var agencies = database.GetCollection<Agency>("agency");
            agencies.InsertBatch(Associator<Agency>.Read(new StreamReader(fileInfo.OpenRead())));
        }


        //private void ImportAgency(object param)
        //{
        //    var paras = (object[])param;
        //    var database = (MongoDatabase)paras[0];
        //    var fileInfo = (FileInfo)paras[1];
        //    ImportAgency(database, fileInfo);
        //}

        //private void ImportAgency(MongoDatabase database, FileInfo fileInfo)
        //{
        //    var agencies = database.GetCollection<Agency>("agency");
        //    agencies.InsertBatch(Associator<Agency>.Read(new StreamReader(fileInfo.OpenRead())));
        //}
    }
}