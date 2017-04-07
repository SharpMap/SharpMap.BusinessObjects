namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    public enum LocationType
    {
        /// <summary>
        /// A location where passengers board or disembark from a transit vehicle.
        /// </summary>
        Stop = 0,
        /// <summary>
        /// A physical structure or area that contains one or more stop. 
        /// </summary>
        Station
    }
}