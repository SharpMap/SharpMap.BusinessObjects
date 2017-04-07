namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    /// <summary>
    /// Enumeration of possible transfer types
    /// </summary>
    public enum TransferType
    {
        /// <summary>
        /// This is a recommended transfer point between two routes.
        /// </summary>
        Recommended,
        /// <summary>
        /// This is a timed transfer point between two routes. The departing vehicle is 
        /// expected to wait for the arriving one, with sufficient time for a passenger 
        /// to transfer between routes.
        /// </summary>
        Safe,
        /// <summary>
        /// This transfer requires a minimum amount of time between arrival and departure
        /// to ensure a connection. The time required to transfer is specified by <see cref="Transfer.MinTransferTime"/>
        /// </summary>
        LimitedTime,

        /// <summary>
        /// Transfers are not possible between routes at this location.
        /// </summary>
        Impossible
    }
}