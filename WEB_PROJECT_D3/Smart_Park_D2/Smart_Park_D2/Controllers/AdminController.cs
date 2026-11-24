using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart_Park_D2.Models;
using Smart_Park_D2.Models.Interfaces;
using Smart_Park_D2.Models.Repository;
using Smart_Park_D2.Models.ViewModels;
namespace Smart_Park_Project.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private IUserRepository _userRepository;
        private IParkingLocationRepository _parkingLocationRepository;
        private IBookingRepository _bookingRepository;
        private IParkingSlotRepository _parkingSlotRepository;
        public AdminController(IUserRepository userRepository, IParkingLocationRepository parkingLocationRepository,IBookingRepository bookingRepository, IParkingSlotRepository parkingSlotRepository)
        {
            _userRepository = userRepository;
            _parkingLocationRepository = parkingLocationRepository;
            _bookingRepository = bookingRepository;
            _parkingSlotRepository = parkingSlotRepository;

        }
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var parkingLocations = await _parkingLocationRepository.GetAllAsync();
            var bookings = await _bookingRepository.GetAllBookingsAsync();
            int availableSlots = 0;
            int occupiedSlots = 0;
            decimal totalRevenue = 0;
            foreach (var location in parkingLocations)
            {
                availableSlots += location.TotalSlots - location.OccupiedSlots;

                occupiedSlots += location.OccupiedSlots;
            }
            foreach (var booking in bookings)
            {
                totalRevenue += booking.Amount;
            }
            var viewModel = new AdminDashboardViewModel
            {
                Users = users,
                ParkingLocations = parkingLocations,
                Bookings = bookings,
                TotalBookings = bookings.Count,
                AvailableSlots = availableSlots,
                OccupiedSlots = occupiedSlots,
                TotalRevenue = totalRevenue
            };
            return View(viewModel);
        }
        [HttpGet]
        public IActionResult AddLocation()
        {
            var model = new AddParkingLocationViewModel();

            model.Rows.Add(new ParkingRowViewModel
            {
                RowName = "A",
                NumberOfSlots = 10
            });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddLocation(AddParkingLocationViewModel model)
        {
            int totalSlots = 0;

            foreach (var row in model.Rows)
            {
                totalSlots += row.NumberOfSlots;
            }

            var location = new ParkingLocation
            {
                Name = model.Name,
                Address = model.Address,
                RatePerHour = model.RatePerHour,
                TotalSlots = totalSlots,
                OccupiedSlots = 0
            };

            int locationId = await _parkingLocationRepository.AddAsync(location);

            List<ParkingSlot> slots = new List<ParkingSlot>();

            foreach (var row in model.Rows)
            {
                for (int i = 1; i <= row.NumberOfSlots; i++)
                {
                    var slot = new ParkingSlot
                    {
                        ParkingLocationId = locationId,
                        RowName = row.RowName,
                        SlotNumber = row.RowName + "-" + i.ToString("D2"),
                        IsOccupied = false
                    };

                    slots.Add(slot);
                }
            }

            await _parkingSlotRepository.AddSlotsAsync(locationId, slots);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateLocation(int id)
        {
            var location = await _parkingLocationRepository.GetByIdAsync(id);

            if (location == null)
            {
                return RedirectToAction("Index");
            }

            var existingSlots = await _parkingSlotRepository.GetByLocationIdAsync(id);

            var model = new AddParkingLocationViewModel
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                RatePerHour = location.RatePerHour
            };

            foreach (var slot in existingSlots)
            {
                ParkingRowViewModel row = null;

                foreach (var existingRow in model.Rows)
                {
                    if (existingRow.RowName == slot.RowName)
                    {
                        row = existingRow;
                        break;
                    }
                }

                if (row == null)
                {
                    row = new ParkingRowViewModel
                    {
                        RowName = slot.RowName,
                        NumberOfSlots = 0
                    };

                    model.Rows.Add(row);
                }

                int slotNumber = 0;

                int.TryParse(
                    slot.SlotNumber.Substring(slot.SlotNumber.LastIndexOf('-') + 1),
                    out slotNumber);

                if (slotNumber > row.NumberOfSlots)
                {
                    row.NumberOfSlots = slotNumber;
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLocation(AddParkingLocationViewModel model)
        {
            var location = await _parkingLocationRepository.GetByIdAsync(model.Id);

            if (location == null)
            {
                return RedirectToAction("Index");
            }

            List<ParkingSlot> slots = new List<ParkingSlot>();

            foreach (var row in model.Rows)
            {
                for (int i = 1; i <= row.NumberOfSlots; i++)
                {
                    slots.Add(new ParkingSlot
                    {
                        ParkingLocationId = model.Id,
                        RowName = row.RowName,
                        SlotNumber = row.RowName + "-" + i.ToString("D2"),
                        IsOccupied = false
                    });
                }
            }

            int totalSlots = await _parkingSlotRepository.UpdateSlotsAsync(
                model.Id, slots);

            location.Name = model.Name;
            location.Address = model.Address;
            location.RatePerHour = model.RatePerHour;
            location.TotalSlots = totalSlots;

            await _parkingLocationRepository.UpdateAsync(location);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            bool deleted = await _parkingLocationRepository.DeleteAsync(id);

            if (!deleted)
            {
                TempData["ErrorMessage"] = "This location cannot be deleted because it has bookings.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            var parkingLocations = await _parkingLocationRepository.GetAllAsync();
            var bookings = await _bookingRepository.GetAllBookingsAsync();

            int availableSlots = 0;
            int occupiedSlots = 0;
            decimal totalRevenue = 0;

            foreach (var location in parkingLocations)
            {
                availableSlots += location.TotalSlots - location.OccupiedSlots;
                occupiedSlots += location.OccupiedSlots;
            }

            foreach (var booking in bookings)
            {
                totalRevenue += booking.Amount;
            }

            var model = new AdminDashboardViewModel
            {
                TotalBookings = bookings.Count,
                AvailableSlots = availableSlots,
                OccupiedSlots = occupiedSlots,
                TotalRevenue = totalRevenue
            };

            return PartialView("_DashboardStats", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetParkingLocations()
        {
            var locations = await _parkingLocationRepository.GetAllAsync();

            return PartialView("_ParkingLocations", locations);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();

            return PartialView("_UsersTable", users);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _bookingRepository.GetAllBookingsAsync();

            return PartialView("_BookingsTable", bookings);
        }


    }
}

