using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Smart_Park_D2.Data;
using Smart_Park_D2.Models;
using Smart_Park_D2.Models.Interfaces;
using Smart_Park_D2.Models.ViewModels;
using System.Security.Claims;

namespace Smart_Park_Project.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private IParkingLocationRepository _parkingRepository;
        private IBookingRepository _bookingRepository;
        private UserManager<ApplicationUser> _userManager;
        private IParkingSlotRepository _parkingSlotRepository;
        public UserController(IParkingLocationRepository parkingRepository,IBookingRepository bookingRepository,IParkingSlotRepository parkingSlotRepository,UserManager<ApplicationUser> userManager)
        {
            _parkingRepository = parkingRepository;
            _bookingRepository = bookingRepository;
            _parkingSlotRepository = parkingSlotRepository;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var locations = await _parkingRepository.GetAllAsync();
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId!);
            var currentUser = await _userManager.GetUserAsync(User);
            int activeBookings = 0;
            int totalBookings = 0;
            decimal totalSpent = 0;
            foreach (var booking in bookings)
            {
                totalBookings++;
                if (booking.Status == "Confirmed")
                {
                    activeBookings++;
                }
                totalSpent += booking.Amount;
            }
            var model = new UserDashboardViewModel
            {
                CustomerName = currentUser?.FullName ?? "Customer",
                ParkingLocations = locations,
                Bookings = bookings,
                ActiveBookings = activeBookings,
                TotalBookings = totalBookings,
                TotalSpent = totalSpent
            };
            return View(model);
        }
        //New one 
        [HttpGet]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = _userManager.GetUserId(User);

            var bookings = await _bookingRepository.GetUserBookingsAsync(userId!);

            return PartialView("_BookingTable", bookings);
        }
        [HttpGet]
        public async Task<IActionResult> BookNow(int id)
        {
            var location = await _parkingRepository.GetByIdAsync(id);

            if (location == null)
            {
                return RedirectToAction("Index");
            }

            var allSlots = await _parkingSlotRepository.GetByLocationIdAsync(id);

            var model = new BookNowViewModel
            {
                ParkingLocationId = location.Id,
                ParkingLocationName = location.Name,
                Address = location.Address,
                RatePerHour = location.RatePerHour,
                BookingDate = DateTime.Now
            };

            foreach (var slot in allSlots)
            {
                if (!slot.IsOccupied)
                {
                    model.AvailableSlots.Add(slot);
                }
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> BookNow(BookNowViewModel model)
        {
            var location = await _parkingRepository.GetByIdAsync(model.ParkingLocationId);

            if (location == null)
            {
                return RedirectToAction("Index");
            }

            if (model.DurationMinutes < 30 || model.DurationMinutes % 30 != 0)
            {
                return RedirectToAction("BookNow", new { id = model.ParkingLocationId });
            }

            var user = await _userManager.GetUserAsync(User);

            var booking = new Booking
            {
                UserId = user!.Id,
                ParkingLocationId = location.Id,
                SlotNumber = model.SlotNumber,
                BookingDate = model.BookingDate,
                DurationMinutes = model.DurationMinutes,
                Amount = location.RatePerHour * model.DurationMinutes / 60,
                Status = "Confirmed"
            };

            bool result = await _bookingRepository.AddBookingAsync(booking);

            if (!result)
            {
                TempData["ErrorMessage"] = "This slot is no longer available. Please select another slot.";
                return RedirectToAction("BookNow", new { id = model.ParkingLocationId });
            }

            return RedirectToAction("Index");
        }

    }
}