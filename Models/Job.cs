using System;
using System.ComponentModel.DataAnnotations;
using JobMatch.Models.Enums;

namespace JobMatch.Models
{
    
    public class Job
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = "";

        [Required, MaxLength(120)]
        public string Organization { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [MaxLength(120)]
        public string? Location { get; set; }

        [MaxLength(40)]
        public string? JobType { get; set; } 

        [MaxLength(120)]
        public string? SalaryRange { get; set; }

        [MaxLength(256)]
        public string? TagsCsv { get; set; }

        [Required]
        public string PostedByUserId { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    }
}
