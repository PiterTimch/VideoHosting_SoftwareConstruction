using Application.Models.Comments;
using Application.Models.Portfolio;
using Application.Models.User;
using Application.Models.Video;
using Application.Validators.Account;
using Application.Validators.Comments;
using Application.Validators.Portfolio;
using Application.Validators.User;
using Application.Validators.Video;
using FluentAssertions;
using Moq;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Validators;

public class ValidatorsTests
{
    [Fact]
    public void CreateCommentValidator_ValidatesContentProperly()
    {
        var validator = new CreateCommentValidator();

        var resEmpty = validator.Validate(new Application.Features.Comments.Commands.CreateComments.CreateCommentCommand("", 1, null));
        resEmpty.IsValid.Should().BeFalse();

        var resTooLong = validator.Validate(new Application.Features.Comments.Commands.CreateComments.CreateCommentCommand(new string('a', 2001), 1, null));
        resTooLong.IsValid.Should().BeFalse();

        var resZeroVideo = validator.Validate(new Application.Features.Comments.Commands.CreateComments.CreateCommentCommand("Valid", 0, null));
        resZeroVideo.IsValid.Should().BeFalse();

        var resValid = validator.Validate(new Application.Features.Comments.Commands.CreateComments.CreateCommentCommand("Valid comment", 1, null));
        resValid.IsValid.Should().BeTrue();
    }

    [Fact]
    public void VideoSearchModelValidator_ValidatesYearsAndPagination()
    {
        var validator = new VideoSearchModelValidator();

        var validModel = new VideoSearchModel
        {
            Page = 1,
            ItemPerPage = 10,
            CreateYearFrom = "2020",
            CreateYearTo = "2024"
        };
        validator.Validate(validModel).IsValid.Should().BeTrue();

        var invalidYear = new VideoSearchModel { CreateYearFrom = "invalid" };
        validator.Validate(invalidYear).IsValid.Should().BeFalse();

        var rangeInvalid = new VideoSearchModel { CreateYearFrom = "2025", CreateYearTo = "2020" };
        validator.Validate(rangeInvalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UserEditModelValidator_ValidatesImageProperly()
    {
        var validator = new UserEditModelValidator();

        var validModel = new UserEditModel { FirstName = "John", LastName = "Doe" };
        validator.Validate(validModel).IsValid.Should().BeTrue();

        var invalidFileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        invalidFileMock.Setup(f => f.FileName).Returns("file.exe");
        invalidFileMock.Setup(f => f.ContentType).Returns("application/x-msdownload");
        var invalidModel = new UserEditModel { Image = invalidFileMock.Object };
        validator.Validate(invalidModel).IsValid.Should().BeFalse();
    }

    [Fact]
    public void PortfolioSearchModelValidator_ValidatesSuccessfully()
    {
        var validator = new PortfolioSearchModelValidator();
        var model = new PortfolioSearchModel { Page = 1, ItemPerPage = 10 };
        validator.Validate(model).IsValid.Should().BeTrue();
    }

    [Fact]
    public void AccountRegisterModelValidator_ValidatesRequiredFields()
    {
        var validator = new AccountRegisterModelValidator();

        var emptyModel = new Application.Models.Account.AccountRegisterModel();
        var emptyRes = validator.Validate(emptyModel);
        emptyRes.IsValid.Should().BeFalse();
        emptyRes.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        emptyRes.Errors.Should().Contain(e => e.PropertyName == "Email");
        emptyRes.Errors.Should().Contain(e => e.PropertyName == "Password");

        var validModel = new Application.Models.Account.AccountRegisterModel
        {
            FirstName = "Alice",
            Email = "alice@example.com",
            Password = "Password123!"
        };
        validator.Validate(validModel).IsValid.Should().BeTrue();
    }
}
