using HotelBookingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace HotelBookingWebApp.Controllers
{
    public class RoomController : Controller
    {
        private readonly IConfiguration _config;
        public RoomController(IConfiguration config)
        {
            _config = config;
        }
        public IActionResult Index(DateTime? checkInDate, DateTime? checkOutDate)
        {
            List<Room> rooms = new List<Room>();
            string connStr = _config.GetConnectionString("DefaultConnection");
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string sql = @"SELECT * FROM rooms";
                if (checkInDate.HasValue && checkOutDate.HasValue)
                {
                    sql = @"
                SELECT * FROM rooms r
                WHERE r.room_id NOT IN (
                    SELECT b.room_id FROM bookings b
                    WHERE NOT(@out <= b.check_in_date OR @in >= b.check_out_date)
                );";
                }

                var cmd = new MySqlCommand(sql, conn);

                if (checkInDate.HasValue && checkOutDate.HasValue)
                {
                    cmd.Parameters.AddWithValue("@in", checkInDate);
                    cmd.Parameters.AddWithValue("@out", checkOutDate);
                }

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rooms.Add(new Room
                    {
                        RoomId = Convert.ToInt32(reader["room_id"]),
                        RoomNumber = reader["room_number"].ToString(),
                        RoomType = reader["room_type"].ToString(),
                        Price = Convert.ToDecimal(reader["price"]),
                        BedType = reader["bed_type"].ToString(),
                        Description = reader["description"].ToString()
                    });
                }
            }

            ViewBag.CheckIn = checkInDate?.ToString("yyyy-MM-dd");
            ViewBag.CheckOut = checkOutDate?.ToString("yyyy-MM-dd");

            return View(rooms);
        }
        public IActionResult Add()
        {
            string email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "User");
            if (!IsAdmin(email))
                return Content("Access denied. Admins only.");

            return View();
        }
        [HttpPost]
        public IActionResult Add(Room room)
        {
            string email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "User");

            if (!IsAdmin(email))
                return Content(" Access denied. Admins only.");

            if (string.IsNullOrEmpty(room.RoomNumber) || string.IsNullOrEmpty(room.RoomType))
            {
                ViewBag.Message = "All fields are required.";
                return View();
            }
            string connStr = _config.GetConnectionString("DefaultConnection");
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string sql = "INSERT INTO rooms (room_number, room_type, price, bed_type, description) VALUES (@num, @type, @price, @bed, @desc)";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@num", room.RoomNumber);
                cmd.Parameters.AddWithValue("@type", room.RoomType);
                cmd.Parameters.AddWithValue("@price", room.Price);
                cmd.Parameters.AddWithValue("@bed", room.BedType);
                cmd.Parameters.AddWithValue("@desc", room.Description);
                cmd.ExecuteNonQuery();
                ViewBag.Message = "Room added successfully!";
            }

            return View();
        }
        private bool IsAdmin(string email)
        {
           
            if (email.ToLower() == "admin@hotel.com")
                return true;

            return false;
        }
    }
}
