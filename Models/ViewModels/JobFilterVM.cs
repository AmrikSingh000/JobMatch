// this file is for JobFilterVM model. just keeping it simple.
namespace JobMatch.Models.ViewModels
{
// plain model class to hold data.
    public class JobFilterVM
    {
    // --- Properties ---
        public string? Query { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public string? Sort { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int Total { get; set; }
        public IEnumerable<JobMatch.Models.Job> Items { get; set; } = Enumerable.Empty<JobMatch.Models.Job>();
    }
}