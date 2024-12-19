using System;

namespace TaskManagement.Models
{
    public class TaskDistribution
    {
        public int ID { get; set; }
        public int TaskID { get; set; }
        public int UserID { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;
    }
}
