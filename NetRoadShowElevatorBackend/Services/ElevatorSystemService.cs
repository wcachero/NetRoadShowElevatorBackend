using ElevatorMovement.Dto;
using ElevatorMovement.Enums;
using ElevatorMovement.Model;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
    public async Task AddRequestAsync(ElevatorRequest request)
    {
        var bestElevator = await Task.Run(() => FindBestElevator(request));
        if (bestElevator == null)
        {
            Console.WriteLine("No available elevator for request.");
            return;
        }

        if (!bestElevator.Destinations.Contains(request.CurrentFloor))
        {
            bestElevator.Destinations.Enqueue(request.CurrentFloor);
        }

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
        // Filter eligible elevators based on direction and current state
        var eligibleElevators = _elevators
            .Where(e =>
                (e.Direction == Direction.Idle) || // Idle elevators can handle any request
                (e.Direction == request.Direction && // Elevators moving in the same direction
                 ((e.Direction == Direction.Up && e.CurrentFloor <= request.CurrentFloor) || // Moving up and can stop at the requested floor
                  (e.Direction == Direction.Down && e.CurrentFloor >= request.CurrentFloor)))) // Moving down and can stop at the requested floor
            .OrderBy(e => Math.Abs(e.CurrentFloor - request.CurrentFloor)) // Closest elevator first
            .ToList();

        if (!eligibleElevators.Any())
        {
            // No eligible elevators, return null or handle as needed
            return null!;
        }

        // Prioritize elevators already moving in the same direction
        var sameDirectionElevators = eligibleElevators
            .Where(e => e.Direction == request.Direction)
            .OrderBy(e => Math.Abs(e.CurrentFloor - request.CurrentFloor))
            .ToList();

        if (sameDirectionElevators.Any())
        {
            return sameDirectionElevators.First();
        }

        // If no elevators are moving in the same direction, prioritize idle elevators
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
            .OrderBy(e => e.Destinations.Count) // Prioritize elevators with fewer destinations
            .ThenBy(e => Math.Abs(e.CurrentFloor - request.CurrentFloor)) // Then by proximity
            .FirstOrDefault()!;
    }
    #endregion

    #region Set Elevator Status and State
    public async Task StepAsync()
    {
        var tasks = _elevators.Select(async elevator =>
        {
            if (elevator.Destinations.Count == 0)
            {
                elevator.Direction = Direction.Idle;
                Console.WriteLine($"Elevator {elevator.ElevatorId} is idle at floor {elevator.CurrentFloor}.");
                return;
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
                Console.WriteLine($"Elevator {elevator.ElevatorId} arrived at floor {targetFloor}");
                elevator.Destinations.Dequeue();
                _requestedFloors.TryRemove(targetFloor, out _);

                if (elevator.Destinations.Count == 0)
                    elevator.Direction = Direction.Idle;
            }

            await Task.Delay(100); // Simulate elevator movement delay
        });

        await Task.WhenAll(tasks);
    }
    #endregion

    #region Retrieve Elevators Status
    public async Task<List<Elevator>> GetElevatorStatusAsync()
    {
        return await Task.Run(() =>
            _elevators.Select(e => new Elevator
            {
                ElevatorId = e.ElevatorId,
                CurrentFloor = e.CurrentFloor,
                Direction = e.Direction,
                Destinations = new Queue<int>(e.Destinations),
                Status = Status.Online.ToString()
            }).ToList()
        );
    }

    public virtual async Task<List<int>> GetPendingFloorRequestsAsync()
    {
        return await Task.Run(() => _requestedFloors.Keys.OrderBy(f => f).ToList());
    }
    #endregion
}
