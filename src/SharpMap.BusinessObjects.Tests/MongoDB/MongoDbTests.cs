using System;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using Common.Logging;
using GeoAPI.Geometries;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using NUnit.Framework;
using SharpMap.Converters;
using SharpMap.Data.Providers.Business;
using SharpMap.Layers;

namespace SharpMap.Business.Tests.MongoDB
{
    /// <summary>
    /// 
    /// </summary>
    public class MongoDbTests
    {
        private MongoClient _client;

        private const string TestConnection = "mongodb://localhost";
        private const string TestDatabase = "SharpMap_PoiTest";
        private const string TestCollection = "Items";

        private const string StartMongoDbServerCommand = @"c:\program files\mongodb\3.4\server\bin\mongod.exe";
        private const string StartMongoDbServerArguments = @"-directoryperdb -dbpath d:\temp\mongodb\data --journal --noauth";

        static MongoClient GetClient()
        {
            try
            {
                var client = new MongoClient(TestConnection);
                var server = client.GetServer();
                foreach (var dbNames in server.GetDatabaseNames())
                {
                    LogManager.GetCurrentClassLogger().Debug(fmh => fmh("{0}", dbNames));
                }
                return client;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Debug(fmh => fmh("MongoDB Server not found or running"));
            }
            return null;
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            _client = GetClient();
            if (_client == null)
            {
                try
                {
                    System.Diagnostics.Process.Start(StartMongoDbServerCommand, StartMongoDbServerArguments);
                    Thread.Sleep(2000);
                }
                catch (Exception e)
                {
                    throw new IgnoreException("Failed to start MongoDB Server", e);
                }
                finally
                {
                    _client = GetClient();
                }
            }
            if (_client == null)
                throw new IgnoreException("Creation of MongoClient failed");

            var server = _client.GetServer();
            if (server.BuildInfo != null)
            {
                if (server.BuildInfo.Version < new Version(2, 4))
                    throw new IgnoreException("MongoDB server must have at least version 2.4");
            }

            if (server.DatabaseExists(TestDatabase))
                server.DropDatabase(TestDatabase);

            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            CreatePoIDatabase(server);
        }

        private static void CreatePoIDatabase(MongoServer server)
        {
            var db = server.GetDatabase(TestDatabase);
            if (!db.CreateCollection("Items").Ok)
                throw new IgnoreException("Faild to create collection items");

            //Assign the converter
            PoI.Converter = GeoJsonConverter.Converter2D;
            
            var col = db.GetCollection<PoI>("Items");
            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            for (uint oid = 1; oid <= 1000; oid++)
            {
                col.Insert(RndPoi(ref oid, factory));
            }
            


        }

        [Test]
        public void Test1()
        {
            MongoDbBusinessObjectSource<PoI, GeoJson2DCoordinates> repo = null;
            Assert.DoesNotThrow( () => repo =
                new PoIRepository(
                    GeoJsonConverter.Converter2D,
                    TestConnection, TestDatabase, TestCollection));

            Assert.IsNotNull(repo);
            Assert.AreEqual(1000, repo.Count);
        }

        [Test]
        public void Test2()
        {
            MongoDbBusinessObjectSource<PoI, GeoJson2DCoordinates> repo = null;
            Assert.DoesNotThrow(() => repo =
                new PoIRepository(
                    GeoJsonConverter.Converter2D,
                    TestConnection, TestDatabase, TestCollection));

            Assert.IsNotNull(repo);
            var extent = repo.GetExtents();
            extent = extent.Grow(-0.2*extent.Width, -0.2*extent.Height);
            Assert.Less(repo.Select(extent).Count(), 1000);
        }

        [Test]
        public void TestWithProvider()
        {
            MongoDbBusinessObjectSource<PoI, GeoJson2DCoordinates> repo = null;
            Assert.DoesNotThrow(() => repo =
                new PoIRepository(
                    GeoJsonConverter.Converter2D,
                    TestConnection, TestDatabase, TestCollection));

            var p = new BusinessObjectProvider<PoI>(TestCollection, repo);
            var vl = new VectorLayer(p.ConnectionID, p);

            var bl = new BusinessObjectLayer<PoI>(repo);

            var m = new Map();
            m.Layers.Add(vl);
            m.Layers.Add(bl);
            m.ZoomToExtents();
            m.GetMap().Save("MongoDB.PoI.png", ImageFormat.Png);
            m.Dispose();
        }


        #region Poi generation

        private static readonly Random RND = new Random(6658475);

        private static PoI RndPoi(ref uint oid, IGeometryFactory factory)
        {
            return new PoI
            {
                Id = oid,
                Label = string.Format("POI {0}", oid),
                Geometry = factory.CreatePoint(new Coordinate(RndX(), RndY()))
            };
        }

        private static double RndX()
        {
            return -180 + RND.NextDouble() * 360;
        }

        private static double RndY()
        {
            return -90 + RND.NextDouble() * 180;
        }

        #endregion

    }
}