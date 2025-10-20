// this file is for MatchScore model. just keeping it simple.

namespace JobMatch.Models
{
// plain model class to hold data.
    public class MatchScore
    {
    // --- Properties ---
    // primary key
        public int Id { get; set; }
    // foreign key to Job
        public int JobId { get; set; }
        public int ResumeId { get; set; }
        public double Score { get; set; }
        public string? BreakdownJson { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }
}