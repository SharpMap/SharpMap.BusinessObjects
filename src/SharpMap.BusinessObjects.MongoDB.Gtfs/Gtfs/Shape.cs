using MongoDB.Bson.Serialization.Attributes;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    /// <summary>
    /// Shapes describe the physical path that a vehicle takes, and are defined in 
    /// the file shapes.txt. Shapes belong to Trips, and consist of a sequence of 
    /// points. Tracing the points in order provides the path of the vehicle. The 
    /// points do not need to match stop locations.
    /// </summary>
    public class Shape
    {
        /// <summary>
        /// Contains an ID that uniquely identifies a shape.
        /// </summary>
        [BsonElement("shape_id")]
        [BsonRequired]
        public string ShapeID { get; set; }

        /// <summary>
        /// Associates a shape point's latitude with a shape ID. The field value must be 
        /// a valid WGS 84 latitude from -90 to 90. Each row in <see cref="Shape"/>s represents a shape point in 
        /// your shape definition.
        /// </summary>
        [BsonElement("shape_pt_lat")]
        [BsonRequired]
        public double ShapePointLatitude { get; set; }

        /// <summary>
        /// Associates a shape point's longitude with a shape ID. The field value must be 
        /// a valid WGS 84 longitude value from -180 to 180. Each row in <see cref="Shape"/>s represents 
        /// a shape point in your shape definition.
        /// </summary>
        [BsonElement("shape_pt_lon")]
        [BsonRequired]
        public double ShapePointLongitude { get; set; }

        /// <summary>
        /// Associates the latitude and longitude of a shape point with its sequence order 
        /// along the shape. The values for <see cref="ShapePointSequence"/> must be non-negative integers, 
        /// and they must increase along the trip.
        /// </summary>
        [BsonElement("shape_pt_sequence")]
        [BsonRequired]
        public int ShapePointSequence { get; set; }

        /// <summary>
        /// When used in the <see cref="Shape"/> file, this field positions a shape point as a distance 
        /// traveled along a shape from the first shape point. The <see cref="ShapeDistanceTraveled"/> field 
        /// represents a real distance traveled along the route in units such as feet or kilometers. 
        /// This information allows the trip planner to determine how much of the shape to draw when 
        /// showing part of a trip on the map. The values used forshape_dist_traveled must increase 
        /// along with <see cref="ShapePointSequence"/>: they cannot be used to show reverse travel 
        /// along a route.
        /// <para/>
        /// The units used for <see cref="ShapeDistanceTraveled"/> in the <see cref="Shape"/> file 
        /// must match the units that are used for this field in the <see cref="StopTime"/> file.
        /// </summary>
        [BsonElement("shape_dist_traveled")]
        public double? ShapeDistanceTraveled { get; set; }
    }
}