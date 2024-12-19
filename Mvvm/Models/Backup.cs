using System;

namespace TaskManagement.Models
{
    public class Backup
    {
        public int ID { get; set; }
        public DateTime BackupDate { get; set; } = DateTime.Now; 
        public string BackupFilePath { get; set; } = string.Empty;
    }
}
