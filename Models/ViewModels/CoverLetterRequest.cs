using System.ComponentModel.DataAnnotations;
using JobMatch.Models;

namespace JobMatch.Models.ViewModels
{
    public class CoverLetterRequest
    {
        public int? ResumeId { get; set; }
        public Resume? Resume { get; set; }

        public int? JobId { get; set; }

        [MaxLength(200)]
        public string? JobTitle { get; set; }

        [MaxLength(200)]
        public string? Company { get; set; }

        [Display(Name="Job Description / Posting")]
        public string? JobDescription { get; set; }

        public string? GeneratedLetter { get; set; }
    }
}