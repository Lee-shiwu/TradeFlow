using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TradeFlow.Api.Infrastructure.Identity;

namespace TradeFlow.IntegrationTests.Infrastructure.Identity;

public sealed class TemporaryIdentityResolverTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Testing")]
    public void TryResolve_WithValidHeadersInAllowedEnvironment_ReturnsIdentity(
        string environmentName)
    {
        DefaultHttpContext httpContext =
            new();

        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        httpContext.Request.Headers[
            "X-Organisation-Id"] =
            organisationId.ToString();

        httpContext.Request.Headers[
            "X-User-Id"] =
            userId.ToString();

        TestHostEnvironment environment =
            new(environmentName);

        IResult? errorResult =
            TemporaryIdentityResolver.TryResolve(
                httpContext,
                environment,
                out TemporaryIdentity identity);

        Assert.Null(errorResult);

        Assert.Equal(
            organisationId,
            identity.OrganisationId);

        Assert.Equal(
            userId,
            identity.UserId);
    }

    [Fact]
    public void TryResolve_OutsideAllowedEnvironment_ReturnsNotImplemented()
    {
        DefaultHttpContext httpContext =
            new();

        TestHostEnvironment environment =
            new(Environments.Production);

        IResult? errorResult =
            TemporaryIdentityResolver.TryResolve(
                httpContext,
                environment,
                out TemporaryIdentity identity);

        Assert.NotNull(errorResult);

        IStatusCodeHttpResult statusResult =
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(
                errorResult);

        Assert.Equal(
            StatusCodes.Status501NotImplemented,
            statusResult.StatusCode);

        Assert.Equal(
            Guid.Empty,
            identity.OrganisationId);

        Assert.Equal(
            Guid.Empty,
            identity.UserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void TryResolve_WithInvalidOrganisationHeader_ReturnsBadRequest(
        string? organisationHeader)
    {
        DefaultHttpContext httpContext =
            new();

        if (organisationHeader is not null)
        {
            httpContext.Request.Headers[
                "X-Organisation-Id"] =
                organisationHeader;
        }

        httpContext.Request.Headers[
            "X-User-Id"] =
            Guid.NewGuid().ToString();

        TestHostEnvironment environment =
            new("Testing");

        IResult? errorResult =
            TemporaryIdentityResolver.TryResolve(
                httpContext,
                environment,
                out TemporaryIdentity identity);

        Assert.NotNull(errorResult);

        IStatusCodeHttpResult statusResult =
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(
                errorResult);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            statusResult.StatusCode);

        Assert.Equal(
            Guid.Empty,
            identity.OrganisationId);

        Assert.Equal(
            Guid.Empty,
            identity.UserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void TryResolve_WithInvalidUserHeader_ReturnsBadRequest(
        string? userHeader)
    {
        DefaultHttpContext httpContext =
            new();

        httpContext.Request.Headers[
            "X-Organisation-Id"] =
            Guid.NewGuid().ToString();

        if (userHeader is not null)
        {
            httpContext.Request.Headers[
                "X-User-Id"] =
                userHeader;
        }

        TestHostEnvironment environment =
            new("Testing");

        IResult? errorResult =
            TemporaryIdentityResolver.TryResolve(
                httpContext,
                environment,
                out TemporaryIdentity identity);

        Assert.NotNull(errorResult);

        IStatusCodeHttpResult statusResult =
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(
                errorResult);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            statusResult.StatusCode);

        Assert.Equal(
            Guid.Empty,
            identity.OrganisationId);

        Assert.Equal(
            Guid.Empty,
            identity.UserId);
    }

    private sealed class TestHostEnvironment(
        string environmentName)
        : IHostEnvironment
    {
        public string EnvironmentName { get; set; } =
            environmentName;

        public string ApplicationName { get; set; } =
            "TradeFlow.Api";

        public string ContentRootPath { get; set; } =
            AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider
        {
            get;
            set;
        } = new NullFileProvider();
    }
}
