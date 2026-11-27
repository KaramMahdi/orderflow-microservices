namespace OrderFlow.Architecture.Tests;

/// <summary>
/// Package id prefixes, framework references and namespaces that the inner layers must never use.
/// A prefix matches the id itself and every id below it (for example Microsoft.EntityFrameworkCore.SqlServer).
/// </summary>
internal static class ForbiddenDependencies
{
    public const string EntityFrameworkCorePackage = "Microsoft.EntityFrameworkCore";
    public const string StackExchangeRedisPackage = "StackExchange.Redis";
    public const string RedisCachePackage = "Microsoft.Extensions.Caching.StackExchangeRedis";
    public const string RabbitMqPackage = "RabbitMQ.Client";
    public const string AspNetCorePackage = "Microsoft.AspNetCore";

    public const string AspNetCoreFramework = "Microsoft.AspNetCore.App";

    public const string EntityFrameworkCoreNamespace = "Microsoft.EntityFrameworkCore";
    public const string StackExchangeRedisNamespace = "StackExchange.Redis";
    public const string RabbitMqNamespace = "RabbitMQ.Client";
    public const string AspNetCoreNamespace = "Microsoft.AspNetCore";

    public static readonly string[] RedisAndRabbitMqPackages =
    [
        StackExchangeRedisPackage,
        RedisCachePackage,
        RabbitMqPackage,
    ];

    public static readonly string[] InfrastructurePackages =
    [
        EntityFrameworkCorePackage,
        StackExchangeRedisPackage,
        RedisCachePackage,
        RabbitMqPackage,
        AspNetCorePackage,
    ];

    public static readonly string[] InfrastructureNamespaces =
    [
        EntityFrameworkCoreNamespace,
        StackExchangeRedisNamespace,
        RabbitMqNamespace,
        AspNetCoreNamespace,
    ];
}
