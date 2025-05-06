using System.Threading;
using System.Threading.Tasks;
using ElevatorMovement.Services.BackgroundServices;
using ElevatorMovement.Services;
using FakeItEasy;
using Xunit;

namespace NetRoadShowElevatorTest.Services.BackgroundServices
{
    public class ElevatorWorkerServiceTest
    {
        [Fact]
        public async Task ExecuteAsync_ShouldCallStepMethod()
        {
            // Arrange
            var elevatorSystemService = A.Fake<ElevatorSystemService>();
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

             A.CallTo(() => elevatorSystemService.Step()).MustHaveHappened();
 
        }
    }
}