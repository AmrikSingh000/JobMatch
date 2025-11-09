using JobMatch.Models;
using JobMatch.Models.ViewModels;

namespace JobMatch.Services.CoverLetters
{
    public interface ICoverLetterGenerator
    {
        string Generate(CoverLetterRequest request);
    }
}
