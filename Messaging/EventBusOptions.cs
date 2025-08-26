namespace CarCareTracker.Messaging;

public sealed class EventBusOptions
{
    public int? BoundedCapacity { get; set; }

    public int NumReaders { get; set; } = 1;
    public bool ThrowOnSyncPublishWhenFull { get; set; } = true;
}