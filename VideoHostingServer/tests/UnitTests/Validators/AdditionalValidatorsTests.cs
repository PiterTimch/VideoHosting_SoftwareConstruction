using System.Security.Claims;
using Application.Models.Portfolio;
using Application.Models.Video;
using Application.Validators.Account;
using Application.Validators.Extensions;
using Application.Validators.Portfolio;
using Application.Validators.User;
using Application.Validators.Video;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using Domain.Entities.Video;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Validators;

public class AdditionalValidatorsTests
{
    private static IFormFile File(string name, string contentType)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(name);
        mock.Setup(f => f.ContentType).Returns(contentType);
        return mock.Object;
    }

    [Fact]
    public async Task VideoCreateModelValidator_ValidAndInvalidCases()
    {
        var context = TestDbContextHelper.CreateContext();
        context.VideoPrivacies.Add(new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = "PUBLIC" });
        context.Videos.Add(new VideoEntity { Id = 1, Title = "Existing", Slug = "existing-slug", IsDeleted = false });
        await context.SaveChangesAsync();

        var validator = new VideoCreateModelValidator(context);
        var video = File("clip.mp4", "video/mp4");
        var image = File("pic.png", "image/png");

        var valid = new VideoCreateModel
        {
            Title = "New video",
            Slug = "new-video",
            Description = "desc",
            PrivacyId = 1,
            Video = video,
            Image = image
        };
        (await validator.ValidateAsync(valid)).IsValid.Should().BeTrue();

        var empty = new VideoCreateModel();
        (await validator.ValidateAsync(empty)).IsValid.Should().BeFalse();

        var badSlug = new VideoCreateModel
        {
            Title = "T",
            Slug = "Bad Slug",
            Description = "d",
            PrivacyId = 1,
            Video = video
        };
        (await validator.ValidateAsync(badSlug)).IsValid.Should().BeFalse();

        var duplicateSlug = new VideoCreateModel
        {
            Title = "T",
            Slug = "existing-slug",
            Description = "d",
            PrivacyId = 1,
            Video = video
        };
        (await validator.ValidateAsync(duplicateSlug)).IsValid.Should().BeFalse();

        var longTitle = new VideoCreateModel
        {
            Title = new string('a', 256),
            Slug = "ok-slug",
            Description = "d",
            PrivacyId = 1,
            Video = video
        };
        (await validator.ValidateAsync(longTitle)).IsValid.Should().BeFalse();

        var badVideo = new VideoCreateModel
        {
            Title = "T",
            Slug = "ok-slug",
            Description = "d",
            PrivacyId = 1,
            Video = File("clip.exe", "application/octet-stream")
        };
        (await validator.ValidateAsync(badVideo)).IsValid.Should().BeFalse();

        var missingPrivacy = new VideoCreateModel
        {
            Title = "T",
            Slug = "ok-slug-2",
            Description = "d",
            PrivacyId = 99,
            Video = video
        };
        (await validator.ValidateAsync(missingPrivacy)).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VideoUpdateModelValidator_ValidAndInvalidCases()
    {
        var context = TestDbContextHelper.CreateContext();
        context.VideoPrivacies.Add(new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = "PUBLIC" });
        context.Videos.Add(new VideoEntity { Id = 1, Title = "V1", Slug = "v1", IsDeleted = false });
        context.Videos.Add(new VideoEntity { Id = 2, Title = "V2", Slug = "v2", IsDeleted = false });
        await context.SaveChangesAsync();

        var validator = new VideoUpdateModelValidator(context);
        var valid = new VideoUpdateModel
        {
            Id = 1,
            Title = "Updated",
            Slug = "v1",
            Description = "d",
            PrivacyId = 1
        };
        (await validator.ValidateAsync(valid)).IsValid.Should().BeTrue();

        var missing = new VideoUpdateModel { Id = 99, Title = "T", Slug = "s", PrivacyId = 1 };
        (await validator.ValidateAsync(missing)).IsValid.Should().BeFalse();

        var takenSlug = new VideoUpdateModel { Id = 1, Title = "T", Slug = "v2", PrivacyId = 1 };
        (await validator.ValidateAsync(takenSlug)).IsValid.Should().BeFalse();

        var emptyId = new VideoUpdateModel { Id = 0, Title = "T", Slug = "s", PrivacyId = 1 };
        (await validator.ValidateAsync(emptyId)).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VideoDeleteAndReactionValidators_CheckExistence()
    {
        var context = TestDbContextHelper.CreateContext();
        context.Videos.Add(new VideoEntity { Id = 1, Title = "V1", Slug = "v1", IsDeleted = false });
        await context.SaveChangesAsync();

        var deleteValidator = new VideoDeleteModelValidator(context);
        (await deleteValidator.ValidateAsync(new VideoDeleteModel { Id = 1 })).IsValid.Should().BeTrue();
        (await deleteValidator.ValidateAsync(new VideoDeleteModel { Id = 0 })).IsValid.Should().BeFalse();
        (await deleteValidator.ValidateAsync(new VideoDeleteModel { Id = 9 })).IsValid.Should().BeFalse();

        var reactionValidator = new VideoReactionModelValidator(context);
        (await reactionValidator.ValidateAsync(new VideoReactionModel { VideoId = 1, IsLike = true })).IsValid.Should().BeTrue();
        (await reactionValidator.ValidateAsync(new VideoReactionModel { VideoId = 0 })).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task PortfolioValidators_CoverCreateUpdateDeleteAndSubscribe()
    {
        var context = TestDbContextHelper.CreateContext();
        var user = new UserEntity { Id = 5, UserName = "u5" };
        context.Users.Add(user);
        context.Portfolios.Add(new PortfolioEntity { Id = 5, Name = "P", NickName = "taken-nick", Freelancer = user });
        await context.SaveChangesAsync();

        var http = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "5") }, "t"));
        http.Setup(h => h.HttpContext).Returns(httpContext);

        var createValidator = new PortfolioCreateModelValidator(context, http.Object);
        var createTaken = new PortfolioCreateModel { Name = "N", NickName = "taken-nick" };
        (await createValidator.ValidateAsync(createTaken)).IsValid.Should().BeFalse();

        var createExistingChannel = new PortfolioCreateModel { Name = "N", NickName = "fresh-nick" };
        (await createValidator.ValidateAsync(createExistingChannel)).IsValid.Should().BeFalse();

        http.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());
        var createOk = new PortfolioCreateModel { Name = "N", NickName = "fresh-nick", Description = "d" };
        (await createValidator.ValidateAsync(createOk)).IsValid.Should().BeTrue();

        var emptyCreate = new PortfolioCreateModel();
        (await createValidator.ValidateAsync(emptyCreate)).IsValid.Should().BeFalse();

        var updateValidator = new PortfolioUpdateModelValidator(context);
        var updateOk = new PortfolioUpdateModel { Id = 5, Name = "N", NickName = "taken-nick" };
        (await updateValidator.ValidateAsync(updateOk)).IsValid.Should().BeTrue();

        var updateMissing = new PortfolioUpdateModel { Id = 99, Name = "N", NickName = "x" };
        (await updateValidator.ValidateAsync(updateMissing)).IsValid.Should().BeFalse();

        var updateBadId = new PortfolioUpdateModel { Id = 0, Name = "N", NickName = "x" };
        (await updateValidator.ValidateAsync(updateBadId)).IsValid.Should().BeFalse();

        var deleteValidator = new PortfolioDeleteModelValidator(context);
        (await deleteValidator.ValidateAsync(new PortfolioDeleteModel { Id = 5 })).IsValid.Should().BeTrue();
        (await deleteValidator.ValidateAsync(new PortfolioDeleteModel { Id = 0 })).IsValid.Should().BeFalse();

        var subValidator = new PortfolioSubscriptionModelValidator(context);
        (await subValidator.ValidateAsync(new PortfolioSubscriptionModel { PortfolioId = 5 })).IsValid.Should().BeTrue();
        (await subValidator.ValidateAsync(new PortfolioSubscriptionModel { PortfolioId = 0 })).IsValid.Should().BeFalse();
    }

    [Fact]
    public void FileAndSlugExtensions_ValidateOptionalAndInvalidFiles()
    {
        var imageValidator = new UserEditModelValidator();
        imageValidator.Validate(new Application.Models.User.UserEditModel()).IsValid.Should().BeTrue();

        var okImage = new Mock<IFormFile>();
        okImage.Setup(f => f.FileName).Returns("a.jpg");
        okImage.Setup(f => f.ContentType).Returns("image/jpeg");
        imageValidator.Validate(new Application.Models.User.UserEditModel { Image = okImage.Object }).IsValid.Should().BeTrue();

        var registerValidator = new AccountRegisterModelValidator();
        var badImage = new Mock<IFormFile>();
        badImage.Setup(f => f.FileName).Returns("a.gif");
        badImage.Setup(f => f.ContentType).Returns("image/gif");
        var register = new Application.Models.Account.AccountRegisterModel
        {
            FirstName = "A",
            Email = "a@b.com",
            Password = "123456",
            ImageFile = badImage.Object
        };
        registerValidator.Validate(register).IsValid.Should().BeFalse();

        var shortPassword = new Application.Models.Account.AccountRegisterModel
        {
            FirstName = "A",
            Email = "not-email",
            Password = "123"
        };
        registerValidator.Validate(shortPassword).IsValid.Should().BeFalse();
    }

    [Fact]
    public void VideoSearchModelValidator_CoversLengthAndYearBounds()
    {
        var validator = new VideoSearchModelValidator();
        validator.Validate(new VideoSearchModel { Page = 0, ItemPerPage = 10 }).IsValid.Should().BeFalse();
        validator.Validate(new VideoSearchModel { Page = 1, ItemPerPage = 101 }).IsValid.Should().BeFalse();
        validator.Validate(new VideoSearchModel { Page = 1, ItemPerPage = 10, Q = new string('q', 101) }).IsValid.Should().BeFalse();
        validator.Validate(new VideoSearchModel { Page = 1, ItemPerPage = 10, Title = new string('t', 256) }).IsValid.Should().BeFalse();
        validator.Validate(new VideoSearchModel { Page = 1, ItemPerPage = 10, CreateYearFrom = "1899" }).IsValid.Should().BeFalse();
        validator.Validate(new VideoSearchModel { Page = 1, ItemPerPage = 10, CreateYearTo = "9999" }).IsValid.Should().BeFalse();
        validator.Validate(new VideoSearchModel { Page = 1, ItemPerPage = 10, CreateYearFrom = "2020", CreateYearTo = "2021" }).IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DatabaseValidatorExtensions_CoverSlugAndNullableRules()
    {
        var context = TestDbContextHelper.CreateContext();
        context.Videos.Add(new VideoEntity { Id = 1, Title = "V", Slug = "my-slug", IsDeleted = false });
        context.Videos.Add(new VideoEntity { Id = 2, Title = "D", Slug = "gone", IsDeleted = true });
        var user = new UserEntity { Id = 3, UserName = "u" };
        context.Users.Add(user);
        context.Portfolios.Add(new PortfolioEntity { Id = 3, Name = "P", NickName = "nick", Freelancer = user });
        await context.SaveChangesAsync();

        var validator = new ProbeValidator(context);

        (await validator.ValidateAsync(new ProbeModel { Id = 1, Slug = " ", Nick = " " })).IsValid.Should().BeTrue();
        (await validator.ValidateAsync(new ProbeModel { Id = 1, OptionalId = null, Slug = "other", Nick = "other" })).IsValid.Should().BeTrue();
        (await validator.ValidateAsync(new ProbeModel { Id = 9, OptionalId = 9, Slug = "my-slug", Nick = "nick" })).IsValid.Should().BeFalse();
        (await validator.ValidateAsync(new ProbeModel { Id = 1, Slug = "My Slug", Nick = "fresh" })).IsValid.Should().BeTrue();
        (await validator.ValidateAsync(new ProbeModel { Id = 2, Slug = "my-slug", Nick = "nick" })).IsValid.Should().BeFalse();
    }

    private class ProbeModel
    {
        public long Id { get; set; }
        public long? OptionalId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Nick { get; set; } = string.Empty;
    }

    private class ProbeValidator : AbstractValidator<ProbeModel>
    {
        public ProbeValidator(Domain.AppDbContext db)
        {
            RuleFor(x => x.Id).MustExistAsync<ProbeModel, VideoEntity, long>(db);
            RuleFor(x => x.OptionalId).MustExistAsync<ProbeModel, VideoEntity, long>(db);
            RuleFor(x => x.Slug).UniqueSlugAsync<ProbeModel, VideoEntity, long>(db);
            RuleFor(x => x.Slug).UniqueSlugUpdateAsync<ProbeModel, VideoEntity, long>(db, x => x.Id);
            RuleFor(x => x.Nick).UniquePropertyAsync<ProbeModel, PortfolioEntity, long>(db, nameof(PortfolioEntity.NickName), "taken");
            RuleFor(x => x.Nick).UniquePropertyUpdateAsync<ProbeModel, PortfolioEntity, long>(db, nameof(PortfolioEntity.NickName), x => x.Id, "taken");
        }
    }
}
