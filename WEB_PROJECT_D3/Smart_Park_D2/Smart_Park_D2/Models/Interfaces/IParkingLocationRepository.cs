using Smart_Park_D2.Models;
namespace Smart_Park_D2.Models.Interfaces
{
    public interface IParkingLocationRepository
    {
        Task<List<ParkingLocation>> GetAllAsync();
        Task<ParkingLocation?> GetByIdAsync(int id);

        Task<int> AddAsync(ParkingLocation location);

        Task UpdateAsync(ParkingLocation location);

        Task<bool> DeleteAsync(int id);
    }
}