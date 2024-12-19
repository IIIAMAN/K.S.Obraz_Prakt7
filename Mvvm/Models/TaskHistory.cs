using System;

namespace TaskManagement.Models
{
    public class TaskHistory
    {
        public int ID { get; set; }
        public DateTime ChangeDate { get; set; }
        public int TaskID { get; set; }
        public int StatusID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
