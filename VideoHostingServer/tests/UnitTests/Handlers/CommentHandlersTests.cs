using Application.Constants;
using Application.Features.Comments.Commands.CreateComments;
using Application.Features.Comments.Commands.DeleteComment;
using Application.Features.Comments.Commands.UpdateComment;
using Application.Features.Comments.Queries.GetCommentsReplies;
using Application.Features.Comments.Queries.GetVideoComments;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.Search;
using Domain.Entities.Comments;
using Domain.Entities.Identity;
using Domain.Entities.Video;
using FluentAssertions;
using MediatR;
using Moq;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Handlers;

public class CommentHandlersTests
{
    private readonly CommentMappingProfile _mapper = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    [Fact]
    public async Task CreateComment_VideoNotFound_ThrowsException()
    {
        var context = TestDbContextHelper.CreateContext();
        _currentUserMock.Setup(c => c.GetCurrentUserId()).Returns(1);
        _currentUserMock.Setup(c => c.IsInRole(Roles.Admin)).Returns(false);

        var handler = new CreateCommentCommandHandler(context, _mapper, _currentUserMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new CreateCommentCommand("Nice", 999, null), default))
            .Should().ThrowAsync<Exception>().WithMessage("Відео не знайдено");
    }

    [Fact]
    public async Task CreateComment_ValidVideo_Success()
    {
        var context = TestDbContextHelper.CreateContext();
        _currentUserMock.Setup(c => c.GetCurrentUserId()).Returns(1);
        _currentUserMock.Setup(c => c.IsInRole(Roles.Admin)).Returns(true);

        var user = new UserEntity { Id = 1, UserName = "User1" };
        context.Users.Add(user);
        var privacy = new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = VideoPrivacyConstants.Public };
        var video = new VideoEntity { Id = 1, Title = "Vid", Privacy = privacy, PrivacyId = 1, IsDeleted = false };
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(context, _mapper, _currentUserMock.Object);

        var result = await handler.Handle(new CreateCommentCommand("Awesome video!", 1, null), default);

        result.Should().NotBeNull();
        result.Content.Should().Be("Awesome video!");
    }

    [Fact]
    public async Task CreateComment_ParentNotFound_ThrowsException()
    {
        var context = TestDbContextHelper.CreateContext();
        _currentUserMock.Setup(c => c.GetCurrentUserId()).Returns(1);
        _currentUserMock.Setup(c => c.IsInRole(Roles.Admin)).Returns(true);

        var privacy = new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = VideoPrivacyConstants.Public };
        var video = new VideoEntity { Id = 1, Title = "Vid", Privacy = privacy, PrivacyId = 1, IsDeleted = false };
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(context, _mapper, _currentUserMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new CreateCommentCommand("Reply", 1, 999), default))
            .Should().ThrowAsync<Exception>().WithMessage("Батьківський коментар не знайдено");
    }

    [Fact]
    public async Task CreateComment_MaxDepthExceeded_ThrowsException()
    {
        var context = TestDbContextHelper.CreateContext();
        _currentUserMock.Setup(c => c.GetCurrentUserId()).Returns(1);
        _currentUserMock.Setup(c => c.IsInRole(Roles.Admin)).Returns(true);

        var privacy = new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = VideoPrivacyConstants.Public };
        var video = new VideoEntity { Id = 1, Title = "Vid", Privacy = privacy, PrivacyId = 1, IsDeleted = false };
        context.Videos.Add(video);

        var root = new CommentsEntity { Id = 1, VideoId = 1, Content = "Root" };
        var reply1 = new CommentsEntity { Id = 2, VideoId = 1, Parent = root, ParentId = 1, Content = "Reply 1" };
        var reply2 = new CommentsEntity { Id = 3, VideoId = 1, Parent = reply1, ParentId = 2, Content = "Reply 2" };

        context.Comments.AddRange(root, reply1, reply2);
        await context.SaveChangesAsync();

        var handler = new CreateCommentCommandHandler(context, _mapper, _currentUserMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new CreateCommentCommand("Reply 3", 1, 3), default))
            .Should().ThrowAsync<Exception>().WithMessage("Досягнуто максимальну глибину відповідей (максимум 2 рівні)");
    }

    [Fact]
    public async Task DeleteComment_NotFoundOrWrongUser_ThrowsException()
    {
        var context = TestDbContextHelper.CreateContext();
        _currentUserMock.Setup(c => c.GetCurrentUserId()).Returns(1);

        var handler = new DeleteCommentCommandHandler(context, _currentUserMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new DeleteCommentCommand(999), default))
            .Should().ThrowAsync<Exception>().WithMessage("Коментар не знайдено");

        context.Comments.Add(new CommentsEntity { Id = 5, UserId = 2, Content = "User 2 comment" });
        await context.SaveChangesAsync();

        await FluentActions.Invoking(() => handler.Handle(new DeleteCommentCommand(5), default))
            .Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Ви не можете видалити чужий коментар");
    }

    [Fact]
    public async Task UpdateComment_ValidComment_UpdatesSuccessfully()
    {
        var context = TestDbContextHelper.CreateContext();
        _currentUserMock.Setup(c => c.GetCurrentUserId()).Returns(1);

        var user = new UserEntity { Id = 1, UserName = "User1" };
        context.Users.Add(user);
        var comment = new CommentsEntity { Id = 10, UserId = 1, User = user, Content = "Old content", IsEdited = false };
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        var handler = new UpdateCommentCommandHandler(context, _mapper, _currentUserMock.Object);

        var result = await handler.Handle(new UpdateCommentCommand(10, "New content"), default);

        result.Content.Should().Be("New content");
        var updated = await context.Comments.FindAsync(10L);
        updated!.IsEdited.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateComment_NotFoundOrWrongUser_ThrowsException()
    {
        var context = TestDbContextHelper.CreateContext();
        _currentUserMock.Setup(c => c.GetCurrentUserId()).Returns(1);

        var handler = new UpdateCommentCommandHandler(context, _mapper, _currentUserMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new UpdateCommentCommand(999, "Content"), default))
            .Should().ThrowAsync<Exception>().WithMessage("Коментар не знайдено");

        context.Comments.Add(new CommentsEntity { Id = 20, UserId = 2, Content = "Other user" });
        await context.SaveChangesAsync();

        await FluentActions.Invoking(() => handler.Handle(new UpdateCommentCommand(20, "Content"), default))
            .Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Ви не можете редагувати чужий коментар");
    }

    [Fact]
    public async Task GetVideoCommentsQueryHandler_ReturnsPagedComments()
    {
        var context = TestDbContextHelper.CreateContext();
        _currentUserMock.Setup(c => c.GetCurrentUserId()).Returns(1);
        _currentUserMock.Setup(c => c.IsInRole(Roles.Admin)).Returns(true);

        var user = new UserEntity { Id = 1, UserName = "User1" };
        context.Users.Add(user);
        var privacy = new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = VideoPrivacyConstants.Public };
        var video = new VideoEntity { Id = 1, Title = "Vid", Privacy = privacy, PrivacyId = 1, IsDeleted = false };
        context.Videos.Add(video);
        context.Comments.Add(new CommentsEntity { Id = 1, VideoId = 1, UserId = 1, User = user, Content = "Comment 1", ParentId = null, IsDeleted = false });
        context.Comments.Add(new CommentsEntity { Id = 2, VideoId = 1, UserId = 1, User = user, Content = "Reply 1", ParentId = 1, IsDeleted = false });
        await context.SaveChangesAsync();

        var handler = new GetVideoCommentsQueryHandler(context, _mapper, _currentUserMock.Object);

        var result = await handler.Handle(new GetVideoCommentsQuery(1, new BaseSearchParamsModel { Page = 1, ItemPerPage = 10 }), default);

        result.Items.Should().HaveCount(1);
        result.Pagination.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCommentRepliesQueryHandler_ReturnsReplies()
    {
        var context = TestDbContextHelper.CreateContext();
        var user = new UserEntity { Id = 1, UserName = "User1" };
        context.Users.Add(user);
        context.Comments.Add(new CommentsEntity { Id = 1, UserId = 1, User = user, Content = "Parent", ParentId = null, IsDeleted = false });
        context.Comments.Add(new CommentsEntity { Id = 2, UserId = 1, User = user, Content = "Child", ParentId = 1, IsDeleted = false });
        await context.SaveChangesAsync();

        var handler = new GetCommentRepliesQueryHandler(context, _mapper);

        var result = await handler.Handle(new GetCommentRepliesQuery(1, new BaseSearchParamsModel { Page = 1, ItemPerPage = 10 }), default);

        result.Items.Should().HaveCount(1);
        result.Items.First().Content.Should().Be("Child");
    }
}
