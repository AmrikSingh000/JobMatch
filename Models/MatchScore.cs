

namespace JobMatch.Models
{

    public class MatchScore
    {
    
    
        public int Id { get; set; }
    
        public int JobId { get; set; }
        public int ResumeId { get; set; }
        public double Score { get; set; }
        public string? BreakdownJson { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }
}