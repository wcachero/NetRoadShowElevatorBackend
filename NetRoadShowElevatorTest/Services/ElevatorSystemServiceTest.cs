using ElevatorMovement.Dto;
using ElevatorMovement.Enums;
using ElevatorMovement.Model;
using ElevatorMovement.Services;
using ElevatorMovement.Services.BackgroundServices;
using FakeItEasy;
using System.Collections.Concurrent;

namespace NetRoadShowElevatorTest.Services
{
    public class ElevatorSystemServiceTests
    {
        private readonly ElevatorSystemService _elevatorSystemService;

        public ElevatorSystemServiceTests()
        {
            // Initialize the service with 4 elevators for testing
            _elevatorSystemService = new ElevatorSystemService((int)NumberOfElevator.Four);
        }

        [Fact]
        public void AddRequest_ShouldAssignRequestToIdleElevator()
        {
            // Arrange
            var request = new ElevatorRequest
            {
                CurrentFloor = 1,
                Floor = 5,
                Direction = Direction.Up
            };

            // Act
            _elevatorSystemService.AddRequest(request);
            var status = _elevatorSystemService.GetElevatorStatus();

            // Assert
            var elevator = status.FirstOrDefault(e => ((dynamic)e).Destinations.Contains(5));
            Assert.NotNull(elevator);
            Assert.Equal(1, ((dynamic)elevator).CurrentFloor);
            Assert.Contains(5, ((dynamic)elevator).Destinations);
        }

        [Fact]
        public async Task FindBestElevator_ShouldReturnClosestIdleElevatorAsync()
        {
            // Arrange
            var elevatorSystemService = new ElevatorSystemService(2); // Initialize with 2 elevators
            var workerService = new ElevatorWorkerService(elevatorSystemService);
            var request = new ElevatorRequest
            {
                CurrentFloor = 3,
                Floor = 7,
                Direction = Direction.Up
            };

            // Manually set the state of the elevators
            var elevators = new List<Elevator>
            {
                new Elevator { ElevatorId = 1, CurrentFloor = 1, Direction = Direction.Idle },
                new Elevator { ElevatorId = 2, CurrentFloor = 5, Direction = Direction.Idle }
            };

            var elevatorsField = typeof(ElevatorSystemService).GetField("_elevators", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            elevatorsField?.SetValue(elevatorSystemService, new ConcurrentBag<Elevator>(elevators));

            // Start the worker service
            var cancellationTokenSource = new CancellationTokenSource();
            var workerTask = workerService.StartAsync(cancellationTokenSource.Token);

            // Act
            var bestElevator = typeof(ElevatorSystemService)
                .GetMethod("FindBestElevator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(elevatorSystemService, new object[] { request }) as Elevator;
            await Task.Delay(1000);

            // Assert
            Assert.NotNull(bestElevator);
            Assert.Equal(2, bestElevator.ElevatorId); // Closest elevator to floor 3 is elevator 2

            // Cleanup
            cancellationTokenSource.Cancel();
            await workerTask;
        }

        [Fact]
        public async Task Step_ShouldMoveElevatorToNextDestinationAsync()
        {
            // Arrange
            var workerService = new ElevatorWorkerService(_elevatorSystemService);
            var request = new ElevatorRequest
            {
                CurrentFloor = 1,
                Floor = 5,
                Direction = Direction.Up
            };
            _elevatorSystemService.AddRequest(request);

            // Start the worker service
            var cancellationTokenSource = new CancellationTokenSource();
            var workerTask = workerService.StartAsync(cancellationTokenSource.Token);

            // Act
            await Task.Delay(1000); // Allow some time for the worker to process
            _elevatorSystemService.Step();
            var status = _elevatorSystemService.GetElevatorStatus();

            // Assert
            var elevator = status.FirstOrDefault(e => ((dynamic)e).Destinations.Contains(5));
            Assert.NotNull(elevator);
            Assert.Equal(2, ((dynamic)elevator).CurrentFloor); // Elevator should have moved one step

            // Cleanup
            cancellationTokenSource.Cancel();
            await workerTask;
        }


        [Fact]
        public void AddRequest_ShouldHandleBusyElevatorOnSamePath()
        {
            // Arrange
            var request1 = new ElevatorRequest
            {
                CurrentFloor = 1,
                Floor = 9,
                Direction = Direction.Up
            };
            var request2 = new ElevatorRequest
            {
                CurrentFloor = 4,
                Floor = 10,
                Direction = Direction.Up
            };

            _elevatorSystemService.AddRequest(request1);

            // Act
            _elevatorSystemService.AddRequest(request2);
            var status = _elevatorSystemService.GetElevatorStatus();

            // Assert
            var elevator = status.FirstOrDefault(e => ((dynamic)e).Destinations.Contains(10));
            Assert.NotNull(elevator);
            Assert.Contains(4, ((dynamic)elevator).Destinations);
            Assert.Contains(10, ((dynamic)elevator).Destinations);
        }

        [Fact]
        public void Step_ShouldHandleMultipleRequests()
        {
            // Arrange
            var request1 = new ElevatorRequest
            {
                CurrentFloor = 1,
                Floor = 5,
                Direction = Direction.Up
            };
            var request2 = new ElevatorRequest
            {
                CurrentFloor = 3,
                Floor = 7,
                Direction = Direction.Up
            };

            _elevatorSystemService.AddRequest(request1);
            _elevatorSystemService.AddRequest(request2);

            // Act
            _elevatorSystemService.Step();
            var status = _elevatorSystemService.GetElevatorStatus();

            // Assert
            var elevator1 = status.FirstOrDefault(e => ((dynamic)e).Destinations.Contains(5));
            var elevator2 = status.FirstOrDefault(e => ((dynamic)e).Destinations.Contains(7));

            Assert.NotNull(elevator1);
            Assert.NotNull(elevator2);
        }
    }
}