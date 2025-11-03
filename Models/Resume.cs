

using System.ComponentModel.DataAnnotations;
namespace JobMatch.Models
{

    public class Resume
    {
    
    
        public int Id { get; set; }
        [Required] public string JobseekerUserId { get; set; } = "";
        [Required, MaxLength(260)] public string FilePath { get; set; } = "";
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string? ParsedName { get; set; }
        public string? ParsedEducation { get; set; }
        public string? ParsedExperience { get; set; }
        public string? ParsedSkillsCsv { get; set; }
    }
}