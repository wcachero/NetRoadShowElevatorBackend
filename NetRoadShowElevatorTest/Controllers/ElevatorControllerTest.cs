using ElevatorMovement.Controllers;
using ElevatorMovement.Dto;
using ElevatorMovement.Enums;
using ElevatorMovement.Model;
using ElevatorMovement.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Xunit;

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
        public async Task GetStatus_ShouldReturnElevatorStatus()
        {
            // Arrange
            var mockStatus = new List<Elevator>
            {
                new Elevator { ElevatorId = 1, CurrentFloor = 1, Direction = Direction.Idle },
                new Elevator { ElevatorId = 2, CurrentFloor = 5, Direction = Direction.Up }
            };
            A.CallTo(() => _elevatorSystemServiceFake.GetElevatorStatusAsync()).Returns(mockStatus);

            // Act
            var result = await _controller.GetStatus() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockStatus, result.Value);
        }


        [Fact]
        public async Task RequestElevator_ShouldReturnBadRequestForInvalidRequest()
        {
            // Arrange
            ElevatorRequest invalidRequest = null!;

            // Act
            var result = await _controller.RequestElevator(invalidRequest) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid request", result.Value);
        }

        [Fact]
        public async Task GetPendingFloors_ShouldReturnPendingFloorRequests()
        {
            // Arrange
            var mockPendingFloors = new List<int> { 3, 5, 7 };
            A.CallTo(() =>  _elevatorSystemServiceFake.GetPendingFloorRequestsAsync()).Returns(mockPendingFloors);

            // Act
            var result =await _controller.GetPendingFloors() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }
    }
}