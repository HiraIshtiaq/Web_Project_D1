using Microsoft.Data.SqlClient;
using Dapper;
using Smart_Park_D2.Models.Interfaces;
namespace Smart_Park_D2.Models.Repository
{
    public class ParkingLocationRepository:IParkingLocationRepository
    {
        //private string connectionString ="Server=(localdb)\\mssqllocaldb;Database=aspnet-Smart_Park_D2-ac73dd28-d1d8-44c8-8772-affc81d936b3;Trusted_Connection=True;MultipleActiveResultSets=true";
        private string connectionString = "Server=(localdb)\\.\\SmartParkIISShared;Database=aspnet-Smart_Park_D2-ac73dd28-d1d8-44c8-8772-affc81d936b3;Trusted_Connection=True;MultipleActiveResultSets=true";
        public async Task<List<ParkingLocation>> GetAllAsync()
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"SELECT Id, Name, Address, RatePerHour,TotalSlots, OccupiedSlots FROM ParkingLocations";
            var locations = await connection.QueryAsync<ParkingLocation>(query);
            return locations.ToList();
        }
        public async Task<ParkingLocation?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"SELECT Id, Name, Address, RatePerHour,TotalSlots, OccupiedSlots FROM ParkingLocations WHERE Id = @id";

            return await connection.QueryFirstOrDefaultAsync<ParkingLocation>(
                query, new { id });
        }

        public async Task<int> AddAsync(ParkingLocation location)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"INSERT INTO ParkingLocations (Name, Address, RatePerHour, TotalSlots, OccupiedSlots)
                          VALUES (@Name, @Address, @RatePerHour, @TotalSlots, 0); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.QuerySingleAsync<int>(query, location);
        }

        public async Task UpdateAsync(ParkingLocation location)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"UPDATE ParkingLocations SET Name = @Name, Address = @Address, RatePerHour = @RatePerHour, TotalSlots = @TotalSlots WHERE Id = @Id";

            await connection.ExecuteAsync(query, location);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            var bookingCount = await connection.QuerySingleAsync<int>( @"SELECT COUNT(*) FROM Bookings  WHERE ParkingLocationId = @id", new { id }, transaction);

            if (bookingCount > 0)
            {
                return false;
            }

            await connection.ExecuteAsync("DELETE FROM ParkingSlots WHERE ParkingLocationId = @id",new { id }, transaction);

            await connection.ExecuteAsync( "DELETE FROM ParkingLocations WHERE Id = @id",  new { id }, transaction);

            transaction.Commit();

            return true;
        }

    }
}
