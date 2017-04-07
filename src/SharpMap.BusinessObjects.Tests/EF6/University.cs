using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using SharpMap.Data.Providers.Business;

namespace SharpMap.Business.Tests.EF6
{
    public class University : EF6SpatialGeometryObjectBase
    {
        private DbGeometry _dbGeometry;
        
        [NotMapped, BusinessObjectIdentifier]
        public override uint Fid { get { return (uint)Id; } }
        
        [Key]
        public int Id { get; set; }

        [Required, BusinessObjectAttribute]
        public string Name { get; set; }

        [Required]
        public override DbGeometry DbGeometry
        {
            get { return _dbGeometry; }
            set
            {
                if (value == _dbGeometry)
                    return;
                _dbGeometry = value;
                SetGeometry(_dbGeometry.ToGeometry());
            }
        }

        protected override void SetDbGeometry(DbGeometry dbGeometry)
        {
            _dbGeometry = dbGeometry;
        }
    }
}