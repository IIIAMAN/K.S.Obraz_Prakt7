using System;

namespace TaskManagement.Models
{
    public class Comment
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string CommentText { get; set; }
        public int TaskID { get; set; }
        public int UserID { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
    }
}
