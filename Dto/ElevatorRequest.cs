using ElevatorMovement.Enums;

namespace ElevatorMovement.Dto
{
    public class ElevatorRequest
    {
        public int CurrentFloor { get; set; } // The floor where the request originates
        public int Floor { get; set; }        // The destination floor
        public Direction Direction { get; set; }
    }
}
