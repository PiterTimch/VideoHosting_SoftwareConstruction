using Application.Constants;
using Application.Features.Comments.Commands.CreateComments;
using Application.Features.Comments.Commands.UpdateComment;
using Application.Features.Comments.Queries.GetCommentsReplies;
using Application.Features.Comments.Queries.GetVideoComments;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.Comments;
using Application.Models.Search;
using Domain.Entities.Comments;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using Domain.Entities.Video;
using FluentAssertions;
using Moq;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Handlers;

public class CommentHandlersCoverageTests
{
    private readonly CommentMappingProfile _mapper = new();

    // ── helpers ───────────────────────────────────────────────────────────────

    private static ICurrentUserService Admin(long userId = 1)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(c => c.IsInRole(Roles.Admin)).Returns(true);
        mock.Setup(c => c.TryGetCurrentUserId()).Returns(userId);
        mock.Setup(c => c.GetCurrentUserId()).Returns(userId);
        return mock.Object;
    }

    private static async Task<Domain.AppDbContext> SeedVideoAndUser(long userId = 1, long videoId = 10)
    {
        var ctx = TestDbContextHelper.CreateContext();
        var user = new UserEntity { Id = userId, UserName = "u" + userId };
        var portfolio = new PortfolioEntity { Id = userId, Name = "P", NickName = "p" + userId, Freelancer = user };
        var privacy = new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = VideoPrivacyConstants.Public };
        var video = new VideoEntity
        {
            Id = videoId, Title = "V", Slug = "v", PrivacyId = 1,
            Privacy = privacy, PortfolioId = portfolio.Id,
            Portfolio = portfolio, IsDeleted = false
        };
        ctx.Users.Add(user);
        ctx.Portfolios.Add(portfolio);
        ctx.VideoPrivacies.Add(privacy);
        ctx.Videos.Add(video);
        await ctx.SaveChangesAsync();
        return ctx;
    }

    // ── CreateCommentCommandHandler ───────────────────────────────────────────

    [Fact]
    public async Task CreateComment_VideoNotFound_ThrowsException()
    {
        var ctx = TestDbContextHelper.CreateContext();
        var handler = new CreateCommentCommandHandler(ctx, _mapper, Admin());

        await FluentActions
            .Invoking(() => handler.Handle(new CreateCommentCommand("hello", 99, null), default))
            .Should().ThrowAsync<Exception>().WithMessage("Відео не знайдено");
    }

    [Fact]
    public async Task CreateComment_RootComment_Succeeds()
    {
        var ctx = await SeedVideoAndUser(userId: 1, videoId: 10);
        var handler = new CreateCommentCommandHandler(ctx, _mapper, Admin(1));

        var result = await handler.Handle(new CreateCommentCommand("Great video!", 10, null), default);

        result.Should().NotBeNull();
        result.Content.Should().Be("Great video!");
        ctx.Comments.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateComment_Reply_IncrementsParentRepliesCount()
    {
        var ctx = await SeedVideoAndUser(userId: 1, videoId: 10);
        var parent = new CommentsEntity { Id = 1, Content = "parent", VideoId = 10, UserId = 1 };
        ctx.Comments.Add(parent);
        await ctx.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(ctx, _mapper, Admin(1));
        await handler.Handle(new CreateCommentCommand("reply", 10, 1), default);

        ctx.ChangeTracker.Clear();
        var updatedParent = await ctx.Comments.FindAsync(1L);
        updatedParent!.RepliesCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateComment_ParentNotFound_ThrowsException()
    {
        var ctx = await SeedVideoAndUser(userId: 1, videoId: 10);
        var handler = new CreateCommentCommandHandler(ctx, _mapper, Admin(1));

        await FluentActions
            .Invoking(() => handler.Handle(new CreateCommentCommand("reply", 10, 999), default))
            .Should().ThrowAsync<Exception>().WithMessage("Батьківський коментар не знайдено");
    }

    [Fact]
    public async Task CreateComment_TooDeepReply_ThrowsException()
    {
        var ctx = await SeedVideoAndUser(userId: 1, videoId: 10);

        var root = new CommentsEntity { Id = 1, Content = "root", VideoId = 10, UserId = 1 };
        ctx.Comments.Add(root);
        await ctx.SaveChangesAsync();

        // depth1 links to root
        var depth1 = new CommentsEntity { Id = 2, Content = "d1", VideoId = 10, UserId = 1, ParentId = 1 };
        ctx.Comments.Add(depth1);
        await ctx.SaveChangesAsync();

        // depth2 links to depth1
        var depth2 = new CommentsEntity { Id = 3, Content = "d2", VideoId = 10, UserId = 1, ParentId = 2 };
        ctx.Comments.Add(depth2);
        await ctx.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(ctx, _mapper, Admin(1));

        // trying to reply to depth2 (which is already 2 levels deep) should fail
        await FluentActions
            .Invoking(() => handler.Handle(new CreateCommentCommand("too deep", 10, 3), default))
            .Should().ThrowAsync<Exception>().WithMessage("*максимальну глибину*");
    }

    // ── UpdateCommentCommandHandler ───────────────────────────────────────────

    [Fact]
    public async Task UpdateComment_CommentNotFound_ThrowsException()
    {
        var ctx = TestDbContextHelper.CreateContext();
        var handler = new UpdateCommentCommandHandler(ctx, _mapper, Admin(1));

        await FluentActions
            .Invoking(() => handler.Handle(new UpdateCommentCommand(999, "new content"), default))
            .Should().ThrowAsync<Exception>().WithMessage("Коментар не знайдено");
    }

    [Fact]
    public async Task UpdateComment_NotOwner_ThrowsUnauthorizedException()
    {
        var ctx = await SeedVideoAndUser(userId: 1, videoId: 10);
        ctx.Comments.Add(new CommentsEntity { Id = 1, Content = "original", VideoId = 10, UserId = 1 });
        await ctx.SaveChangesAsync();

        var handler = new UpdateCommentCommandHandler(ctx, _mapper, Admin(userId: 99));

        await FluentActions
            .Invoking(() => handler.Handle(new UpdateCommentCommand(1, "hacked"), default))
            .Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*чужий*");
    }

    [Fact]
    public async Task UpdateComment_Owner_UpdatesContentAndMarksEdited()
    {
        var ctx = await SeedVideoAndUser(userId: 1, videoId: 10);
        ctx.Comments.Add(new CommentsEntity { Id = 1, Content = "original", VideoId = 10, UserId = 1 });
        await ctx.SaveChangesAsync();

        var handler = new UpdateCommentCommandHandler(ctx, _mapper, Admin(1));
        var result = await handler.Handle(new UpdateCommentCommand(1, "updated content"), default);

        result.Content.Should().Be("updated content");
        ctx.ChangeTracker.Clear();
        var updated = await ctx.Comments.FindAsync(1L);
        updated!.IsEdited.Should().BeTrue();
        updated.Content.Should().Be("updated content");
    }

    // ── GetVideoCommentsQueryHandler ──────────────────────────────────────────

    [Fact]
    public async Task GetVideoComments_VideoNotFound_ThrowsException()
    {
        var ctx = TestDbContextHelper.CreateContext();
        var handler = new GetVideoCommentsQueryHandler(ctx, _mapper, Admin());

        await FluentActions
            .Invoking(() => handler.Handle(
                new GetVideoCommentsQuery(999, new BaseSearchParamsModel()), default))
            .Should().ThrowAsync<Exception>().WithMessage("Відео не знайдено");
    }

    [Fact]
    public async Task GetVideoComments_ReturnsPagedRootComments()
    {
        var ctx = await SeedVideoAndUser(userId: 1, videoId: 10);
        ctx.Comments.Add(new CommentsEntity { Id = 1, Content = "c1", VideoId = 10, UserId = 1, IsDeleted = false });
        ctx.Comments.Add(new CommentsEntity { Id = 2, Content = "c2", VideoId = 10, UserId = 1, IsDeleted = false });
        // reply should NOT appear in root comments
        ctx.Comments.Add(new CommentsEntity { Id = 3, Content = "reply", VideoId = 10, UserId = 1, ParentId = 1, IsDeleted = false });
        // deleted comment should not appear
        ctx.Comments.Add(new CommentsEntity { Id = 4, Content = "del", VideoId = 10, UserId = 1, IsDeleted = true });
        await ctx.SaveChangesAsync();

        var handler = new GetVideoCommentsQueryHandler(ctx, _mapper, Admin(1));
        var result = await handler.Handle(
            new GetVideoCommentsQuery(10, new BaseSearchParamsModel { Page = 1, ItemPerPage = 10 }), default);

        result.Items.Should().HaveCount(2);
        result.Pagination.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetVideoComments_DefaultPageValues_WorkCorrectly()
    {
        var ctx = await SeedVideoAndUser(userId: 1, videoId: 10);
        var handler = new GetVideoCommentsQueryHandler(ctx, _mapper, Admin(1));

        // Page < 1 and ItemPerPage < 1 should use defaults (1, 10)
        var result = await handler.Handle(
            new GetVideoCommentsQuery(10, new BaseSearchParamsModel { Page = 0, ItemPerPage = 0 }), default);

        result.Pagination.CurrentPage.Should().Be(1);
        result.Pagination.ItemsPerPage.Should().Be(10);
    }

    // ── GetCommentRepliesQueryHandler ─────────────────────────────────────────

    [Fact]
    public async Task GetCommentReplies_ReturnsRepliesForParent()
    {
        var ctx = await SeedVideoAndUser(userId: 1, videoId: 10);
        ctx.Comments.Add(new CommentsEntity { Id = 1, Content = "parent", VideoId = 10, UserId = 1, IsDeleted = false });
        ctx.Comments.Add(new CommentsEntity { Id = 2, Content = "reply1", VideoId = 10, UserId = 1, ParentId = 1, IsDeleted = false });
        ctx.Comments.Add(new CommentsEntity { Id = 3, Content = "reply2", VideoId = 10, UserId = 1, ParentId = 1, IsDeleted = false });
        ctx.Comments.Add(new CommentsEntity { Id = 4, Content = "deleted", VideoId = 10, UserId = 1, ParentId = 1, IsDeleted = true });
        await ctx.SaveChangesAsync();

        var handler = new GetCommentRepliesQueryHandler(ctx, _mapper);
        var result = await handler.Handle(
            new GetCommentRepliesQuery(1, new BaseSearchParamsModel { Page = 1, ItemPerPage = 10 }), default);

        result.Items.Should().HaveCount(2);
        result.Pagination.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCommentReplies_DefaultPagination_WorksCorrectly()
    {
        var ctx = TestDbContextHelper.CreateContext();
        var handler = new GetCommentRepliesQueryHandler(ctx, _mapper);

        var result = await handler.Handle(
            new GetCommentRepliesQuery(999, new BaseSearchParamsModel { Page = -1, ItemPerPage = -5 }), default);

        result.Should().NotBeNull();
        result.Pagination.CurrentPage.Should().Be(1);
        result.Pagination.ItemsPerPage.Should().Be(10);
        result.Items.Should().BeEmpty();
    }
}
