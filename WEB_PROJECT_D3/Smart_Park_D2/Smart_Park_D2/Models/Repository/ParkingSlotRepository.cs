using Dapper;
using Microsoft.Data.SqlClient;
using Smart_Park_D2.Models;
using Smart_Park_D2.Models.Interfaces;

namespace Smart_Park_D2.Models.Repository
{
    public class ParkingSlotRepository : IParkingSlotRepository
    {
        //private string connectionString = "Server=(localdb)\\mssqllocaldb;Database=aspnet-Smart_Park_D2-ac73dd28-d1d8-44c8-8772-affc81d936b3;Trusted_Connection=True;MultipleActiveResultSets=true";
        private string connectionString = "Server=(localdb)\\.\\SmartParkIISShared;Database=aspnet-Smart_Park_D2-ac73dd28-d1d8-44c8-8772-affc81d936b3;Trusted_Connection=True;MultipleActiveResultSets=true";
        public async Task<List<ParkingSlot>> GetByLocationIdAsync(int locationId)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"SELECT Id, ParkingLocationId, RowName, SlotNumber, IsOccupied  FROM ParkingSlots
                          WHERE ParkingLocationId = @locationId";

            var slots = await connection.QueryAsync<ParkingSlot>( query, new { locationId });

            return slots.ToList();
        }

        public async Task AddSlotsAsync(int locationId, List<ParkingSlot> slots)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"INSERT INTO ParkingSlots (ParkingLocationId, RowName, SlotNumber, IsOccupied) VALUES (@ParkingLocationId, @RowName, @SlotNumber, @IsOccupied)";

            foreach (var slot in slots)
            {
                slot.ParkingLocationId = locationId;
                slot.IsOccupied = false;

                await connection.ExecuteAsync(query, slot);
            }
        }
        public async Task<int> UpdateSlotsAsync(int locationId, List<ParkingSlot> slots)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            var query = @"SELECT Id, ParkingLocationId, RowName, SlotNumber, IsOccupied FROM ParkingSlots WHERE ParkingLocationId = @locationId";

            var existingSlots = await connection.QueryAsync<ParkingSlot>(  query, new { locationId }, transaction);

            foreach (var existing in existingSlots)
            {
                bool found = false;

                foreach (var newSlot in slots)
                {
                    if (existing.SlotNumber == newSlot.SlotNumber)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found && !existing.IsOccupied)
                {
                    int bookingCount = await connection.QuerySingleAsync<int>(
                        @"SELECT COUNT(*)  FROM Bookings  WHERE ParkingLocationId = @locationId AND SlotNumber = @slotNumber  AND Status = 'Confirmed'",
                        new { locationId,slotNumber = existing.SlotNumber  },transaction);

                    if (bookingCount == 0)
                    {
                        await connection.ExecuteAsync(@"DELETE FROM ParkingSlots  WHERE Id = @id",  new { existing.Id }, transaction);
                    }
                }
            }

            existingSlots = await connection.QueryAsync<ParkingSlot>( query, new { locationId }, transaction);

            foreach (var newSlot in slots)
            {
                bool found = false;

                foreach (var existing in existingSlots)
                {
                    if (existing.SlotNumber == newSlot.SlotNumber)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    await connection.ExecuteAsync( @"INSERT INTO ParkingSlots (ParkingLocationId, RowName, SlotNumber, IsOccupied) VALUES (@ParkingLocationId, @RowName, @SlotNumber, 0)",new{ ParkingLocationId = locationId,  newSlot.RowName,newSlot.SlotNumber },transaction);
                }
            }
            int totalSlots = await connection.QuerySingleAsync<int>(@"SELECT COUNT(*) FROM ParkingSlots WHERE ParkingLocationId = @locationId",  new { locationId },  transaction);

            transaction.Commit();

            return totalSlots;
        }

    }
}

