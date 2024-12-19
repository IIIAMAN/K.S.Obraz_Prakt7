using System;

namespace TaskManagement.Models
{
    public class UserActivityLog
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; } = DateTime.Now;
    }
}
