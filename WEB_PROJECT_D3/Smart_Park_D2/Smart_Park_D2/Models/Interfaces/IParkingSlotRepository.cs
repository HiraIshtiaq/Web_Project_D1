namespace Smart_Park_D2.Models.Interfaces
{
    public interface IParkingSlotRepository
    {
        Task<List<ParkingSlot>> GetByLocationIdAsync(int locationId);
        Task AddSlotsAsync(int locationId, List<ParkingSlot> slots);
        Task<int> UpdateSlotsAsync(int locationId, List<ParkingSlot> slots);
    }
}
