using System.Data.Entity.Spatial;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Interface for spatial entities using <see cref="DbGeography"/> as geometry
    /// </summary>
    public interface IEF6SpatialGeometryObject 
        : IEF6SpatialObject<DbGeometry>
    {
    }
}