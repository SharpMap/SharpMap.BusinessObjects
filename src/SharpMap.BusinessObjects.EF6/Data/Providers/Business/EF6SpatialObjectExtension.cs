using System;
using System.Data.Entity.Spatial;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Extension methods to deal with SqlServer geometries and geographies.
    /// </summary>
    public static class EF6SpatialObjectExtension
    {
        /// <summary>
        /// Method to convert <paramref name="self"/> to a <see cref="DbGeometry"/>
        /// </summary>
        /// <param name="self">The geometry to convert</param>
        /// <returns>The converted geometry</returns>
        public static DbGeometry ToDbGeometry(this IGeometry self)
        {
            if (self == null)
                return null;
            return DbGeometry.FromBinary(self.AsBinary());
        }

        /// <summary>
        /// Method to convert <paramref name="self"/> to a <see cref="IGeometry"/>
        /// </summary>
        /// <param name="self">The geometry to convert</param>
        /// <returns>The converted geometry</returns>
        public static IGeometry ToGeometry(this DbGeometry self)
        {
            if (self == null)
                return null;
            
            var reader = new WKBReader(NetTopologySuite.NtsGeometryServices.Instance);
            return reader.Read(self.AsBinary());
        }

        /// <summary>
        /// Method to convert <paramref name="self"/> to a <see cref="DbGeography"/>
        /// </summary>
        /// <param name="self">The geometry to convert</param>
        /// <returns>The converted geography</returns>
        public static DbGeography ToDbGeography(this IGeometry self)
        {
            if (self == null)
                return null;
            if (self.SRID != 4326)
                throw new ArgumentException();

            return DbGeography.FromBinary(self.AsBinary());
        }

        /// <summary>
        /// Method to convert <paramref name="self"/> to a <see cref="IGeometry"/>
        /// </summary>
        /// <param name="self">The geography to convert</param>
        /// <returns>The converted geometry</returns>
        public static IGeometry ToGeometry(this DbGeography self)
        {
            if (self == null)
                return null;

            var reader = new WKBReader(NetTopologySuite.NtsGeometryServices.Instance);
            return reader.Read(self.AsBinary());
        }
    }
}
