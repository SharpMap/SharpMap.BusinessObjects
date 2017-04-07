using System.Collections.Generic;
using GeoAPI.Geometries;
using SharpMap.Data.Providers.Business;

namespace SharpMap.Business.Tests.Memory
{
    public class PointOfInterest
    {
        [BusinessObjectIdentifier()]
        public uint ID { get; set; }

        [BusinessObjectGeometry]
        public IGeometry Geometry { get; set; }

        [BusinessObjectAttribute]
        public string Kind { get; set; }

        [BusinessObjectAttribute]
        public string Address { get; set; }

        [BusinessObjectAttribute(AllowNull = true)]
        public List<string> Comments { get; set; }
    }
}
