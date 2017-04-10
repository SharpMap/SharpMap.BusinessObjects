using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.Business
{
    public abstract class EF6SpatialGeometryObjectBase : IEF6SpatialGeometryObject
    {
        private IGeometry _geometry;

        /// <summary>
        /// Gets or sets the Feature Id
        /// </summary>
        [BusinessObjectIdentifier]
        public abstract uint Fid { get; }

        /// <summary>
        /// Gets or sets the DbGeometry object
        /// </summary>
        public abstract DbGeometry DbGeometry { get; set; }

        /// <summary>
        /// Gets or sets the geometry object
        /// </summary>
        [NotMapped, BusinessObjectGeometry]
        public IGeometry Geometry
        {
            get { return _geometry; }
            set
            {
                if (ReferenceEquals(_geometry, value))
                    return;

                _geometry = value;
                DbGeometry = _geometry.ToDbGeometry();
            }
        }

        /// <summary>
        /// Method to set the <see cref="DbGeometry"/> object
        /// </summary>
        /// <param name="dbGeometry">The DbGeometry</param>
        protected abstract void SetDbGeometry(DbGeometry dbGeometry);

        /// <summary>
        /// Method to set the <see cref="Geometry"/> object.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        protected void SetGeometry(IGeometry geometry)
        {
            _geometry = geometry;
        }
    }
}