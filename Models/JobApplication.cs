// this file is for JobApplication model. just keeping it simple.

using System.ComponentModel.DataAnnotations;
namespace JobMatch.Models
{
    public enum ApplicationStatus { Submitted, UnderReview, Shortlisted, Rejected, Hired }
// plain model class to hold data.
    public class JobApplication
    {
    // --- Properties ---
    // primary key
        public int Id { get; set; }
        [Required] public int JobId { get; set; }
        public Job? Job { get; set; }
        [Required] public string ApplicantUserId { get; set; } = "";
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(2048)] public string? NotesForRecruiter { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
        [MaxLength(512)] public string? CvFilePath { get; set; }
    }
}