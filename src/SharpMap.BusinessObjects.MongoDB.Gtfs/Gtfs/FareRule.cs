using System.CodeDom;
using System.Security.AccessControl;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver.Linq;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    public class FareRule
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("fare_id")]
        public string FareId { get; set; }

        [BsonElement("route_id")]
        public string RouteId { get; set; }
    }
}