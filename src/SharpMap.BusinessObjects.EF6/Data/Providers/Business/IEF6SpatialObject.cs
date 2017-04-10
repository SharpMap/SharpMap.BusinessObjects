using System.ComponentModel.DataAnnotations;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Interface for spatial entities in EF6
    /// </summary>
    /// <typeparam name="T">The type of the geometry/geogtraphy</typeparam>
    public interface IEF6SpatialObject<T>
    {
        uint Fid { get; }
        T DbGeometry { get; set; }
        IGeometry Geometry { get; set; }
    }
}