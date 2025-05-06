using ElevatorMovement.Controllers;
using ElevatorMovement.Dto;
using ElevatorMovement.Enums;
using ElevatorMovement.Model;
using ElevatorMovement.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;

namespace NetRoadShowElevatorTest.Controllers
{
    public class ElevatorControllerTest
    {
        private readonly ElevatorController _controller;
        private readonly ElevatorSystemService _elevatorSystemServiceFake;

        public ElevatorControllerTest()
        {
            _elevatorSystemServiceFake = A.Fake<ElevatorSystemService>(x => x.WithArgumentsForConstructor(() => new ElevatorSystemService(4)));
            _controller = new ElevatorController(_elevatorSystemServiceFake);
        }

        [Fact]
        public void GetStatus_ShouldReturnElevatorStatus()
        {
            // Arrange
            var mockStatus = new List<Elevator>
            {
                new Elevator { Id = 1, CurrentFloor = 1, Direction = Direction.Idle },
                new Elevator { Id = 2, CurrentFloor = 5, Direction = Direction.Up }
            };
            A.CallTo(() => _elevatorSystemServiceFake.GetElevatorStatus()).Returns(mockStatus);

            // Act
            var result = _controller.GetStatus() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockStatus, result.Value);
        }



        [Fact]
        public void RequestElevator_ShouldReturnBadRequestForInvalidRequest()
        {
            // Arrange
            ElevatorRequest invalidRequest = null;

            // Act
            var result = _controller.RequestElevator(invalidRequest) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid request", result.Value);
        }

        [Fact]
        public void GetPendingFloors_ShouldReturnPendingFloorRequests()
        {
            // Arrange
            var mockPendingFloors = new List<int> { 3, 5, 7 };
            A.CallTo(() => _elevatorSystemServiceFake.GetPendingFloorRequests()).Returns(mockPendingFloors);

            // Act
            var result = _controller.GetPendingFloors() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockPendingFloors, result.Value);
        }
    }
}