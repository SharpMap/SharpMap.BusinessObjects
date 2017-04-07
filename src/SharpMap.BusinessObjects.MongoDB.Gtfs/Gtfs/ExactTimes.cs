namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs
{
    public enum ExactTimes
    {
        /// <summary>
        /// Frequency-based trips are not exactly scheduled.
        /// </summary>
        No,
        /// <summary>
        /// Frequency-based trips are exactly scheduled. For a <see cref="Frequency"/>, 
        /// trips are scheduled starting with <code>TripStartTime = <see cref="Frequency.StartTime"/> + x * <see cref="Frequency.HeadwaySeconds"/></code> 
        /// for all x in (0, 1, 2, ...) where <code>TripEndTime</code> &lt; <see cref="Frequency.EndTime"/>.
        /// </summary>
        Yes,
    }
}