using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    /// <summary>
    /// Indicates whether service is available on the date specified in the date field
    /// </summary>
    public enum ExceptionType
    {
        /// <summary>
        /// Indicates that service has been added for the specified date.
        /// </summary>
        Add = 1,

        /// <summary>
        /// Indicates that service has been removed for the specified date
        /// </summary>
        Remove = 2
    }

    /// <summary>
    /// If there are specific days when a trip is not available, such as holidays, you should define these 
    /// in the <see cref="CalendarDate"/>s file. You can use this to define exceptional days when the Trip 
    /// is operated, as well as when it is not operated. For example, you may have special services that are 
    /// only operated on a public holiday, and they would be defined as unavailable (in calendar.txt) and 
    /// as available on the holiday (in calendar_dates.txt).
    /// <para/>
    /// The <see cref="CalendarDate"/>s table allows you to explicitly activate or disable service IDs by date.
    /// You can use it in two ways.
    /// <list type="bullet">
    /// <item>Recommended:<br/>Use <see cref="CalendarDate"/>s in conjunction with <see cref="Calendar"/>, 
    /// where <see cref="CalendarDate"/>s define any exceptions to the default service categories defined in
    /// the <see cref="Calendar"/> file. If your service is generally regular, with a few changes on explicit 
    /// dates (for example, to accomodate special event services, or a school schedule), this is a good 
    /// approach.</item>
    /// <item>Alternate:<br/>Omit <see cref="Calendar"/>, and include ALL dates of service in <see cref="CalendarDate"/>s.
    /// If your schedule varies most days of the month, or you want to programmatically output service dates 
    /// without specifying a normal weekly schedule, this approach may be preferable.</item>
    /// </list>
    /// </summary>
    public class CalendarDate
    {
        /// <summary>
        /// Contains an ID that uniquely identifies a set of dates when a service exception is available 
        /// for one or more routes. Each (<see cref="ServiceId"/>, <see cref="Date"/>) pair can only appear 
        /// once in <see cref="CalendarDate"/>. If a <see cref="ServiceId"/> value appears in both the 
        /// <see cref="Calendar"/> and <see cref="CalendarDate"/>s files, the information in <see cref="CalendarDate"/>s
        ///  modifies the service information specified in <see cref="Calendar"/>. This field is referenced by 
        /// the <see cref="Trip"/>s file.
        /// </summary>
        [BsonRequired]
        [BsonElement("service_id")]
        public uint ServiceId { get; set; }

        /// <summary>
        /// Specifies a particular date when service availability is different than the norm. You can use 
        /// the exception_type field to indicate whether service is available on the specified date.<para/>
        /// The date field's value should be in YYYYMMDD format.
        /// </summary>
        [BsonRequired]
        [BsonElement("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Indicates whether service is available on the date specified in the date field
        /// </summary>
        [BsonRequired]
        [BsonElement("exception_type")]
        public ExceptionType ExceptionType { get; set; }
    }
}