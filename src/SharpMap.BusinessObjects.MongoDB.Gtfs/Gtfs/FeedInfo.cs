using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    /// <summary>
    /// The file contains information about the feed itself, rather than the services that the 
    /// feed describes. GTFS currently has an <see cref="Agency"/>s file to provide information about the 
    /// agencies that operate the services described by the feed. However, the publisher of the 
    /// feed is sometimes a different entity than any of the agencies (in the case of regional 
    /// aggregators). In addition, there are some fields that are really feed-wide settings, rather 
    /// than agency-wide.
    /// </summary>
    public class FeedInfo
    {
        /// <summary>
        /// Contains the full name of the organization that publishes the feed. 
        /// (This may be the same as one of the <see cref="Agency.AgencyName"/> values in 
        /// <see cref="Agency"/>s file.) GTFS-consuming applications can display this name when giving 
        /// attribution for a particular feed's data.
        /// </summary>
        [BsonElement("feed_publisher_name")]
        [BsonRequired]
        public string FeedPublisherName { get; set; }

        /// <summary>
        /// Contains the URL of the feed publishing organization's website. 
        /// (This may be the same as one of the <see cref="Agency.AgencyUrl"/> values in 
        /// <see cref="Agency"/>s file.) The value must be a fully qualified URL that includes 
        /// http:// or https://, and any special characters in the URL must be correctly escaped.
        /// </summary>
        [BsonElement("feed_publisher_url")]
        [BsonRequired]
        public string FeedPublisherUrl { get; set; }

        /// <summary>
        /// Contains a IETF BCP 47 language code specifying the default language used for the text 
        /// in this feed. This setting helps GTFS consumers choose capitalization rules and other 
        /// language-specific settings for the feed. For an introduction to IETF BCP 47, please 
        /// refer to http://www.rfc-editor.org/rfc/bcp/bcp47.txt and http://www.w3.org/International/articles/language-tags/.
        /// </summary>
        [BsonElement("feed_lang")]
        [BsonRequired]
        public string FeedLang { get; set; }

        /// <summary>
        /// The feed provides complete and reliable schedule information for service in the period 
        /// from the beginning of the <see cref="FeedStartDate"/> day to the end of the <see cref="FeedEndDate"/> day. 
        /// Both days are given as dates in YYYYMMDD format as for <see cref="Calendar"/>s file, or left empty if 
        /// unavailable.
        /// <para/>
        /// The <see cref="FeedEndDate"/> date must not precede the <see cref="FeedStartDate"/> date 
        /// if both are given. Feed providers are encouraged to give schedule data outside this period 
        /// to advise of likely future service, but feed consumers should treat it mindful of its 
        /// non-authoritative status.
        /// <para/>
        /// If <see cref="FeedStartDate"/> or <see cref="FeedEndDate"/> extend beyond the active calendar dates 
        /// defined in <see cref="Calendar"/>s and <see cref="CalendarDate"/>s, the feed is making an explicit 
        /// assertion that there is no service for dates within the <see cref="FeedStartDate"/> or 
        /// <see cref="FeedEndDate"/> range but not included in the active calendar dates.
        /// </summary>
        [BsonElement("feed_start_date")]
        public DateTime FeedStartDate { get; set; }
        [BsonElement("feed_end_date")]
        public DateTime FeedEndDate { get; set; }

        public string FeedVersion { get; set; }
    }

}