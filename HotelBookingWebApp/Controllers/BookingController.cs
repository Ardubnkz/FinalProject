using HotelBookingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace HotelBookingWebApp.Controllers
{
    public class BookingController : Controller
    {
        private readonly IConfiguration _config;
        public BookingController(IConfiguration config)
        {
            _config = config;
        }
       public IActionResult Reserve (int roomId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToAction("Login", "User");
            ViewBag.RoomId = roomId;
            return View();
        }
        [HttpPost]
        public IActionResult Reserve(int roomId,DateTime checkInDate,DateTime checkOutDate)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToAction("Login", "User");
            if(checkOutDate<= checkInDate)
            {
                ViewBag.Message = "Check-out date must be after check in";
                ViewBag.RoomId = roomId;
                return View();
            }
            string email = HttpContext.Session.GetString("UserEmail");
            int userId = 0;
            string connStr = _config.GetConnectionString("DefaultConnection");
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                var cmdUser = new MySqlCommand("SELECT user_id FROM users WHERE email=@mail", conn);
                cmdUser.Parameters.AddWithValue("@mail", email);
                var reader = cmdUser.ExecuteReader();
                if (reader.Read())
                    userId = Convert.ToInt32(reader["user_id"]);
                reader.Close();
                string checkSql = @"SELECT COUNT(*) from bookings WHERE room_id = @roomId
                                    AND NOT (@out<= check_in_date OR @in >= check_out_date);";
                var cmdCheck = new MySqlCommand(checkSql, conn);
                cmdCheck.Parameters.AddWithValue("@roomId", roomId);
                cmdCheck.Parameters.AddWithValue("@in", checkInDate);
                cmdCheck.Parameters.AddWithValue("@out", checkOutDate);

                int conflicts = Convert.ToInt32(cmdCheck.ExecuteScalar());
                if (conflicts > 0)
                {
                    ViewBag.Message = "This room is already booked for the selected dates";
                    ViewBag.RoomId = roomId;
                    return View();
                }
                var cmdInsert = new MySqlCommand("INSERT INTO bookings (user_id, room_id, check_in_date, check_out_date) VALUES (@uid , @rid, @in, @out)", conn);
                cmdInsert.Parameters.AddWithValue("@uid", userId);
                cmdInsert.Parameters.AddWithValue("@rid", roomId);
                cmdInsert.Parameters.AddWithValue("@in", checkInDate);
                cmdInsert.Parameters.AddWithValue("@out", checkOutDate);
                cmdInsert.ExecuteNonQuery();
            }
            ViewBag.Message = "Room reserved succesfully!";
            return View();
        }
        public IActionResult MyBookings()
        {

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToAction("Login", "User");

            string email = HttpContext.Session.GetString("UserEmail");
            List<Booking> myBookings = new List<Booking>();
            string connStr = _config.GetConnectionString("DefaultConnection");

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
                    SELECT b.booking_id, b.room_id, b.check_in_date, b.check_out_date,
                    r.room_type, r.price
                    FROM bookings b
                    JOIN rooms r ON b.room_id = r.room_id
                    JOIN users u ON b.user_id = u.user_id
                    WHERE u.email=@mail", conn);
                cmd.Parameters.AddWithValue("@mail", email);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    myBookings.Add(new Booking
                    {
                    BookingId = Convert.ToInt32(reader["booking_id"]),
                    RoomId = Convert.ToInt32(reader["room_id"]),
                    CheckInDate = Convert.ToDateTime(reader["check_in_date"]),
                    CheckOutDate = Convert.ToDateTime(reader["check_out_date"]),
                    RoomType = reader["room_type"].ToString(),
                    Price = Convert.ToDecimal(reader["price"])
                    });
                }
            }

            return View(myBookings);
        }
        public IActionResult Cancel(int bookingId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToAction("Login", "User");

            string email = HttpContext.Session.GetString("UserEmail");
            string connStr = _config.GetConnectionString("DefaultConnection");

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
                    DELETE b FROM bookings b
                    JOIN users u ON b.user_id = u.user_id
                    WHERE b.booking_id=@bid AND u.email=@mail", conn);
                cmd.Parameters.AddWithValue("@bid", bookingId);
                cmd.Parameters.AddWithValue("@mail", email);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("MyBookings");
        }
    }
}
