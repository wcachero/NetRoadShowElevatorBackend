using ElevatorMovement.Dto;
using ElevatorMovement.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElevatorMovement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : ControllerBase
    {
        private readonly ElevatorSystemService _elevatorSystemService;

        public ElevatorController(ElevatorSystemService elevatorSystemService)
        {
            _elevatorSystemService = elevatorSystemService;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(_elevatorSystemService.GetElevatorStatus());
        }

        [HttpPost("request")]
        public IActionResult RequestElevator([FromBody] ElevatorRequest request)
        {
            if (request == null || request.Floor < 0)
                return BadRequest("Invalid request");

            _elevatorSystemService.AddRequest(request);
            return Ok(new { message = "Request added" });
        }

        [HttpGet("pending-floors")]
        public IActionResult GetPendingFloors()
        {
            return Ok(_elevatorSystemService.GetPendingFloorRequests());
        }
    }
}
