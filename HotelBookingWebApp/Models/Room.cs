namespace HotelBookingWebApp.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public decimal Price { get; set; }
        public string BedType { get; set; }
        public string Description { get; set; }
    }
}
