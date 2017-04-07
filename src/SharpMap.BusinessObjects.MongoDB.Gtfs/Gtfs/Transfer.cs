using MongoDB.Bson.Serialization.Attributes;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    /// <summary>
    /// Trip planners normally calculate transfer points based on the relative proximity 
    /// of stops in each route. For potentially ambiguous stop pairs, or transfers where 
    /// you want to specify a particular choice, use transfers.txt to define additional 
    /// rules for making connections between routes.
    /// </summary>
    public class Transfer
    {
        /// <summary>
        /// Contains a stop ID that identifies a stop or station where a connection between 
        /// routes begins. Stop IDs are referenced from the <see cref="Stop"/>s file. If the 
        /// stop ID refers to a station that contains multiple stops, this transfer rule 
        /// applies to all stops in that station.
        /// </summary>
        [BsonElement("from_stop_id")]
        [BsonRequired]
        public string FromStopId { get; set; }

        /// <summary>
        /// Contains a stop ID that identifies a stop or station where a connection between 
        /// routes ends. Stop IDs are referenced from the <see cref="Stop"/>s file. If the 
        /// stop ID refers to a station that contains multiple stops, this transfer rule 
        /// applies to all stops in that station.
        /// </summary>
        [BsonElement("to_stop_id")]
        [BsonRequired]
        public string ToStopId { get; set; }

        /// <summary>
        /// Specifies the type of connection for the specified (<see cref="FromStopId"/>, 
        /// <see cref="ToStopId"/>) pair
        /// </summary>
        [BsonElement("transfer_type")]
        [BsonRequired]
        public TransferType TransferType { get; set; }

        /// <summary>
        /// When a connection between routes requires an amount of time between arrival and 
        /// departure (<value>TransferType.LimitedTime</value>), this field defines the amount 
        /// of time that must be available in an itinerary to permit a transfer between routes 
        /// at these stops. The <see cref="MinTransferTime"/> must be sufficient to permit a 
        /// typical rider to move between the two stops, including buffer time to allow for 
        /// schedule variance on each route.
        /// <para/>
        /// The <see cref="MinTransferTime"/> value must be entered in seconds, and must be a 
        /// non-negative integer.
        /// </summary>
        [BsonElement("min_transfer_time")]
        public int MinTransferTime { get; set; }
    }
}