using Application.Interfaces;
using Domain.Entities.Video;

namespace Application.Services;

public class VideoRecommendationService : IVideoRecommendationService
{
    private const int TitleWordWeight = 3;
    private const int DescWordWeight = 1;

    public int ComputeScore(VideoEntity source, VideoEntity candidate)
    {
        int score = 0;

        var titleWords = ExtractWords(source.Title);
        if (titleWords.Length > 0)
        {
            var candidateTitleWords = ExtractWords(candidate.Title).ToHashSet();
            int matchedTitleWords = titleWords.Count(w => candidateTitleWords.Contains(w));
            score += matchedTitleWords * TitleWordWeight;
        }

        var descWords = ExtractWords(source.Description);
        if (descWords.Length > 0)
        {
            var candidateDescWords = ExtractWords(candidate.Description).ToHashSet();
            int matchedDescWords = descWords.Count(w => candidateDescWords.Contains(w));
            score += matchedDescWords * DescWordWeight;
        }

        return score;
    }

    private static string[] ExtractWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        return text
            .ToLowerInvariant()
            .Split([' ', ',', '.', '!', '?', '-', '_', ':', ';', '(', ')', '[', ']', '"', '\''],
                StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= 3)
            .Distinct()
            .ToArray();
    }
}
