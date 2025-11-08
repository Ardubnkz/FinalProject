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
        public IActionResult Index()
        {
            List<Room> rooms = new List<Room>();
            string connStr = _config.GetConnectionString("DefaultConnection");
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM rooms", conn);
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
                    } );
                }
            }

            return View(rooms);
        }
        public IActionResult Add()
        {
            return View();  
        }
        [HttpPost]
        public IActionResult Add(Room room)
        {
            if (string.IsNullOrEmpty(room.RoomNumber) || string.IsNullOrEmpty(room.RoomType))
            {
                ViewBag.Message = "All fields are required";
                return View();
            }
            string connStr = _config.GetConnectionString("DefaultConnection");
            using (var conn = new MySqlConnection(connStr))
            { 
                conn.Open();
                string sql = "INSERT INTO rooms(room_number, room_type, price, bed_type, description)VALUES (@num, @type,@price,@bed,@desc)";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@num", room.RoomNumber);
                cmd.Parameters.AddWithValue("@type", room.RoomType);
                cmd.Parameters.AddWithValue("@price", room.Price);
                cmd.Parameters.AddWithValue("@bed", room.BedType);
                cmd.Parameters.AddWithValue("@desc", room.Description);
                cmd.ExecuteNonQuery();
                ViewBag.Message = "Room added!";
            }
            return View();
        }
    }
}
