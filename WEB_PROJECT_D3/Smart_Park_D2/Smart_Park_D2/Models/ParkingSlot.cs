namespace Smart_Park_D2.Models
{
    public class ParkingSlot
    {
        public int Id { get; set; }

        public int ParkingLocationId { get; set; }

        public string RowName { get; set; } = string.Empty;

        public string SlotNumber { get; set; } = string.Empty;

        public bool IsOccupied { get; set; }

    }
}
