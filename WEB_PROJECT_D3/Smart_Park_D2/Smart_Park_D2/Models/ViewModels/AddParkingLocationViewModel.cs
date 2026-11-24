namespace Smart_Park_D2.Models.ViewModels
{
    public class AddParkingLocationViewModel
    {
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal RatePerHour { get; set; }

        public List<ParkingRowViewModel> Rows { get; set; } = new List<ParkingRowViewModel>();
        public int Id { get; set; }
    }
}
