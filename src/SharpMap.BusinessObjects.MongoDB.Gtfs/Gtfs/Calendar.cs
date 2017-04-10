using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    /// <summary>
    /// A calendar class
    /// </summary>
    public class Calendar
    {
        /// <summary>Gets or sets a value indicating the service id</summary>
        /// <remarks>
        /// The service_id contains an ID that uniquely identifies a set of dates 
        /// when service is available for one or more routes. Each service_id value 
        /// can appear at most once in a calendar.txt file. This value is dataset 
        /// unique. It is referenced by the trips.txt file.
        /// </remarks>
        [BsonElement("service_id")]
        [BsonRequired]
        public string ServiceID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the service is valid for all mondays
        /// </summary>
        /// <remarks>
        /// The monday field contains a binary value that indicates whether the service is valid for all Mondays.
        /// <list type="Bullet">
        /// <item>A value of <value>true</value> indicates that service is available for all Mondays in the date range. 
        /// (The date range is specified using the start_date and end_date fields.)</item>
        /// <item>A value of <value>false</value> indicates that service is not available on Mondays in the date range.</item>
        /// </list>
        /// Note: You may list exceptions for particular dates, such as holidays, in the calendar_dates.txt file.
        /// </remarks>
        [BsonElement("monday")]
        [BsonRequired]
        public bool Monday { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the service is valid for all mondays
        /// </summary>
        /// <remarks>
        /// The tuesday field contains a binary value that indicates whether the service is valid for all tuesdays.
        /// <list type="Bullet">
        /// <item>A value of <value>true</value> indicates that service is available for all tuesdays in the date range. 
        /// (The date range is specified using the start_date and end_date fields.)</item>
        /// <item>A value of <value>false</value> indicates that service is not available on tuesdays in the date range.</item>
        /// </list>
        /// Note: You may list exceptions for particular dates, such as holidays, in the calendar_dates.txt file.
        /// </remarks>
        [BsonElement("tuesday")]
        [BsonRequired]
        public bool Tuesday { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the service is valid for all wednessdays
        /// </summary>
        /// <remarks>
        /// The wednessday field contains a binary value that indicates whether the service is valid for all wednessdays.
        /// <list type="Bullet">
        /// <item>A value of <value>true</value> indicates that service is available for all wednessdays in the date range. 
        /// (The date range is specified using the start_date and end_date fields.)</item>
        /// <item>A value of <value>false</value> indicates that service is not available on wednessdays in the date range.</item>
        /// </list>
        /// Note: You may list exceptions for particular dates, such as holidays, in the calendar_dates.txt file.
        /// </remarks>
        [BsonElement("wednessday")]
        [BsonRequired]
        public bool Wednessday { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the service is valid for all thursdays
        /// </summary>
        /// <remarks>
        /// The thursday field contains a binary value that indicates whether the service is valid for all thursdays.
        /// <list type="Bullet">
        /// <item>A value of <value>true</value> indicates that service is available for all thursdays in the date range. 
        /// (The date range is specified using the start_date and end_date fields.)</item>
        /// <item>A value of <value>false</value> indicates that service is not available on thursdays in the date range.</item>
        /// </list>
        /// Note: You may list exceptions for particular dates, such as holidays, in the calendar_dates.txt file.
        /// </remarks>
        [BsonElement("thursday")]
        [BsonRequired]
        public bool Thursday { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the service is valid for all fridays
        /// </summary>
        /// <remarks>
        /// The friday field contains a binary value that indicates whether the service is valid for all fridays.
        /// <list type="Bullet">
        /// <item>A value of <value>true</value> indicates that service is available for all Mondays in the date range. 
        /// (The date range is specified using the start_date and end_date fields.)</item>
        /// <item>A value of <value>false</value> indicates that service is not available on Mondays in the date range.</item>
        /// </list>
        /// Note: You may list exceptions for particular dates, such as holidays, in the calendar_dates.txt file.
        /// </remarks>
        [BsonElement("friday")]
        [BsonRequired]
        public bool Friday { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the service is valid for all saturdays
        /// </summary>
        /// <remarks>
        /// The saturday field contains a binary value that indicates whether the service is valid for all saturdays.
        /// <list type="Bullet">
        /// <item>A value of <value>true</value> indicates that service is available for all saturdays in the date range. 
        /// (The date range is specified using the start_date and end_date fields.)</item>
        /// <item>A value of <value>false</value> indicates that service is not available on saturdays in the date range.</item>
        /// </list>
        /// Note: You may list exceptions for particular dates, such as holidays, in the calendar_dates.txt file.
        /// </remarks>
        [BsonElement("saturday")]
        [BsonRequired]
        public bool Saturday { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the service is valid for all sundays
        /// </summary>
        /// <remarks>
        /// The sunday field contains a binary value that indicates whether the service is valid for all sundays.
        /// <list type="Bullet">
        /// <item>A value of <value>true</value> indicates that service is available for all sundays in the date range. 
        /// (The date range is specified using the start_date and end_date fields.)</item>
        /// <item>A value of <value>false</value> indicates that service is not available on sundays in the date range.</item>
        /// </list>
        /// Note: You may list exceptions for particular dates, such as holidays, in the calendar_dates.txt file.
        /// </remarks>
        [BsonElement("sunday")]
        [BsonRequired]
        public bool Sunday { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the start date of this calender item
        /// </summary>
        /// <remarks>The start_date field contains the start date for the service.
        /// The start_date field's value should be in YYYYMMDD format.
        /// </remarks>
        [BsonElement("start_date")]
        [BsonRequired]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the start date of this calender item
        /// </summary>
        /// <remarks>The end_date field contains the end date for the service. This date is included in the service interval.
        /// The end_date field's value should be in YYYYMMDD format.
        /// </remarks>
        [BsonElement("end_date")]
        [BsonRequired]
        public DateTime EndDate { get; set; }
    }
}