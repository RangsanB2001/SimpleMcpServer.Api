using Microsoft.Extensions.Configuration;

namespace SimpleMcpServer.Infrastructure.Options;

public sealed class DatabaseSchemaOptions
{
    public const string SectionName = "DatabaseSchema";

    public static readonly string[] DefaultExcludedSchemas =
    [
        "information_schema",
        "mysql",
        "performance_schema",
        "sys"
    ];

    public string DefaultTarget { get; init; } = "psims";

    public IReadOnlyList<string> ExcludedSchemas { get; init; } = DefaultExcludedSchemas;

    public IReadOnlyDictionary<string, DatabaseSchemaTargetOptions> Targets { get; init; } =
        new Dictionary<string, DatabaseSchemaTargetOptions>(StringComparer.OrdinalIgnoreCase);

    public static DatabaseSchemaOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var defaultTarget = Normalize(section["DefaultTarget"]) ?? "psims";
        var excludedSchemas = ReadStringArray(section.GetSection("ExcludedSchemas"));
        var targets = ReadTargets(section.GetSection("Targets"), defaultTarget);

        if (targets.Count == 0)
        {
            targets[defaultTarget] = new DatabaseSchemaTargetOptions
            {
                ConnectionStringName = "DefaultConnection",
                DisplayName = "PSIMS default"
            };
        }

        if (!targets.ContainsKey(defaultTarget))
        {
            throw new InvalidOperationException(
                $"DatabaseSchema default target '{defaultTarget}' is not configured. " +
                $"Configured targets: {string.Join(", ", targets.Keys.Order(StringComparer.OrdinalIgnoreCase))}.");
        }

        return new DatabaseSchemaOptions
        {
            DefaultTarget = defaultTarget,
            ExcludedSchemas = excludedSchemas.Count > 0
                ? excludedSchemas
                : DefaultExcludedSchemas,
            Targets = targets
        };
    }

    private static Dictionary<string, DatabaseSchemaTargetOptions> ReadTargets(
        IConfigurationSection section,
        string defaultTarget)
    {
        var targets = new Dictionary<string, DatabaseSchemaTargetOptions>(StringComparer.OrdinalIgnoreCase);

        foreach (var targetSection in section.GetChildren())
        {
            var alias = Normalize(targetSection.Key);

            if (alias is null)
            {
                continue;
            }

            var connectionStringName =
                Normalize(targetSection["ConnectionStringName"]) ??
                (alias.Equals(defaultTarget, StringComparison.OrdinalIgnoreCase)
                    ? "DefaultConnection"
                    : alias);

            targets[alias] = new DatabaseSchemaTargetOptions
            {
                ConnectionStringName = connectionStringName,
                DisplayName = Normalize(targetSection["DisplayName"]) ?? alias
            };
        }

        return targets;
    }

    private static IReadOnlyList<string> ReadStringArray(IConfigurationSection section) =>
        section
            .GetChildren()
            .Select(static item => Normalize(item.Value))
            .Where(static value => value is not null)
            .Select(static value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}

public sealed class DatabaseSchemaTargetOptions
{
    public string ConnectionStringName { get; init; } = "DefaultConnection";

    public string DisplayName { get; init; } = string.Empty;
}
