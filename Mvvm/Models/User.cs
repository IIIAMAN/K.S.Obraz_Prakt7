using System;

namespace TaskManagement.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Login { get; set; } = string.Empty;
        public byte[] Password { get; set; } = Array.Empty<byte>();
        public int RoleID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
    }
}
