namespace CarCareTracker.Events;

public class VehicleUpdated
{
    public required string Identifier { get; init; }

    public required string Make { get; init; }

    public required string Model { get; init; }

    public required int Year { get; init; }

    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required string UserName { get; init; }
}