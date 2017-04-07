using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    public class FareAttribute
    {
        /// <summary>
        /// Contains an ID that uniquely identifies a fare class. The fare_id is dataset unique.
        /// </summary>
        [BsonId]
        [BsonRequired]
        [BsonElement("fare_id")]
        public string FareId { get; set; }

        /// <summary>
        /// Contains the fare price, in the unit specified by <see cref="CurrencyType"/>.
        /// </summary>
        [BsonRequired]
        [BsonElement("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Defines the currency used to pay the fare. Please use the ISO 4217 
        /// alphabetical currency codes which can be found at the following URL: 
        /// <see href="http://en.wikipedia.org/wiki/ISO_4217"/>.
        /// </summary>
        [BsonRequired]
        [BsonElement("currency_type")]
        public string CurrencyType { get; set; }

        [BsonRequired]
        [BsonElement("payment_method")]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Specifies the number of transfers permitted on this fare.
        /// </summary>
        [BsonElement("transfers")]
        public int? Transfers { get; set; }

        [BsonElement("transfer_duration")]
        public TimeSpan? TransferDuration { get; set; }
    }
}