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
        public async Task<IActionResult> GetStatus()
        {
            return Ok(await _elevatorSystemService.GetElevatorStatusAsync());
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestElevator([FromBody] ElevatorRequest request)
        {
            if (request == null || request.Floor < 0)
                return BadRequest("Invalid request");

           await _elevatorSystemService.AddRequestAsync(request);
            return Ok(new { message = "Request added" });
        }

        [HttpGet("pending-floors")]
        public async Task<IActionResult> GetPendingFloors()
        {
            return Ok(await _elevatorSystemService.GetElevatorStatusAsync());
        }
    }
}
