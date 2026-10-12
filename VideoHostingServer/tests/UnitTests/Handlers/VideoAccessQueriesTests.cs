using Application.Constants;
using Application.Interfaces;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using Domain.Entities.Video;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Handlers;

public class VideoAccessQueriesTests
{
    private static async Task<Domain.AppDbContext> Seed()
    {
        var context = TestDbContextHelper.CreateContext();
        var user = new UserEntity { Id = 10, UserName = "owner" };
        var portfolio = new PortfolioEntity { Id = 10, Name = "P", NickName = "p", Freelancer = user };
        var pub = new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = VideoPrivacyConstants.Public };
        var priv = new VideoPrivacyEntity { Id = 2, Name = "Private", SystemCode = VideoPrivacyConstants.Private };
        var url = new VideoPrivacyEntity { Id = 3, Name = "Url", SystemCode = VideoPrivacyConstants.UrlOnly };
        context.Users.Add(user);
        context.Portfolios.Add(portfolio);
        context.VideoPrivacies.AddRange(pub, priv, url);
        context.Videos.AddRange(
            new VideoEntity { Id = 1, Title = "Pub", PrivacyId = 1, Privacy = pub, PortfolioId = 99, IsDeleted = false },
            new VideoEntity { Id = 2, Title = "Priv", PrivacyId = 2, Privacy = priv, PortfolioId = 10, IsDeleted = false },
            new VideoEntity { Id = 3, Title = "Url", PrivacyId = 3, Privacy = url, PortfolioId = 99, IsDeleted = false }
        );
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task ForCurrentUser_Admin_ReturnsAll()
    {
        var context = await Seed();
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.IsInRole(Roles.Admin)).Returns(true);

        var ids = await context.Videos.ForCurrentUser(current.Object).Select(v => v.Id).ToListAsync();
        ids.Should().BeEquivalentTo([1L, 2L, 3L]);
    }

    [Fact]
    public async Task ForCurrentUser_Authenticated_ReturnsOwnOrPublic()
    {
        var context = await Seed();
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.IsInRole(Roles.Admin)).Returns(false);
        current.Setup(c => c.TryGetCurrentUserId()).Returns(10);

        var ids = await context.Videos.ForCurrentUser(current.Object).Select(v => v.Id).ToListAsync();
        ids.Should().BeEquivalentTo([1L, 2L]);
    }

    [Fact]
    public async Task ForCurrentUser_Anonymous_ReturnsPublicOnly()
    {
        var context = await Seed();
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.IsInRole(Roles.Admin)).Returns(false);
        current.Setup(c => c.TryGetCurrentUserId()).Returns((long?)null);

        var ids = await context.Videos.ForCurrentUser(current.Object).Select(v => v.Id).ToListAsync();
        ids.Should().BeEquivalentTo([1L]);
    }

    [Fact]
    public async Task ForDirectAccess_Admin_ReturnsAll()
    {
        var context = await Seed();
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.IsInRole(Roles.Admin)).Returns(true);

        var ids = await context.Videos.ForDirectAccess(current.Object).Select(v => v.Id).ToListAsync();
        ids.Should().BeEquivalentTo([1L, 2L, 3L]);
    }

    [Fact]
    public async Task ForDirectAccess_Authenticated_ReturnsOwnPublicAndUrlOnly()
    {
        var context = await Seed();
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.IsInRole(Roles.Admin)).Returns(false);
        current.Setup(c => c.TryGetCurrentUserId()).Returns(10);

        var ids = await context.Videos.ForDirectAccess(current.Object).Select(v => v.Id).ToListAsync();
        ids.Should().BeEquivalentTo([1L, 2L, 3L]);
    }

    [Fact]
    public async Task ForDirectAccess_Anonymous_ReturnsPublicAndUrlOnly()
    {
        var context = await Seed();
        var current = new Mock<ICurrentUserService>();
        current.Setup(c => c.IsInRole(Roles.Admin)).Returns(false);
        current.Setup(c => c.TryGetCurrentUserId()).Returns((long?)null);

        var ids = await context.Videos.ForDirectAccess(current.Object).Select(v => v.Id).ToListAsync();
        ids.Should().BeEquivalentTo([1L, 3L]);
    }
}
