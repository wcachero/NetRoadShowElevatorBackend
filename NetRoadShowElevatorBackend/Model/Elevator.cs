using ElevatorMovement.Enums;
using System.Diagnostics.CodeAnalysis;

namespace ElevatorMovement.Model;
[ExcludeFromCodeCoverage]
public class Elevator
{
    public int Id { get; set; }
    public int CurrentFloor { get; set; } = 1;
    public Direction Direction { get; set; } = Direction.Idle;
    public Queue<int> Destinations { get; set; } = new();
    public bool IsBusy => Destinations.Any();
    public string? Status { get; set; }
}