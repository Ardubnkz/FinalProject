using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace HotelBookingWebApp.Controllers
{
    public class TestController : Controller
    {
        private readonly IConfiguration _configuration;
        public TestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    ViewBag.Message = "Connected succesfully to MYSQl";
                }
            }catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View();
        }
    }
}
