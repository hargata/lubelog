namespace CarCareTracker.Messaging;

public sealed class EventBusOptions
{
    /// <summary>
    /// Null = unbounded channel; otherwise bounded to this capacity.
    /// </summary>
    public int? BoundedCapacity { get; set; }

    /// <summary>
    /// If true, attempt single-reader optimizations.
    /// </summary>
    public bool SingleReader { get; set; } = true;

    /// <summary>
    /// If true and bounded, synchronous Publish() throws when the channel is full.
    /// </summary>
    public bool ThrowOnSyncPublishWhenFull { get; set; } = true;
}