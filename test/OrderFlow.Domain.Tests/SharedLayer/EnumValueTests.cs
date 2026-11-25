using Domain.Shared.Layer.Context;
using Domain.Shared.Layer.Errors;

namespace OrderFlow.Domain.Tests.SharedLayer;

/// <summary>
/// Enum numbers are a contract with the database and the clients: they must match the decisions
/// exactly and never be reordered (ADR-0021 for ErrorCode, ADR-0013 for RequestSource).
/// </summary>
public sealed class EnumValueTests
{
    private static readonly Dictionary<ErrorCode, int> ExpectedErrorCodes = new()
    {
        [ErrorCode.BadRequest] = 400,
        [ErrorCode.Unauthorized] = 401,
        [ErrorCode.Forbidden] = 403,
        [ErrorCode.NotFound] = 404,
        [ErrorCode.Conflict] = 409,
        [ErrorCode.ValidationFailed] = 422,
        [ErrorCode.TooManyRequests] = 429,
        [ErrorCode.InternalServerError] = 500,
        [ErrorCode.ServiceUnavailable] = 503,
    };

    private static readonly Dictionary<RequestSource, byte> ExpectedRequestSources = new()
    {
        [RequestSource.Unknown] = 0,
        [RequestSource.Web] = 1,
        [RequestSource.Mobile] = 2,
        [RequestSource.Api] = 3,
    };

    public static TheoryData<ErrorCode, int> ErrorCodeValues =>
        new(ExpectedErrorCodes.Select(pair => (pair.Key, pair.Value)));

    public static TheoryData<RequestSource, byte> RequestSourceValues =>
        new(ExpectedRequestSources.Select(pair => (pair.Key, pair.Value)));

    [Theory]
    [MemberData(nameof(ErrorCodeValues))]
    public void ErrorCodeHasTheSpecifiedNumber(ErrorCode code, int expected)
    {
        Assert.Equal(expected, (int)code);
    }

    [Fact]
    public void ErrorCodeHasOnlyTheSpecifiedValues()
    {
        Assert.Equal(
            ExpectedErrorCodes.Values.Order(),
            Enum.GetValues<ErrorCode>().Select(code => (int)code).Order());
    }

    [Theory]
    [MemberData(nameof(RequestSourceValues))]
    public void RequestSourceHasTheSpecifiedNumber(RequestSource source, byte expected)
    {
        Assert.Equal(expected, (byte)source);
    }

    [Fact]
    public void RequestSourceHasOnlyTheSpecifiedValues()
    {
        Assert.Equal(
            ExpectedRequestSources.Values.Order(),
            Enum.GetValues<RequestSource>().Select(source => (byte)source).Order());
    }

    [Fact]
    public void RequestSourceIsStoredAsAByte()
    {
        Assert.Equal(typeof(byte), Enum.GetUnderlyingType(typeof(RequestSource)));
    }

    [Fact]
    public void RequestSourceDefaultIsUnknown()
    {
        Assert.Equal(RequestSource.Unknown, default(RequestSource));
    }
}
