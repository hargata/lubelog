namespace CarCareTracker.Events;

public class VehicleAdded
{
    public required string Identifier { get; init; }

    public required string Make { get; init; }

    public required string Model { get; init; }

    public required int Year { get; init; }

    public required int VehicleId { get; init; }

    public required int UserId { get; init; }

    public required string UserName { get; init; }
}