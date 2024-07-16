namespace DoAnCoSo2.Models
{
    public class Notification
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Slug { get; set; }
        public string AvatarUrl { get; set; } // Add this property
    }

}

