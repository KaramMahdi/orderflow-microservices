using System.Reflection;
using Domain.Shared.Layer.Caching;
using Domain.Shared.Layer.Messaging;
using Domain.Shared.Layer.Storage;

namespace OrderFlow.Domain.Tests.SharedLayer;

/// <summary>
/// Guards the shape of the infrastructure abstractions declared in Domain.Shared.Layer (ADR-0017):
/// async with cancellation, streamed file content (ADR-0011), no file deletion (ADR-0005) and a
/// time to live on every cache entry.
/// </summary>
public sealed class InfrastructureAbstractionTests
{
    private const string AsyncSuffix = "Async";

    private static readonly string[] DeletingMethodPrefixes = ["Delete", "Remove"];

    public static TheoryData<Type> Abstractions =>
    [
        typeof(IFileStorage),
        typeof(ICacheService),
        typeof(IMessagePublisher),
    ];

    [Theory]
    [MemberData(nameof(Abstractions))]
    public void AbstractionIsAPublicInterface(Type abstraction)
    {
        Assert.True(abstraction.IsInterface);
        Assert.True(abstraction.IsPublic);
    }

    [Theory]
    [MemberData(nameof(Abstractions))]
    public void EveryMethodIsAsyncAndCancellable(Type abstraction)
    {
        foreach (var method in abstraction.GetMethods())
        {
            Assert.EndsWith(AsyncSuffix, method.Name, StringComparison.Ordinal);
            Assert.True(typeof(Task).IsAssignableFrom(method.ReturnType), $"{method.Name} must return a Task.");

            var lastParameter = method.GetParameters()[^1];
            Assert.Equal(typeof(CancellationToken), lastParameter.ParameterType);
            Assert.True(lastParameter.HasDefaultValue, $"{method.Name} must default its CancellationToken.");
        }
    }

    [Fact]
    public void FileStorageStreamsContentAndNeverUsesByteArraysOrMemoryStreams()
    {
        Type[] forbidden = [typeof(byte[]), typeof(MemoryStream)];

        var types = typeof(IFileStorage).GetMethods()
            .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType)
                .Append(method.ReturnType)
                .Concat(method.ReturnType.GetGenericArguments()));

        Assert.DoesNotContain(types, type => forbidden.Contains(type));
        Assert.Contains(typeof(Stream), typeof(IFileStorage).GetMethod(nameof(IFileStorage.SaveAsync))!
            .GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(typeof(Task<Stream>), typeof(IFileStorage).GetMethod(nameof(IFileStorage.OpenReadAsync))!.ReturnType);
    }

    [Fact]
    public void FileStorageCannotDeleteContent()
    {
        var deletingMethods = typeof(IFileStorage).GetMethods()
            .Where(method => DeletingMethodPrefixes.Any(prefix => method.Name.StartsWith(prefix, StringComparison.Ordinal)));

        Assert.Empty(deletingMethods);
    }

    [Fact]
    public void FileStorageSaveReturnsTheSizeAndTheHash()
    {
        Assert.Equal(
            typeof(Task<FileWriteResult>),
            typeof(IFileStorage).GetMethod(nameof(IFileStorage.SaveAsync))!.ReturnType);
    }

    [Fact]
    public void FileWriteResultCarriesTheSizeAndTheHash()
    {
        const long size = 1024;
        var sha256 = new string('a', 64);

        var result = new FileWriteResult(size, sha256);

        Assert.Equal(size, result.Size);
        Assert.Equal(sha256, result.Sha256);
        Assert.Equal(new FileWriteResult(size, sha256), result);
    }

    [Fact]
    public void EveryCacheWriteRequiresATimeToLive()
    {
        var timeToLive = typeof(ICacheService).GetMethod(nameof(ICacheService.SetAsync))!
            .GetParameters()
            .Single(parameter => parameter.ParameterType == typeof(TimeSpan));

        Assert.False(timeToLive.HasDefaultValue);
        Assert.False(timeToLive.IsOptional);
    }

    [Fact]
    public void CacheValuesAreReferenceTypesSoAMissIsNull()
    {
        foreach (var method in typeof(ICacheService).GetMethods().Where(method => method.IsGenericMethodDefinition))
        {
            var attributes = method.GetGenericArguments()[0].GenericParameterAttributes;

            Assert.True(
                attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint),
                $"{method.Name} must constrain its value type to a class.");
        }
    }

    [Fact]
    public void PublisherPublishesOneEventAtATime()
    {
        var method = typeof(IMessagePublisher).GetMethod(nameof(IMessagePublisher.PublishAsync))!;

        Assert.True(method.IsGenericMethodDefinition);
        Assert.Equal(method.GetGenericArguments()[0], method.GetParameters()[0].ParameterType);
    }
}
