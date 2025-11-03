using System;
using System.ComponentModel.DataAnnotations;

namespace JobMatch.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = "";

        [Required, MaxLength(4000)]
        public string Message { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
