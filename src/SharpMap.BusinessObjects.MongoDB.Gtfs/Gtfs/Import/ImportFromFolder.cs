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

        public static async Task Import(DirectoryInfo directory)
        {
            var client = GetClient();
            await client.DropDatabaseAsync(directory.Name);
            var database = client.GetDatabase(directory.Name);

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
        private delegate Task ImportToMongoDb(IMongoDatabase db, FileInfo fi);

        private static async Task ImportOptional(object param)
        {
            var paras = (object[])param;
            var fileInfo = (FileInfo[])paras[1];
            if (fileInfo != null && fileInfo.Length > 0)
            {
                var action = (ImportToMongoDb) paras[2];
                var database = (IMongoDatabase)paras[0];
                action(database, fileInfo[0]);
            }
        }

        private static async Task ImportFeedInfo(IMongoDatabase database, FileInfo getFeedInfo)
        {
            var feedInfo = database.GetCollection<FeedInfo>(getFeedInfo.Name);
            await feedInfo.InsertManyAsync(Associator<FeedInfo>.Read(new StreamReader(getFeedInfo.OpenRead())));
        }

        private static async Task ImportTransfer(IMongoDatabase database, FileInfo getTransfer)
        {
            var shape = database.GetCollection<Transfer>(getTransfer.Name);
            await shape.InsertManyAsync(Associator<Transfer>.Read(new StreamReader(getTransfer.OpenRead())));

            var bldr = Builders<Transfer>.IndexKeys;
            await shape.Indexes.CreateOneAsync(new CreateIndexModel<Transfer>(
                bldr.Combine(bldr.Ascending(t => t.FromStopId),
                    bldr.Ascending(t => t.ToStopId))));
        }

        private static async Task ImportFrequency(IMongoDatabase database, FileInfo getFrequency)
        {
            var shape = database.GetCollection<Frequency>(getFrequency.Name);
            await shape.InsertManyAsync(Associator<Frequency>.Read(new StreamReader(getFrequency.OpenRead())));
            await shape.Indexes.CreateOneAsync(new CreateIndexModel<Frequency>(
                Builders<Frequency>.IndexKeys.Ascending(t => t.TripId)));
        }

        private static async Task ImportShape(IMongoDatabase database, FileInfo getShape)
        {
            var shape = database.GetCollection<Shape>(getShape.Name);
            await shape.InsertManyAsync(Associator<Shape>.Read(new StreamReader(getShape.OpenRead())));
            var bldr = Builders<Shape>.IndexKeys;
            await shape.Indexes.CreateOneAsync(new CreateIndexModel<Shape>(
                bldr.Combine(bldr.Ascending(t => t.ShapeID),
                    bldr.Ascending(t => t.ShapePointSequence))));
        }

        private static async Task ImportFareRule(IMongoDatabase database, FileInfo getFareRule)
        {
            var fareRule = database.GetCollection<FareRule>(getFareRule.Name);
            await fareRule.InsertManyAsync(Associator<FareRule>.Read(new StreamReader(getFareRule.OpenRead())));
            var bldr = Builders<FareRule>.IndexKeys;
            await fareRule.Indexes.CreateOneAsync(new CreateIndexModel<FareRule>(
                bldr.Combine(bldr.Ascending(t => t.FareId), bldr.Ascending(t => t.RouteId))));
        }

        private static async Task ImportFareAttribute(IMongoDatabase database, FileInfo getFareAttribute)
        {
            var fareAttribute = database.GetCollection<FareAttribute>(getFareAttribute.Name);
            await fareAttribute.InsertManyAsync(Associator<FareAttribute>.Read(new StreamReader(getFareAttribute.OpenRead())));
            await fareAttribute.Indexes.CreateOneAsync(new CreateIndexModel<FareAttribute>(
                Builders<FareAttribute>.IndexKeys.Ascending(t => t.FareId)));
        }

        private static async Task ImportCalendarDate(IMongoDatabase database, FileInfo getCalenderDate)
        {
            var calendarDate = database.GetCollection<CalendarDate>("calendar_date");
            await calendarDate.InsertManyAsync(Associator<CalendarDate>.Read(new StreamReader(getCalenderDate.OpenRead())));
            await calendarDate.Indexes.CreateOneAsync(new CreateIndexModel<CalendarDate>(
                Builders<CalendarDate>.IndexKeys.Ascending(t => t.ServiceId)));
        }

        private static async Task ImportCalendar(IMongoDatabase database, FileInfo getFile)
        {
            var calendar = database.GetCollection<Calendar>("calendar");
            await calendar.InsertManyAsync(Associator<Calendar>.Read(new StreamReader(getFile.OpenRead())));
            await calendar.Indexes.CreateOneAsync(new CreateIndexModel<Calendar>(
                Builders<Calendar>.IndexKeys.Ascending(t => t.ServiceID)));
        }

        private static async Task ImportStopTimes(object param)
        {
            var paras = (object[])param;
            var database = (IMongoDatabase)paras[0];
            var fileInfo = (FileInfo)paras[1];
            await ImportStopTimes(database, fileInfo);
        }
        private static async Task ImportStopTimes(IMongoDatabase database, FileInfo getFile)
        {
            var stopTimes = database.GetCollection<StopTime>("stop_times");
            await stopTimes.InsertManyAsync(Associator<StopTime>.Read(new StreamReader(getFile.OpenRead())));
            var bldr = Builders<StopTime>.IndexKeys;
            await stopTimes.Indexes.CreateOneAsync(new CreateIndexModel<StopTime>(bldr.Ascending(t => t.TripId)));
            await stopTimes.Indexes.CreateOneAsync(new CreateIndexModel<StopTime>(bldr.Combine(
                bldr.Ascending(t => t.TripId), bldr.Ascending(t => t.StopId),
                bldr.Ascending(t => t.StopSequence))));
        }

        private static async Task ImportRoute(object param)
        {
            var paras = (object[])param;
            var database = (IMongoDatabase)paras[0];
            var fileInfo = (FileInfo)paras[1];
            await ImportRoute(database, fileInfo);
        }
        private static async Task ImportRoute(IMongoDatabase database, FileInfo fileInfo)
        {
            var routes = database.GetCollection<Route>("routes");
            await routes.InsertManyAsync(Associator<Route>.Read(new StreamReader(fileInfo.OpenRead())));
        }

        private static async Task ImportTrip(object param)
        {
            var paras = (object[])param;
            var database = (IMongoDatabase)paras[0];
            var fileInfo = (FileInfo)paras[1];
            await ImportTrip(database, fileInfo);
        }
        private static async Task ImportTrip(IMongoDatabase database, FileInfo fileInfo)
        {
            var routes = database.GetCollection<Trip>("trips");
            await routes.InsertManyAsync(Associator<Trip>.Read(new StreamReader(fileInfo.OpenRead())));
        }

        public static Regex Csv { get { return new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)"); } }

        private static async Task ImportStop(object param)
        {
            var paras = (object[]) param;
            var database = (IMongoDatabase) paras[0];
            var fileInfo = (FileInfo) paras[1];
            await ImportStop(database,fileInfo);
        }

        private static async Task ImportStop(IMongoDatabase database, FileInfo fileInfo)
        {
            var stops = database.GetCollection<Stop>("stops");
            await stops.InsertManyAsync(Associator<Stop>.Read(new StreamReader(fileInfo.OpenRead())));
            await stops.Indexes.CreateOneAsync(new CreateIndexModel<Stop>(Builders<Stop>.IndexKeys.Ascending(t => t.StopId)));
            await stops.Indexes.CreateOneAsync(new CreateIndexModel<Stop>(Builders<Stop>.IndexKeys.Ascending(t => t.StopLatLon)));
        }

        private static async Task ImportAgency(object param)
        {
            var paras = (object[]) param;
            var database = (IMongoDatabase)paras[0];
            var fileInfo = (FileInfo)paras[1];
            await ImportAgency(database, fileInfo);
        }

        private static async Task ImportAgency(IMongoDatabase database, FileInfo fileInfo)
        {
            var agencies = database.GetCollection<Agency>("agency");
            await agencies.InsertManyAsync(Associator<Agency>.Read(new StreamReader(fileInfo.OpenRead())));
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
