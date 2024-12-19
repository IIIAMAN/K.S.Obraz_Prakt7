using System;

namespace TaskManagement.Models
{
    public class Project
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
        public DateTime UpdatedAt { get; set; }
    }
}
