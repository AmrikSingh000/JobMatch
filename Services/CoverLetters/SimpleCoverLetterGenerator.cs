using System;
using System.Linq;
using System.Text;
using JobMatch.Models;
using JobMatch.Models.ViewModels;

namespace JobMatch.Services.CoverLetters
{
    public class SimpleCoverLetterGenerator : ICoverLetterGenerator
    {
        public string Generate(CoverLetterRequest request)
        {
            var applicantName = request.Resume?.ParsedName;
            if (string.IsNullOrWhiteSpace(applicantName))
            {

                applicantName = GuessNameFromFilePath(request.Resume?.FilePath) ?? "Applicant";
            }

            var today = DateTime.UtcNow.ToString("MMMM d, yyyy");
            var role = string.IsNullOrWhiteSpace(request.JobTitle) ? "the role" : request.JobTitle.Trim();
            var company = string.IsNullOrWhiteSpace(request.Company) ? "Hiring Manager" : request.Company.Trim();

            var resumeSkills = SplitCsv(request.Resume?.ParsedSkillsCsv);
            var resumeHighlights = SplitCsv(request.Resume?.ParsedExperience);
            var jobKeywords = ExtractKeywords(request.JobDescription);

            var topMatches = resumeSkills
                .Select(s => (skill: s, score: Score(s, jobKeywords)))
                .OrderByDescending(x => x.score)
                .ThenBy(x => x.skill.Length)
                .Take(7)
                .Select(x => x.skill)
                .ToList();

            if (topMatches.Count == 0 && resumeSkills.Count == 0)
            {
                topMatches = jobKeywords.Take(5).ToList();
            }

            var sb = new StringBuilder();
            sb.AppendLine(today);
            sb.AppendLine(company);
            sb.AppendLine();
            sb.AppendLine($"Re: Application for {role}");
            sb.AppendLine();

            sb.AppendLine($"Dear {company},");
            sb.AppendLine();
            sb.AppendLine(
                $"I am excited to submit my application for {role}. With a background in {DescribeDomain(resumeSkills, jobKeywords)}, I bring a track record of delivering results that align closely with your needs.");

            if (topMatches.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Highlights I would bring to the role include:");
                foreach (var m in topMatches)
                {
                    sb.AppendLine($"• {Capitalize(m)}");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.JobDescription))
            {
                var valueTailoring = TailorToJob(request.JobDescription);
                if (!string.IsNullOrWhiteSpace(valueTailoring))
                {
                    sb.AppendLine();
                    sb.AppendLine(valueTailoring);
                }
            }

            var closing = "I would welcome the opportunity to discuss how my experience can support your team. Thank you for your time and consideration.";
            sb.AppendLine();
            sb.AppendLine(closing);
            sb.AppendLine();
            sb.AppendLine($"Sincerely,");
            sb.AppendLine(applicantName);

            return sb.ToString();
        }

        private static string? GuessNameFromFilePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var file = System.IO.Path.GetFileNameWithoutExtension(path);

            var parts = new string(file.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray())
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;
            return string.Join(" ", parts.Select(TitleCase));
        }

        private static string TitleCase(string s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + (s.Length > 1 ? s[1..].ToLower() : "");

        private static int Score(string token, System.Collections.Generic.IEnumerable<string> jobKeywords)
        {
            token = token.ToLowerInvariant();
            return jobKeywords.Count(k => k.Contains(token) || token.Contains(k));
        }

        private static System.Collections.Generic.List<string> SplitCsv(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new();
            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim())
                      .Where(s => s.Length > 0)
                      .Distinct(StringComparer.OrdinalIgnoreCase)
                      .ToList();
        }

        private static System.Collections.Generic.List<string> ExtractKeywords(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new();
            var stop = new System.Collections.Generic.HashSet<string>(new [] {
                "and","or","the","to","a","of","in","for","with","on","as","by","an","is","are","be","you","your","we","our","that","this","will","role","team","job","position"
            }, StringComparer.OrdinalIgnoreCase);
            var tokens = new System.Collections.Generic.List<string>();
            foreach (var raw in text.Split(new [] { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '/', '\\', '(', ')', '[', ']', '{', '}', '-', '—' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = new string(raw.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
                if (t.Length < 3 || stop.Contains(t)) continue;
                tokens.Add(t);
            }
            return tokens.GroupBy(t => t).OrderByDescending(g => g.Count()).Select(g => g.Key).Take(30).ToList();
        }

        private static string DescribeDomain(System.Collections.Generic.List<string> resumeSkills, System.Collections.Generic.List<string> jobKeywords)
        {
            var intersect = resumeSkills.Where(s => jobKeywords.Any(k => k.Contains(s, StringComparison.OrdinalIgnoreCase) || s.Contains(k, StringComparison.OrdinalIgnoreCase))).ToList();
            if (intersect.Count >= 2) return string.Join(", ", intersect.Take(3));
            if (resumeSkills.Count > 0) return string.Join(", ", resumeSkills.Take(3));
            if (jobKeywords.Count > 0) return string.Join(", ", jobKeywords.Take(3));
            return "relevant domains";
        }

        private static string TailorToJob(string jobText)
        {

            var lines = jobText.Split(new [] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(s => s.Trim())
                               .Where(s => s.Length > 0)
                               .ToList();
            var keyLines = lines.Where(l => l.IndexOf("responsib", StringComparison.OrdinalIgnoreCase) >= 0
                                          || l.IndexOf("require", StringComparison.OrdinalIgnoreCase) >= 0
                                          || l.IndexOf("qualif", StringComparison.OrdinalIgnoreCase) >= 0)
                                .Take(2)
                                .ToList();
            if (keyLines.Count == 0) return "";
            return "From your posting, I noted: " + string.Join(" ", keyLines) + " I have direct experience addressing these needs and can ramp up quickly.";
        }

        private static string Capitalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            return char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s[1..] : "");
        }
    }
}
