using ElevatorMovement.Dto;
using ElevatorMovement.Enums;
using ElevatorMovement.Model;
using System.Collections.Concurrent;

namespace ElevatorMovement.Services;

/// <summary>
/// ElevatorSystemService is responsible for managing the elevator system.
/// Developer: Wilson Cachero
/// Contact: wilson.09c@gmail.com
/// </summary>
public class ElevatorSystemService
{
    private readonly ConcurrentBag<Elevator> _elevators;
    private readonly ConcurrentDictionary<int, bool> _requestedFloors = new();

    public ElevatorSystemService(int elevatorCount)
    {
        _elevators = new ConcurrentBag<Elevator>(
            Enumerable.Range(1, elevatorCount)
                      .Select(id => new Elevator { ElevatorId = id })
        );
    }

    #region Elevator user request
    public void AddRequest(ElevatorRequest request)
    {
        var bestElevator = FindBestElevator(request);
        if (bestElevator == null)
        {
            Console.WriteLine("No available elevator for request.");
            return;
        }

        // Add the current floor to the elevator's destinations if it's not already there
        if (!bestElevator.Destinations.Contains(request.CurrentFloor))
        {
            bestElevator.Destinations.Enqueue(request.CurrentFloor);
        }

        // Add the destination floor to the elevator's destinations
        if (!bestElevator.Destinations.Contains(request.Floor))
        {
            bestElevator.Destinations.Enqueue(request.Floor);
        }

        _requestedFloors.TryAdd(request.Floor, true);

        if (bestElevator.Direction == Direction.Idle)
        {
            bestElevator.Direction = request.CurrentFloor > bestElevator.CurrentFloor
                ? Direction.Up
                : Direction.Down;
        }

        Console.WriteLine($"Request added: Current Floor {request.CurrentFloor}, Destination Floor {request.Floor}, Direction {request.Direction}, Assigned Elevator {bestElevator.ElevatorId}");
    }

    #endregion

    #region FindBest Elevator using dispatch logic
    private Elevator FindBestElevator(ElevatorRequest request)
    {
        // Filter elevators that can handle the request
        var eligibleElevators = _elevators
            .Where(e =>
                (e.Direction == Direction.Idle) || // Idle elevators can handle any request
                (e.Direction == Direction.Up && e.CurrentFloor <= request.CurrentFloor && request.CurrentFloor <= e.Destinations.Max()) || // Elevator going up and can stop at the requested floor
                (e.Direction == Direction.Down && e.CurrentFloor >= request.CurrentFloor && request.CurrentFloor >= e.Destinations.Min())) // Elevator going down and can stop at the requested floor
            .ToList();

        if (!eligibleElevators.Any())
        {
            // No eligible elevators, return null or handle as needed
            return null!;
        }

        // Prioritize idle elevators first
        var idleElevators = eligibleElevators
            .Where(e => e.Direction == Direction.Idle)
            .OrderBy(e => Math.Abs(e.CurrentFloor - request.CurrentFloor))
            .ToList();

        if (idleElevators.Any())
        {
            return idleElevators.First();
        }

        // If no idle elevators, find the closest busy elevator that can handle the request
        return eligibleElevators
            .OrderBy(e => Math.Abs(e.CurrentFloor - request.CurrentFloor))
            .FirstOrDefault()!;
    }

    #endregion

    #region Set Elevator Status and State
    public virtual void Step()
    {
        foreach (var elevator in _elevators)
        {
            if (elevator.Destinations.Count == 0)
            {
                elevator.Direction = Direction.Idle;
                Console.WriteLine($"Elevator {elevator.ElevatorId} is idle at floor {elevator.CurrentFloor}.");
                continue;
            }

            var targetFloor = elevator.Destinations.Peek();

            if (elevator.CurrentFloor < targetFloor)
            {
                elevator.CurrentFloor++;
                elevator.Direction = Direction.Up;
            }
            else if (elevator.CurrentFloor > targetFloor)
            {
                elevator.CurrentFloor--;
                elevator.Direction = Direction.Down;
            }
            else
            {
                // Arrived at floor
                Console.WriteLine($"Elevator {elevator.ElevatorId} arrived at floor {targetFloor}");
                elevator.Destinations.Dequeue();
                _requestedFloors.TryRemove(targetFloor, out _);

                if (elevator.Destinations.Count == 0)
                    elevator.Direction = Direction.Idle;
            }
        }
    }
    #endregion

    #region Retrieve Elevators Status
    public virtual List<Elevator> GetElevatorStatus()
    {
        return _elevators.Select(e => new Elevator
        {
            ElevatorId = e.ElevatorId,
            CurrentFloor = e.CurrentFloor,
            Direction = e.Direction,
            Destinations = new Queue<int>(e.Destinations),
            Status = Status.Online.ToString()
        }).ToList();
    }
    public virtual List<int> GetPendingFloorRequests()
    {
        return _requestedFloors.Keys.OrderBy(f => f).ToList();
    }
    #endregion
}