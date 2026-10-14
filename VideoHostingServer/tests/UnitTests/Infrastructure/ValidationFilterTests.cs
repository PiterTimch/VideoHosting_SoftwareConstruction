using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace UnitTests.Infrastructure;

public class ValidationFilterTests
{
    public class SampleModel
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenValidationFails_ReturnsBadRequest()
    {
        var model = new SampleModel { Name = "" };
        var validatorMock = new Mock<IValidator<SampleModel>>();
        var failures = new List<ValidationFailure> { new("Name", "Name is required") };
        validatorMock.Setup(v => v.ValidateAsync(model, default))
            .ReturnsAsync(new ValidationResult(failures));

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<SampleModel>)))
            .Returns(validatorMock.Object);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { { "model", model } },
            new object());

        bool nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        };

        var filter = new ValidationFilter();
        await filter.OnActionExecutionAsync(actionExecutingContext, next);

        nextCalled.Should().BeFalse();
        actionExecutingContext.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenValidationSucceeds_CallsNext()
    {
        var model = new SampleModel { Name = "Valid Name" };
        var validatorMock = new Mock<IValidator<SampleModel>>();
        validatorMock.Setup(v => v.ValidateAsync(model, default))
            .ReturnsAsync(new ValidationResult());

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<SampleModel>)))
            .Returns(validatorMock.Object);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { { "model", model } },
            new object());

        bool nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        };

        var filter = new ValidationFilter();
        await filter.OnActionExecutionAsync(actionExecutingContext, next);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenArgumentIsNull_SkipsValidationAndCallsNext()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { { "model", null } },
            new object());

        bool nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        };

        var filter = new ValidationFilter();
        await filter.OnActionExecutionAsync(actionExecutingContext, next);

        nextCalled.Should().BeTrue();
        actionExecutingContext.Result.Should().BeNull();
    }
}
