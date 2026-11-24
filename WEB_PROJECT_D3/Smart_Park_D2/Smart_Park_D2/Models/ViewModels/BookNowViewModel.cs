namespace Smart_Park_D2.Models.ViewModels
{
    public class BookNowViewModel
    {
        public int ParkingLocationId { get; set; }
        public string ParkingLocationName { get; set; } 
        public string Address { get; set; } 
        public decimal RatePerHour { get; set; }

        public List<ParkingSlot> AvailableSlots { get; set; } = new List<ParkingSlot>();

        public DateTime BookingDate { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public string SlotNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
