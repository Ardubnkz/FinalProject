using HotelBookingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Text;
using System.Security.Cryptography;


namespace HotelBookingWebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PasswordHash))
            {
                ViewBag.Message = "Please fill in all fields";
                return View();
            }
            user.PasswordHash = HashPassword(user.PasswordHash);
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("INSERT INTO users(name, email, password_hash) VALUES (@name, @mail, @pass", conn);
                cmd.Parameters.AddWithValue("@name", user.Name);
                cmd.Parameters.AddWithValue("@mail", user.Email);
                cmd.Parameters.AddWithValue("@name", user.PasswordHash);
                try
                {
                    cmd.ExecuteNonQuery();
                    ViewBag.Message = "Registration Succesful!";
                }
                catch(MySqlException ex)
                {
                    if (ex.Number == 1062)
                    {
                        ViewBag.Message = "Email Already Exist";
                    }
                    else
                    {
                        ViewBag.Message = ex.Message;
                    }
                }
            }
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM users WHERE email = @mail", conn);
                cmd.Parameters.AddWithValue("@mail", email);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string storedHash = reader["password_hash"].ToString();
                    if (VerifyPassword(password, storedHash))
                    {
                        HttpContext.Session.SetString("userEmail", email);
                        HttpContext.Session.SetString("UserName", reader["name"].ToString());
                        return RedirectToAction("Welcome"+ reader["name"].ToString());

                    }
                }
                ViewBag.Message = "Invalid Email or PAssword";
            }
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        public IActionResult Welcome()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToAction("Login");
            ViewBag.name = HttpContext.Session.GetString("UserName");
            return View();
        }
        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }
    }
}
