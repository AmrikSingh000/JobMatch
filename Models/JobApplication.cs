using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobMatch.Models
{
    public enum ApplicationStatus
    {
        Pending = 0,       
        Submitted = 1,     
        Shortlisted = 2,   
        Rejected = 3,      
        Hired = 4          
    }

    public class JobApplication
    {
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; } = null!;

        [Required, MaxLength(64)]
        public string ApplicantUserId { get; set; } = "";

        [MaxLength(4000)]
        public string? CoverLetter { get; set; }

        // Default status when a new application is created
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
