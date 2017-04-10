using System.Data.Entity.Spatial;

namespace SharpMap.Data.Providers.Business
{
    public interface IEF6SpatialGeographyObject 
        : IEF6SpatialObject<DbGeography>
    {
    }
}