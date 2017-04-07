using GeoAPI.Geometries;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using SharpMap.Converters;
using SharpMap.Data.Providers.Business.MongoDB.Gtfs.Import;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    public class Stop
    {
        private GeoJsonPoint<GeoJson2DGeographicCoordinates> _stopLatLon;

        /// <summary>
        /// The feature ID for use with SharpMap
        /// </summary>
        [BusinessObjectIdentifier]
        [BsonElement("fid")]
        [BsonId(IdGenerator = typeof(UintIdGenerator))]
        public uint FID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the stop id
        /// </summary>
        /// <remarks>The stop_id field contains an ID that uniquely identifies a stop or station. Multiple routes may use the same stop. The stop_id is dataset unique.</remarks>
        [BusinessObjectAttribute(AllowNull = false, IsUnique = true,Ordinal = 1)]
        [BsonElement("stop_id")]
        //[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRequired]
        public string StopId { get; set; }
    
        /// <summary>
        /// Gets or sets a value indicating the stop code
        /// </summary>
        /// <remarks>The stop_code field contains short text or a number that uniquely identifies the stop for passengers. Stop codes are often used in phone-based transit information systems or printed on stop signage to make it easier for riders to get a stop schedule or real-time arrival information for a particular stop.
        /// The stop_code field should only be used for stop codes that are displayed to passengers. For internal codes, use stop_id. This field should be left blank for stops without a code.
        /// </remarks>
        [BusinessObjectAttribute(AllowNull = true, IsUnique = false, Ordinal = 2)]
        [BsonElement("stop_code")]
        public string StopCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the stop name
        /// </summary>
        /// <remarks>The stop_name field contains the name of a stop or station. Please use a name that people will understand in the local and tourist vernacular.
        /// </remarks>
        [BusinessObjectAttribute(AllowNull = false, IsUnique = false, Ordinal = 3)]
        [BsonElement("stop_name")]
        public string StopName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the stop desc
        /// </summary>
        /// <remarks>The stop_desc field contains a description of a stop. Please provide useful, quality information. Do not simply duplicate the name of the stop.
        /// </remarks>
        [BusinessObjectAttribute(AllowNull = true, IsUnique = false, Ordinal = 4)]
        [BsonElement("stop_desc")]
        public string StopDesc { get; set; }

        [BsonElement("stop_lat")]
        [BsonIgnore]
        public double StopLat
        {
            get { return _stopLatLon != null ? _stopLatLon.Coordinates.Latitude : double.NaN; }
            set
            {
                StopLatLon = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(StopLon, value));
            }
        }

        [BsonElement("stop_lon")]
        [BsonIgnore]
        public double StopLon
        {
            get { return _stopLatLon != null ? _stopLatLon.Coordinates.Longitude : double.NaN; }
            set
            {
                StopLatLon = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(value, StopLat));
            }
        }

        [BsonElement("stop_latlon")]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> StopLatLon
        {
            get { 
                return _stopLatLon 
                    ?? (_stopLatLon = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                                  new GeoJson2DGeographicCoordinates(double.NaN, double.NaN))); }
            set
            {
                _stopLatLon = value;
                _geometry = null;
            }
        }

        public static GeoJsonConverter<GeoJson2DGeographicCoordinates> Converter;

        private IGeometry _geometry;

        [BsonIgnore]
        [BusinessObjectGeometry]
        public IGeometry Geometry
        {
            get { return _geometry ?? (_geometry = Converter.ToPoint(StopLatLon)); }
            set
            {
                _geometry = value;
                StopLatLon = Converter.ToPoint(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the zone id
        /// </summary>
        /// <remarks>The zone_id field defines the fare zone for a stop ID. Zone IDs are required if you want to provide fare information using fare_rules.txt. If this stop ID represents a station, the zone ID is ignored.</remarks>
        [BusinessObjectAttribute(AllowNull = true, IsUnique = false, Ordinal = 5)]
        [BsonElement("zone_id")]
        public string ZoneID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the stop url
        /// </summary>
        /// <remarks>
        /// The stop_url field contains the URL of a web page about a particular stop. This should be different from the agency_url and the route_url fields.
        /// The value must be a fully qualified URL that includes http:// or https://, and any special characters in the URL must be correctly escaped. See http://www.w3.org/Addressing/URL/4_URI_Recommentations.html for a description of how to create fully qualified URL values.
        /// </remarks>
        [BusinessObjectAttribute(AllowNull = true, IsUnique = false, Ordinal = 6)]
        [BsonElement("stop_url")]
        public string StopUrl { get; set; }

        /// <summary>
        /// Gets or sets a vaue indicating the location type
        /// </summary>
        /// <remarks>
        /// The location_type field identifies whether this stop ID represents a stop or station. If no location type is specified, or the location_type is blank, stop IDs are treated as stops. Stations may have different properties from stops when they are represented on a map or used in trip planning.
        /// </remarks>
        [BsonElement("location_type")]
        [BusinessObjectAttribute(AllowNull = true, IsUnique = false, Ordinal = 7)]
        public LocationType LocationType { get; set; }

        /// <summary>
        /// For stops that are physically located inside stations, the parent_station 
        /// field identifies the station associated with the stop. To use this field, 
        /// stops.txt must also contain a row where this stop ID is assigned location type=1.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader><term>This stop ID represents</term><description>This entry's location_type field contains<br/>
        ///                                                              This entry's parent_station field contains</description></listheader>
        /// <item><term>A stop inside a station</term><description>0 or blank<br/>The stop ID of the station where this stop is located. The stop referenced by parent_station must have location_type=1</description></item>
        /// <item><term>A stop located outside a station</term><description>0 or blank<br/>A blank value. The parent_station field doesn't apply to this stop</description></item>
        /// <item><term>A station</term><description>1<br/>Stations can't contain other stations</description></item>
        /// </list>
        /// </remarks>
        [BusinessObjectAttribute(AllowNull = true, IsUnique = false, Ordinal = 8)]
        [BsonElement("parent_station")]
        public string ParentStation { get; set; }

        /// <summary>
        /// The stop_timezone field contains the timezone in which this stop or station is located. Please refer to Wikipedia List of Timezones for a list of valid values. If omitted, the stop should be assumed to be located in the timezone specified by agency_timezone in agency.txt.
        /// When a stop has a parent station, the stop is considered to be in the timezone specified by the parent station's stop_timezone value. If the parent has no stop_timezone value, the stops that belong to that station are assumed to be in the timezone specified by agency_timezone, even if the stops have their own stop_timezone values. In other words, if a given stop has a parent_station value, any stop_timezone value specified for that stop must be ignored.
        /// Even if stop_timezone values are provided in stops.txt, the times in stop_times.txt should continue to be specified as time since midnight in the timezone specified by agency_timezone in agency.txt. This ensures that the time values in a trip always increase over the course of a trip, regardless of which timezones the trip crosses.
        /// </summary>
        [BusinessObjectAttribute(AllowNull = true, IsUnique = false, Ordinal = 9)]
        [BsonElement("stop_timezone")]
        public string TimeZone { get; set; }

        /// <summary>
        /// The wheelchair_boarding field identifies whether wheelchair boardings are possible from the specified stop or station. The field can have the following values:
        /// <list type="table">
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>0 (or empty)</term><description>indicates that there is no accessibility information for the stop</description></item>
        /// <item><term>1</term><description>indicates that at least some vehicles at this stop can be boarded by a rider in a wheelchair</description></item>
        /// <item><term>2</term><desciption>wheelchair boarding is not possible at this stop</desciption></item>
        /// </list>
        /// When a stop is part of a larger station complex, as indicated by a stop with a parent_station value, the stop's wheelchair_boarding field has the following additional semantics:
        /// <list type="table">
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>0 (or empty)</term><description>the stop will inherit its wheelchair_boarding value from the parent station, if specified in the parent</description></item>
        /// <item><term>1</term><description>there exists some accessible path from outside the station to the specific stop / platform</description></item>
        /// <item><term>2</term><desciption>there exists no accessible path from outside the station to the specific stop / platform</desciption></item>
        /// </list>
        /// </summary>
        [BusinessObjectAttribute(AllowNull = true, IsUnique = false, Ordinal = 10)]
        [BsonElement("wheelchair_boarding")]
        public int? WheelchairBoarding { get; set; }
    }
}