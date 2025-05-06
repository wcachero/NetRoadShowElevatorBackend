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
        var eligibleElevators = _elevators
            .Where(e =>
                (e.Direction == Direction.Idle) ||
                (e.Direction == Direction.Up && e.CurrentFloor <= request.CurrentFloor && request.CurrentFloor <= e.Destinations.Max()) ||
                (e.Direction == Direction.Down && e.CurrentFloor >= request.CurrentFloor && request.CurrentFloor >= e.Destinations.Min()))
            .ToList();

        if (!eligibleElevators.Any())
        {
            return null!;
        }

        var idleElevators = eligibleElevators
            .Where(e => e.Direction == Direction.Idle)
            .OrderBy(e => Math.Abs(e.CurrentFloor - request.CurrentFloor))
            .ToList();

        if (idleElevators.Any())
        {
            return idleElevators.First();
        }

        return eligibleElevators
            .OrderBy(e => Math.Abs(e.CurrentFloor - request.CurrentFloor))
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
