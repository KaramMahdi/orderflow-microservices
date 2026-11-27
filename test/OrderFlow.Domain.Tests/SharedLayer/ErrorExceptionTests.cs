using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;

namespace OrderFlow.Domain.Tests.SharedLayer;

/// <summary>
/// <see cref="ErrorException"/> carries a code, a message key and the format arguments (ADR-0021).
/// </summary>
public sealed class ErrorExceptionTests
{
    private const string MessageName = "TestMessage";
    private const string ListName = "TestList";

    private static readonly MessageKey Key = new(Modules.Common, MessageName);

    [Fact]
    public void CarriesTheCodeTheKeyAndTheArgs()
    {
        var firstArg = Guid.CreateVersion7();
        const int secondArg = 42;

        var exception = new ErrorException(ErrorCode.Conflict, Key, firstArg, secondArg);

        Assert.Equal(ErrorCode.Conflict, exception.Code);
        Assert.Same(Key, exception.Key);
        Assert.Equal([firstArg, secondArg], exception.Args);
    }

    [Fact]
    public void ArgsAreEmptyWhenNoneAreGiven()
    {
        var exception = new ErrorException(ErrorCode.NotFound, Key);

        Assert.Empty(exception.Args);
    }

    [Fact]
    public void ArgsMayContainNull()
    {
        var exception = new ErrorException(ErrorCode.BadRequest, Key, null, 1);

        Assert.Equal([null, 1], exception.Args);
    }

    [Fact]
    public void KeepsTheKeyWithItsList()
    {
        var key = new MessageKey(Modules.Orders, MessageName, ListName);

        var exception = new ErrorException(ErrorCode.ValidationFailed, key);

        Assert.Equal(Modules.Orders, exception.Key.Module);
        Assert.Equal(MessageName, exception.Key.Name);
        Assert.Equal(ListName, exception.Key.List);
    }

    [Fact]
    public void RejectsANullKey()
    {
        Assert.Throws<ArgumentNullException>(() => new ErrorException(ErrorCode.BadRequest, null!));
    }

    [Fact]
    public void RejectsANullArgsArray()
    {
        Assert.Throws<ArgumentNullException>(() => new ErrorException(ErrorCode.BadRequest, Key, null!));
    }

    [Fact]
    public void IsAnException()
    {
        Assert.IsAssignableFrom<Exception>(new ErrorException(ErrorCode.InternalServerError, Key));
    }
}
