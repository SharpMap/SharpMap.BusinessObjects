using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    public class StopTime
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId Oid { get; set; }

        /// <summary>
        /// The trip_id field contains an ID that identifies a trip. This value is referenced from the trips.txt file.
        /// </summary>
        [BsonElement("trip_id")]
        [BsonRequired]
        public string TripId { get; set; }

        /// <summary>
        /// The arrival_time specifies the arrival time at a specific stop for a specific trip on a route. 
        /// The time is measured from "noon minus 12h" (effectively midnight, except for days on which 
        /// daylight savings time changes occur) at the beginning of the service date. For times occurring after 
        /// midnight on the service date, enter the time as a value greater than 24:00:00 in HH:MM:SS local time 
        /// for the day on which the trip schedule begins. If you don't have separate times for arrival and 
        /// departure at a stop, enter the same value for arrival_time and departure_time.
        /// <para/>
        /// If this stop isn't a time point, use an empty string value for the arrival_time and departure_time 
        /// fields. Stops without arrival times will be scheduled based on the nearest preceding timed stop. 
        /// To ensure accurate routing, please provide arrival and departure times for all stops that are time 
        /// points. Do not interpolate stops.
        /// <para/>
        /// You must specify arrival and departure times for the first and last stops in a trip.
        /// <para/>
        /// Times must be eight digits in HH:MM:SS format (H:MM:SS is also accepted, if the hour begins with 0).
        /// Do not pad times with spaces. The following columns list stop times for a trip and the proper way to 
        /// express those times in the arrival_time field:
        /// <list type="Table">
        /// <listheader><term>Time</term><description>arrival_time value</description></listheader>
        /// <item><term>08:10:00 A.M.</term><description>08:10:00 or 8:10:00</description></item>
        /// <item><term>01:05:00 P.M.</term><description>13:05:00</description></item>
        /// <item><term>07:40:00 P.M.</term><description>19:40:00</description></item>
        /// <item><term>01:55:00 A.M.</term><description>25:55:00</description></item>
        /// </list>
        /// Note: Trips that span multiple dates will have stop times greater than 24:00:00. For example, 
        /// if a trip begins at 10:30:00 p.m. and ends at 2:15:00 a.m. on the following day, the stop times 
        /// would be 22:30:00 and 26:15:00. Entering those stop times as 22:30:00 and 02:15:00 would not 
        /// produce the desired results.
        /// </summary>
        [BsonElement("arrival_time")]
        [BsonRequired]
        [BsonTimeSpanOptions(BsonType.Int32, TimeSpanUnits.Seconds)]
        public TimeSpan ArrivalTime { get; set; }

        /// <summary>
        /// The departure_time specifies the departure time from a specific stop for a specific trip on a route. 
        /// The time is measured from "noon minus 12h" (effectively midnight, except for days on which 
        /// daylight savings time changes occur) at the beginning of the service date. For times occurring after 
        /// midnight on the service date, enter the time as a value greater than 24:00:00 in HH:MM:SS local time 
        /// for the day on which the trip schedule begins. If you don't have separate times for arrival and 
        /// departure at a stop, enter the same value for arrival_time and departure_time.
        /// <para/>
        /// If this stop isn't a time point, use an empty string value for the arrival_time and departure_time 
        /// fields. Stops without arrival times will be scheduled based on the nearest preceding timed stop. 
        /// To ensure accurate routing, please provide arrival and departure times for all stops that are time 
        /// points. Do not interpolate stops.
        /// <para/>
        /// You must specify arrival and departure times for the first and last stops in a trip.
        /// <para/>
        /// Times must be eight digits in HH:MM:SS format (H:MM:SS is also accepted, if the hour begins with 0).
        /// Do not pad times with spaces. The following columns list stop times for a trip and the proper way to 
        /// express those times in the departure_time field:
        /// <list type="Table">
        /// <listheader><term>Time</term><description>arrival_time value</description></listheader>
        /// <item><term>08:10:00 A.M.</term><description>08:10:00 or 8:10:00</description></item>
        /// <item><term>01:05:00 P.M.</term><description>13:05:00</description></item>
        /// <item><term>07:40:00 P.M.</term><description>19:40:00</description></item>
        /// <item><term>01:55:00 A.M.</term><description>25:55:00</description></item>
        /// </list>
        /// Note: Trips that span multiple dates will have stop times greater than 24:00:00. For example, 
        /// if a trip begins at 10:30:00 p.m. and ends at 2:15:00 a.m. on the following day, the stop times 
        /// would be 22:30:00 and 26:15:00. Entering those stop times as 22:30:00 and 02:15:00 would not 
        /// produce the desired results.
        /// </summary>
        [BsonElement("departure_time")]
        [BsonRequired]
        [BsonTimeSpanOptions(BsonType.Int32, TimeSpanUnits.Seconds)]
        public TimeSpan DepartureTime { get; set; }

        /// <summary>
        /// The stop_id field contains an ID that uniquely identifies a stop. Multiple routes may use the same stop. 
        /// The stop_id is referenced from the stops.txt file. If location_type is used in stops.txt, all stops 
        /// referenced in stop_times.txt must have location_type of 0.
        /// <para/>
        /// Where possible, stop_id values should remain consistent between feed updates. In other words, stop A 
        /// with stop_id 1 should have stop_id 1 in all subsequent data updates. If a stop is not a time point, 
        /// enter blank values for arrival_time and departure_time.
        /// </summary>
        [BsonRequired]
        [BsonElement("stop_id")]
        public string StopId { get; set; }

        /// <summary>
        /// The stop_sequence field identifies the order of the stops for a particular trip. The values for 
        /// stop_sequence must be non-negative integers, and they must increase along the trip.
        /// <para/>
        /// For example, the first stop on the trip could have a stop_sequence of 1, the second stop on the 
        /// trip could have a stop_sequence of 23, the third stop could have a stop_sequence of 40, and so on.
        /// </summary>
        [BsonRequired]
        [BsonElement("stop_sequence")]
        public int StopSequence { get; set; }

        /// <summary>
        /// The stop_headsign field contains the text that appears on a sign that identifies the trip's destination
        /// to passengers. Use this field to override the default trip_headsign when the headsign changes between 
        /// stops. If this headsign is associated with an entire trip, use trip_headsign instead.
        /// <para/>
        /// See a Google Maps screenshot highlighting the headsign.
        /// </summary>
        [BsonElement("stop_headsign")]
        [BsonIgnoreIfNull]
        public string StopHeadsign { get; set; }

        /// <summary>
        /// The pickup_type field indicates whether passengers are picked up at a stop as part of the normal schedule
        /// or whether a pickup at the stop is not available. This field also allows the transit agency to indicate 
        /// that passengers must call the agency or notify the driver to arrange a pickup at a particular stop. 
        /// Valid values for this field are:
        /// <list type="Table">
        /// <item><term>0</term><description>Regularly scheduled pickup</description></item>
        /// <item><term>1</term><description>No pickup available</description></item>
        /// <item><term>2</term><description>Must phone agency to arrange pickup</description></item>
        /// <item><term>3</term><description>Must coordinate with driver to arrange pickup</description></item>
        /// </list>
        /// The default value for this field is 0.
        /// </summary>
        [BsonElement("pickup_type")]
        [BsonIgnoreIfDefault]
        public StopType PickupType { get; set; }

        /// <summary>
        /// The drop_off_type field indicates whether passengers are dropped off at a stop as part of the normal schedule 
        /// or whether a drop off at the stop is not available. This field also allows the transit agency to indicate 
        /// that passengers must call the agency or notify the driver to arrange a pickup at a particular stop. 
        /// Valid values for this field are:
        /// <list type="Table">
        /// <item><term>0</term><description>Regularly scheduled drop off</description></item>
        /// <item><term>1</term><description>No drop off available</description></item>
        /// <item><term>2</term><description>Must phone agency to arrange drop off</description></item>
        /// <item><term>3</term><description>Must coordinate with driver to arrange drop off</description></item>
        /// </list>
        /// The default value for this field is 0.
        /// </summary>
        [BsonElement("drop_off_type")]
        [BsonIgnoreIfDefault]
        public StopType DropOffType { get; set; }

        /// <summary>
        /// When used in the stop_times.txt file, the shape_dist_traveled field positions a stop as a distance from 
        /// the first shape point. The shape_dist_traveled field represents a real distance traveled along the route 
        /// in units such as feet or kilometers. For example, if a bus travels a distance of 5.25 kilometers from the 
        /// start of the shape to the stop, the shape_dist_traveled for the stop ID would be entered as "5.25". This 
        /// information allows the trip planner to determine how much of the shape to draw when showing part of a trip 
        /// on the map. The values used for shape_dist_traveled must increase along with stop_sequence: they cannot 
        /// be used to show reverse travel along a route.
        /// <para/>
        /// The units used for shape_dist_traveled in the stop_times.txt file must match the units that are used for 
        /// this field in the shapes.txt file.
        /// </summary>
        [BsonElement("shape_dist_traveled")]
        [BsonIgnoreIfNull]
        public double? ShapeDistanceTraveled { get; set; }
    }
}