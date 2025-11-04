using System.Collections.Generic;
using JobMatch.Models;

namespace JobMatch.Models.ViewModels
{
    public class AdminDashboardVM
    {
        // High-level counts
        public int TotalUsers { get; set; }
        public int TotalJobseekers { get; set; }
        public int TotalRecruiters { get; set; }
        public int TotalAdmins { get; set; }

        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalApplications { get; set; }

        // Recent activity
        public List<Job> LatestJobs { get; set; } = new();
        public List<JobApplication> LatestApplications { get; set; } = new();
    }

    public class AdminUserVM
    {
        public string Id { get; set; } = "";
        public string? Email { get; set; }
        public string? UserName { get; set; }

        public bool IsJobseeker { get; set; }
        public bool IsRecruiter { get; set; }
        public bool IsAdmin { get; set; }

        public bool IsLockedOut { get; set; }
    }
}
