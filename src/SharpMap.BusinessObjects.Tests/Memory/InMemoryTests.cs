using System.Drawing;
using System.Drawing.Imaging;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Data.Providers.Business;
using SharpMap.Layers;

namespace SharpMap.Business.Tests.Memory
{
    [TestFixture]
    public class InMemoryTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
        }

        [Test]
        public void TestConstruction()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));
            
            Assert.AreEqual(data.Count, p.GetFeatureCount());
        }

        [Test]
        public void TestGetFeature()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            FeatureDataRow row = null;
            Assert.DoesNotThrow(() => row = p.GetFeature(1));
            Assert.IsNotNull(row);
            Assert.IsNotNull(row.Geometry);
            Assert.AreEqual(OgcGeometryType.Point, row.Geometry.OgcGeometryType);
            Assert.AreEqual(new Coordinate(0, 0), row.Geometry.Coordinate);

            var bop = ((BusinessObjectProvider<PointOfInterest>) p).Source;
            var bo = bop.Select(1);
            
            Assert.AreEqual(bo.ID, row["ID"]);
            Assert.AreEqual(bo.Geometry, row.Geometry);
            Assert.AreEqual(bo.Address, row["Address"]);
        }

        [Test]
        public void TestAttributeSelection()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            FeatureDataRow row = null;
            Assert.DoesNotThrow(() => row = p.GetFeature(4));
            Assert.IsNotNull(row);
            Assert.IsNotNull(row.Geometry);
            Assert.AreEqual(OgcGeometryType.Point, row.Geometry.OgcGeometryType);
            Assert.AreEqual(new Coordinate(0, 1), row.Geometry.Coordinate);

            var bop = ((BusinessObjectProvider<PointOfInterest>)p).Source;
            // select single business objects
            var bo = bop.Find(b => b.Address == "lc");

            Assert.NotNull(bo);
            Assert.AreEqual(bo.ID, row["ID"]);
            Assert.AreEqual(bo.Geometry, row.Geometry);
            Assert.AreEqual(bo.Address, row["Address"]);
        }

        [Test]
        public void TestAttributeSelectionMulti()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            var bop = ((BusinessObjectProvider<PointOfInterest>)p).Source;
            // select 5 business objects
            PointOfInterest[] pts;
            pts= bop.FindAll(b => b.ID < 6 && b.Kind == "African food");
            Assert.NotNull(pts);
            Assert.AreEqual(pts.Length, 5);
        }

        [Test]
        public void TestAsReadOnly()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            var bop = ((BusinessObjectProvider<PointOfInterest>)p).Source;
            // select all business objects
            PointOfInterest[] pts;
            pts = bop.AsReadOnly();
            Assert.NotNull(pts);
            Assert.AreEqual(pts.Length, 9);
        }

        [Test]
        public void TestAttributeUpdate()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            var bop = ((BusinessObjectProvider<PointOfInterest>)p).Source;
            // update single business object
            var bo = bop.Find(b => b.Address == "lc");
            Assert.AreEqual(bo.Kind, "African food");
            bo.Kind = "Indian food";

            FeatureDataRow row = null;
            Assert.DoesNotThrow(() => row = p.GetFeature(4));
            Assert.IsNotNull(row);
            Assert.AreEqual(bo.Kind , row["Kind"]);
        }

        [Test]
        public void TestAttributeDelete()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            var bop = ((BusinessObjectProvider<PointOfInterest>)p).Source;

            var cnt = bop.Count;
            var env = bop.GetExtents();
            // delete 3 features on RHS
            bop.Delete(b => b.Address == "rl" || b.Address == "cr" || b.Address == "tr");

            Assert.AreEqual(bop.Count, cnt - 3);
            Assert.That(env.Width == 2 && env.Height == 2);
            Assert.That(bop.GetExtents().Width == 1 && bop.GetExtents().Height == 2);
        }

        [Test]
        public void TestBusinessObjectInsert()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            var bop = ((BusinessObjectProvider<PointOfInterest>)p).Source;

            var cnt = bop.Count;
            var env = bop.GetExtents();

            // insert single feature outside of existing extents
            var f = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            var coord = new Coordinate(env.Right() + 10, env.Top() + 10);
            PointOfInterest bo = new PointOfInterest { ID = 10, Address = "tr++", Kind = "Thai food", Geometry = f.CreatePoint(coord) };
            bop.Insert(bo);

            Assert.AreEqual(bop.Count, cnt + 1);
            Assert.That(bop.GetExtents().Contains(coord));
        }
        [Test]
        public void TestGetExtetnts()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            var e = p.GetExtents();

            var bop = ((BusinessObjectProvider<PointOfInterest>)p).Source;
            var e2 = bop.GetExtents();

            Assert.AreEqual(e, e2);
            Assert.AreEqual(new Envelope(0, 2, 0, 2), e);

        }

        [Test]
        public void TestExecuteFeatureQuery()
        {
            IProvider p = null;
            var data = PointsOfInterest.Create();
            Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

            var fds = new FeatureDataSet();
            p.ExecuteIntersectionQuery(new Envelope(0.5, 1.5, 0.5, 1.5), fds);
            Assert.AreEqual(1, fds.Tables.Count);
            Assert.AreEqual("PointOfInterest", fds.Tables[0].TableName);
            Assert.AreEqual(1, fds.Tables[0].Rows.Count);
            Assert.AreEqual(5, fds.Tables[0].Rows[0]["ID"]);
            Assert.AreEqual(new Coordinate(1, 1), ((FeatureDataRow)fds.Tables[0].Rows[0]).Geometry.Coordinate);
        }

        [Test]
        public void TestGetMap()
        {
            using (var m = new Map(new Size(300, 300)))
            {
                m.BackColor = Color.White;
                
                IProvider p = null;
                var data = PointsOfInterest.Create();
                Assert.DoesNotThrow(() => p = BusinessObjectProvider.Create(data));

                m.Layers.Add(new VectorLayer("POIs", p));
                var ll = new LabelLayer("POIsLabel");
                ll.DataSource = p;
                ll.LabelStringDelegate = (fdr) => string.Format("{0} - {1}", fdr["ID"], fdr["Address"]);
                m.Layers.Add(ll);

                m.ZoomToExtents();
                m.Zoom *= 1.1;

                using (var img = m.GetMap())
                {
                    img.Save("PoiImage.png", ImageFormat.Png);
                }
            }
            
        }
    }
}