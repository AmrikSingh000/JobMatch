using System;

namespace JobMatch.Models.ViewModels
{
    /// <summary>
    /// Simple in-memory data & privacy settings model for the Admin settings page.
    /// </summary>
    public class DataPolicySettings
    {
        public int RetentionDays { get; set; } = 365;

        public bool AllowExportRequests { get; set; } = true;

        public bool AllowDeletionRequests { get; set; } = true;

        public string PrivacyNoticeText { get; set; } =
            "We store your data only as long as necessary to provide the JobMatch service.";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
