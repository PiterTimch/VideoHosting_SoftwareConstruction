using Application.Services;
using Domain.Entities.Video;
using FluentAssertions;
using Xunit;

namespace UnitTests.Services;

public class VideoRecommendationServiceTests
{
    private readonly VideoRecommendationService _service = new();

    [Fact]
    public void ComputeScore_ExactTitleAndDescriptionMatch_CalculatesCorrectWeightedScore()
    {
        var v1 = new VideoEntity { Title = "Learn CSharp", Description = "Comprehensive tutorial" };
        var v2 = new VideoEntity { Title = "Learn CSharp", Description = "Comprehensive tutorial" };

        var score = _service.ComputeScore(v1, v2);

        score.Should().Be(8);
    }

    [Fact]
    public void ComputeScore_NoMatchingWords_ReturnsZero()
    {
        var v1 = new VideoEntity { Title = "React Framework", Description = "Frontend dev" };
        var v2 = new VideoEntity { Title = "Python Backend", Description = "Data science" };

        var score = _service.ComputeScore(v1, v2);

        score.Should().Be(0);
    }

    [Fact]
    public void ComputeScore_NullOrEmptyFields_ReturnsZeroSafely()
    {
        var v1 = new VideoEntity { Title = null, Description = "" };
        var v2 = new VideoEntity { Title = "   ", Description = null };

        var score = _service.ComputeScore(v1, v2);

        score.Should().Be(0);
    }

    [Fact]
    public void ComputeScore_WordsShorterThanThreeChars_AreIgnored()
    {
        var v1 = new VideoEntity { Title = "Go to C" };
        var v2 = new VideoEntity { Title = "Go to C" };

        var score = _service.ComputeScore(v1, v2);

        score.Should().Be(0);
    }

    [Fact]
    public void ComputeScore_PunctuationAndMixedCase_ExtractedAndMatchedProperly()
    {
        var v1 = new VideoEntity { Title = "Intro: [Docker-compose]!" };
        var v2 = new VideoEntity { Title = "docker, compose intro?" };

        var score = _service.ComputeScore(v1, v2);

        score.Should().Be(9);
    }
}
