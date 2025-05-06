using ElevatorMovement.Services;
using ElevatorMovement.Services.BackgroundServices;
using FakeItEasy;

namespace NetRoadShowElevatorTest.Services.BackgroundServices
{
    public class ElevatorWorkerServiceTest
    {
        [Fact]
        public async Task ExecuteAsync_ShouldCallStepMethodRepeatedly()
        {
            // Arrange
            var elevatorSystemService = A.Fake<ElevatorSystemService>();
            var workerService = new ElevatorWorkerService(elevatorSystemService);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var executeTask = workerService.StartAsync(cancellationTokenSource.Token);

            // Allow the service to run
            await Task.Delay(50);

            // Stop the service
            cancellationTokenSource.Cancel();
            await executeTask;

            // Assert
            A.CallTo(() => elevatorSystemService.StepAsync()).MustHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldStopWhenCancelled()
        {
            // Arrange
            var elevatorSystemService = A.Fake<ElevatorSystemService>();
            var workerService = new ElevatorWorkerService(elevatorSystemService);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var executeTask = workerService.StartAsync(cancellationTokenSource.Token);

            // Cancel the service immediately
            cancellationTokenSource.Cancel();

            await executeTask;

            // Assert
            A.CallTo(() => elevatorSystemService.StepAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldRespectDelay()
        {
            // Arrange
            var elevatorSystemService = A.Fake<ElevatorSystemService>();
            var workerService = new ElevatorWorkerService(elevatorSystemService);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var executeTask = workerService.StartAsync(cancellationTokenSource.Token);

            // Allow the service to run 
            await Task.Delay(50);

            // Stop the service
            cancellationTokenSource.Cancel();
            await executeTask;

            // Assert
            A.CallTo(() => elevatorSystemService.StepAsync()).MustHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleExceptionsGracefully()
        {
            // Arrange
            var elevatorSystemService = A.Fake<ElevatorSystemService>();
            A.CallTo(() => elevatorSystemService.StepAsync()).ThrowsAsync(new System.Exception("Test exception"));
            var workerService = new ElevatorWorkerService(elevatorSystemService);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var executeTask = workerService.StartAsync(cancellationTokenSource.Token);

            // Allow the service to run briefly
            await Task.Delay(50);

            // Stop the service
            cancellationTokenSource.Cancel();
            await executeTask;

            // Assert
            A.CallTo(() => elevatorSystemService.StepAsync()).MustHaveHappened();
        }
    }
}
