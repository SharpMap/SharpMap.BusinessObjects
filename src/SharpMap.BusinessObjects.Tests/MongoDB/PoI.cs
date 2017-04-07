using GeoAPI.Geometries;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver.GeoJsonObjectModel;
using SharpMap.Converters;
using SharpMap.Data.Providers.Business;

namespace SharpMap.Business.Tests.MongoDB
{
    public class PoI//<T> where T : GeoJsonCoordinates
    {
        private IGeometry _geometry;
        public static GeoJsonConverter<GeoJson2DCoordinates> Converter;

        [BusinessObjectIdentifier]
        [BsonId(IdGenerator = typeof (ZeroIdChecker<uint>))]
        public uint Id { get; set; }

        [BusinessObjectAttribute(Ordinal = 1)]
        public string Label { get; set; }

        [BusinessObjectGeometry]
        [BsonIgnore]
        public IGeometry Geometry
        {
            get { return _geometry ?? (_geometry = Converter.ToPoint(BsonGeometry)); }
            set
            {
                _geometry = value;
                BsonGeometry = Converter.ToPoint(value);
            }
        }

        [BusinessObjectAttribute(Ignore = true)]
        public GeoJsonPoint<GeoJson2DCoordinates> BsonGeometry { get; set; }

    }
}