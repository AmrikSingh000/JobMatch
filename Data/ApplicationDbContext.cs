// this file is for data / EF Core setup (ApplicationDbContext.cs). just keeping it simple.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobMatch.Models;

namespace JobMatch.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<Resume> Resumes => Set<Resume>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<MatchScore> MatchScores => Set<MatchScore>();
    }
}