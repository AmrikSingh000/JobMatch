using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JobMatch.Models;
using JobMatch.Models.Enums;

namespace JobMatch.Models.ViewModels
{
    public class JobFilterVM
    {
        [Display(Name = "What")]
        public string? Q { get; set; }

        [Display(Name = "Where")]
        public string? Location { get; set; }

        [Display(Name = "Type")]
        public EmploymentType? Type { get; set; }   

        [Display(Name = "Sort")]
        public string Sort { get; set; } = "Newest"; 

        
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;      
        public int Total { get; set; }
        public int TotalPages => PageSize <= 0 ? 1 : (int)System.Math.Ceiling((double)Total / PageSize);

        public IEnumerable<Job> Results { get; set; } = new List<Job>();
        public List<FilterChip> Chips { get; set; } = new();
    }
}
