namespace DoAnCoSo2.Models
{
    public class Notification
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BlogSlug { get; set; }  // Add this property
    }

}

