﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NUnit.Framework;
using SharpMap.Data.Providers.Business;
using SharpMap.Layers;
using SharpMap.Rendering.Business;

namespace SharpMap.Business.Tests.Memory
{
    [Serializable]
    public class LinkWithLoad
    {
        private static uint _lastId = 1;
        public LinkWithLoad()
        {
            ID = _lastId++;
        }

        [BusinessObjectIdentifier]
        public virtual uint ID { get; set; }
        [BusinessObjectGeometry]
        public virtual ILineString LineString { get; set; }
        [BusinessObjectAttribute]
        public virtual double[][] Load { get; set; }
    }

    [Serializable]
    public class LinkWithLoadRenderer : BusinessObjectToImageRenderer<LinkWithLoad>
    {
        private readonly OffsetCurveBuilder _offsetCurveBuilder = 
            new OffsetCurveBuilder(new PrecisionModel(), new BufferParameters());

        /// <summary>
        /// Gets or set a value indicating the Pen used to draw the link axis
        /// </summary>
        public Pen AxisPen { get; set; }

        /// <summary>
        /// Gets or sets a pen used to draw a frame around each loadstrip
        /// </summary>
        public Pen FramePen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the scale factor to compute the width of the strip
        /// </summary>
        public double Scale { get; set; }

        /// <summary>
        /// A dictionary for load brushes
        /// </summary>
        public IDictionary<int, Brush> LoadBrush { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the line should be simplified
        /// </summary>
        public bool Simplify { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the load strip offset from the axis
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public LinkWithLoadRenderer()
        {
            AxisPen = new Pen(Color.Black, 2);
            FramePen = new Pen(Color.Gainsboro, 1);
            Scale = 0.01;
            LoadBrush = new Dictionary<int, Brush> { { 0, Brushes.Green }, { 1, Brushes.Gold }, { 2, Brushes.OrangeRed } };
        }

        /// <summary>
        /// Method to render each individual business object
        /// </summary>
        /// <param name="businessObject">The business object to render</param>
        public override void Render(LinkWithLoad businessObject)
        {
            var g = (ILineString)Transformation(businessObject.LineString);

            // Draw the loads in positive direction
            RenderLoadStrips(g, businessObject.Load[0], Scale);

            // Draw the loads in negative direction
            RenderLoadStrips(g, businessObject.Load[1], -Scale);

            // Draw the axis
            if (AxisPen != null)
                Graphics.DrawLines(AxisPen, businessObject.LineString.TransformToImage(Map));
        }

        private void RenderLoadStrips(ILineString ls, double[] loads, double scale = 1)
        {
            // Get the coordinates
            var start = Simplify 
                ? NetTopologySuite.Simplify.DouglasPeuckerLineSimplifier.Simplify(ls.Coordinates, 2*Map.PixelSize)
                : ls.Coordinates;

            // Compute the initial offset
            if (Offset != 0d)
                start = RemoveSelfIntersections(_offsetCurveBuilder
                    .GetOffsetCurve(start, Math.Sign(scale)*Offset * Map.PixelSize), ls.Factory);

            // All strips
            for (var i = 0; i < loads.Length; i++)
            {
                // cycle if 
                if (loads[i] == 0) continue;
                
                // Compute the offset line
                var offset = RemoveSelfIntersections(_offsetCurveBuilder
                    .GetOffsetCurve(start, scale * loads[i]), ls.Factory);

                // Setup coordinate sequence
                var pts = new List<Coordinate>(start);
                pts.AddRange(offset.Reverse());
                pts.Add(start[0]);

                // Build the ring
                var ring = ls.Factory.CreateLinearRing(pts.ToArray());

                // Transform to image coordinates
                var gp = ring.TransformToImage(Map);

                // Draw the polygon
                Brush b;
                if (!LoadBrush.TryGetValue(i, out b))
                    b = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.HotPink, Color.Chartreuse);

                Graphics.FillPolygon(b, gp);

                if (FramePen != null)
                    Graphics.DrawPolygon(FramePen, gp);

                start = offset;
            }
        }

        //private ILineString GetOffsetCurve(ILineString lineString)
        //{
        //    return lineString.Factory.CreateLineString(
        //        _offsetCurveBuilder.GetOffsetCurve())
        //}

        private static Coordinate[] RemoveSelfIntersections(Coordinate[] offsetCurve, IGeometryFactory factory)
        {
            var ls = factory.CreateLineString(offsetCurve);
            if (ls.IsValid)
                return offsetCurve;

            var polygonizer = new NetTopologySuite.Operation.Polygonize.Polygonizer();
            polygonizer.Add(ls);
            var dangles = polygonizer.GetDangles();
            var sequencer = new NetTopologySuite.Operation.Linemerge.LineSequencer();
            sequencer.Add(dangles);
            System.Diagnostics.Debug.Assert(sequencer.IsSequenceable());
            return sequencer.GetSequencedLineStrings().Coordinates;

        }


    }

    public class BusinessObjectLayerTest
    {
        private List<LinkWithLoad> _linksWithLoads;

        [OneTimeSetUp]
        public void TestSetUp()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            var f = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(31467);
            _linksWithLoads = new List<LinkWithLoad>
            {
                new LinkWithLoad
                {
                    LineString = f.CreateLineString(new []{ new Coordinate(0, 0), new Coordinate(100, 400), new Coordinate(250, 400), new Coordinate(500, 500) }),
                    Load = new []{ new [] { 500d, 200, 700, 30 }, new [] { 300, 400d, 30, 700 }}
                },

                new LinkWithLoad
                {
                    LineString = f.CreateLineString(new []{ new Coordinate(500, 0), new Coordinate(100, 400) }),
                    Load = new []{ new [] { 50d, 20, 70, 3 }, new [] { 50d, 20, 70, 3 }}
                }
            };
        }

        [Test]
        public void TestRendering()
        {
            var s = new InMemoryBusinessObjectAccess<LinkWithLoad>();
            s.Insert(_linksWithLoads);

            var l = new BusinessObjectLayer<LinkWithLoad>(s);
            l.Renderer = new LinkWithLoadRenderer{ Scale = 0.05, Offset = 1.5 };

            var m = new Map(new Size(500, 300));
            m.Layers.Add(l);

            m.ZoomToExtents();
            m.Zoom *= 1.15;
            m.GetMap().Save("LinkWithLoadImage1.png", ImageFormat.Png);
            Console.WriteLine(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LinkWithLoadImage1.png")).AbsoluteUri);
            l.Renderer = null;
            m.GetMap().Save("LinkWithLoadImage2.png", ImageFormat.Png);
            Console.WriteLine(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LinkWithLoadImage2.png")).AbsoluteUri);
        }
    }
}