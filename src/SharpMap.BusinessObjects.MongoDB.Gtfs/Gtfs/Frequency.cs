using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    /// <summary>
    /// Frequencies are used when a line does not have set arrival and departure times, 
    /// but instead services run with a set interval. Frequencies are defined in the file 
    /// frequencies.txt, and are associated with a Trip definition. It is intended to 
    /// represent schedules that don't have a fixed list of stop times.<para/>
    /// When trips are defined in frequencies.txt, the trip planner ignores the absolute 
    /// values of the <see cref="StopTime.ArrivalTime"/> and <see cref="StopTime.DepartureTime"/> 
    /// fields for those trips in <see cref="StopTime"/>s. Instead, the <see cref="StopTime"/>s 
    /// table defines the sequence of stops and the time difference between each stop.
    /// </summary>
    public class Frequency
    {
        /// <summary>
        /// Contains an ID that identifies a trip on which the specified frequency 
        /// of service applies. Trip IDs are referenced from the <see cref="Trip"/>s file.
        /// </summary>
        [BsonElement("trip_id")]
        [BsonRequired]
        public string TripId { get; set; }

        /// <summary>
        /// Specifies the time at which service begins with the specified frequency. 
        /// The time is measured from "noon minus 12h" (effectively midnight, except for days 
        /// on which daylight savings time changes occur) at the beginning of the service date. 
        /// For times occurring after midnight, enter the time as a value greater than 24:00:00 
        /// in HH:MM:SS local time for the day on which the trip schedule begins. For example, 25:35:00.
        /// </summary>
        [BsonElement("start_time")]
        [BsonRequired]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Indicates the time at which service changes to a different frequency (or ceases) 
        /// at the first stop in the trip. The time is measured from "noon minus 12h" (effectively 
        /// midnight, except for days on which daylight savings time changes occur) at the beginning 
        /// of the service date. For times occurring after midnight, enter the time as a value greater
        /// than 24:00:00 in HH:MM:SS local time for the day on which the trip schedule begins. For 
        /// example, 25:35:00.
        /// </summary>
        [BsonElement("end_time")]
        [BsonRequired]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Indicates the time between departures from the same stop(headway) for this trip type, 
        /// during the time interval specified by <see cref="StartTime"/> and <see cref="EndTime"/>.The headway value must 
        /// be entered in seconds.<para/>
        /// Periods in which headways are defined (the rows in <see cref="Frequency"/>s file) shouldn't overlap 
        /// for the same trip, because it's hard to determine what should be inferred from two 
        /// overlapping headways.
        /// </summary>
        [BsonElement("headway_secs")]
        [BsonRequired]
        public int HeadwaySeconds { get; set; }

        /// <summary>
        /// Determines if frequency-based trips should be exactly scheduled based on the 
        /// specified headway information. <para/>
        /// The value of exact_times must be the same for all frequencies.txt rows with the same 
        /// trip_id. If exact_times is <value>ExactTimes.Yes</value> and a <see cref="Frequency"/> 
        /// row has a <see cref="StartTime"/> equal to <see cref="EndTime"/>, no trip must be scheduled.
        /// <para/>
        /// When exact_times is <value>ExactTimes.Yes</value>, care must be taken to choose an 
        /// <see cref="EndTime"/> value that is greater than the last desired trip start time but 
        /// less than the last desired trip <see cref="StartTime"/> + <see cref="HeadwaySeconds"/>.
        /// </summary>
        [BsonElement("exact_times")]
        public ExactTimes ExactTimes { get; set; } 
    }
}