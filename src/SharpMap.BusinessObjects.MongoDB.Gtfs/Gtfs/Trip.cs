using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    public class Trip
    {
        /// <summary>
        /// The route_id field contains an ID that uniquely identifies a route. 
        /// This value is referenced from the routes.txt file.
        /// </summary>
        [BsonRequired]
        [BsonElement("route_id")] 
        public string RouteId { get; set; }

        /// <summary>
        /// The service_id contains an ID that uniquely identifies a set of dates when 
        /// service is available for one or more routes. 
        /// This value is referenced from the calendar.txt or calendar_dates.txt file.
        /// </summary>
        [BsonRequired]
        [BsonElement("service_id")]
        public string ServiceId { get; set; }

        /// <summary>
        /// The trip_id field contains an ID that identifies a trip. The trip_id is dataset unique.
        /// </summary>
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRequired]
        [BsonElement("trip_id")]
        public string TripId { get; set; }

        /// <summary>
        /// The trip_headsign field contains the text that appears on a sign 
        /// that identifies the trip's destination to passengers. Use this 
        /// field to distinguish between different patterns of service in the 
        /// same route. 
        /// If the headsign changes during a trip, you can override the trip_headsign 
        /// by specifying values for the the stop_headsign field in stop_times.txt.
        /// </summary>
        [BsonElement("trip_headsign")]
        public string TripHeadsign { get; set; }

        /// <summary>
        /// The trip_short_name field contains the text that appears in schedules 
        /// and sign boards to identify the trip to passengers, for example, to 
        /// identify train numbers for commuter rail trips. If riders do not 
        /// commonly rely on trip names, please leave this field blank.
        /// <para/>
        /// A trip_short_name value, if provided, should uniquely identify a trip 
        /// within a service day; it should not be used for destination names or 
        /// limited/express designations.
        /// </summary>
        [BsonElement("trip_short_name")]
        public string TripShortName { get; set; }

        /// <summary>
        /// The direction_id field contains a binary value that indicates the 
        /// direction of travel for a trip. Use this field to distinguish between 
        /// bi-directional trips with the same route_id. This field is not used 
        /// in routing; it provides a way to separate trips by direction when 
        /// publishing time tables. You can specify names for each direction 
        /// with the trip_headsign field.
        /// <list type="Table">
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>0</term><description>travel in one direction (e.g. outbound travel)</description></item>
        /// <item><term>1</term><description>travel in the opposite direction (e.g. inbound travel)</description></item>
        /// </list>
        /// </summary>
        [BsonIgnoreIfNull]
        [BsonElement("direction_id")]
        public int? DirectionId { get; set; }

        /// <summary>
        /// The block_id field identifies the block to which the trip belongs.  
        /// A block consists of two or more sequential trips made using the 
        /// same vehicle, where a passenger can transfer from one trip to the 
        /// next just by staying in the vehicle. The block_id must be referenced 
        /// by two or more trips in trips.txt.
        /// </summary>
        [BsonIgnoreIfNull]
        [BsonElement("block_id")]
        public string BlockId { get; set; }
        
        /// <summary>
        /// The shape_id field contains an ID that defines a shape for the trip. 
        /// This value is referenced from the shapes.txt file. The shapes.txt 
        /// file allows you to define how a line should be drawn on the map to 
        /// represent a trip.
        /// </summary>
        [BsonIgnoreIfNull]
        [BsonElement("shape_id")]
        public string ShapeId { get; set; }

        /// <summary>
        /// <list type="Table">
        /// <listheader><term>Value</term><description>indicates that</description></listheader>
        /// <item><term>0 (or null)</term><description>there is no accessibility information for the trip</description></item>
        /// <item><term>1</term><description>the vehicle being used on this particular trip can accommodate at least one rider in a wheelchair</description></item>
        /// <item><term>2</term><description>no riders in wheelchairs can be accommodated on this trip</description></item>
        /// </list>
        /// </summary>
        [BsonIgnoreIfNull]
        [BsonElement("wheelchair_accessible")]
        public int? WheelchairAccessible { get; set; }

        /// <summary>
        /// <list type="Table">
        /// <listheader><term>Value</term><description>indicates that</description></listheader>
        /// <item><term>0 (or null)</term><description>there is no bike information for the trip</description></item>
        /// <item><term>1</term><description>the vehicle being used on this particular trip can accommodate at least one bicycle</description></item>
        /// <item><term>2</term><description>no bicycles are allowed on this trip</description></item>
        /// </list>
        /// </summary>
        [BsonIgnoreIfNull]
        [BsonElement("bikes_allowed")]
        public int? BikesAllowed { get; set; }
    }
}