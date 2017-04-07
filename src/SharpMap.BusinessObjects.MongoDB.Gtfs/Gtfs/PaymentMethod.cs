namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    /// <summary>
    /// An enumeration of payment methods
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Ticket can be bought on board
        /// </summary>
        OnBoard,
        /// <summary>
        /// Ticket has to be bought before boarding.
        /// </summary>
        BeforeBoarding
    }
}