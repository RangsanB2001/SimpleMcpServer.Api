namespace SimpleMcpServer.Domain.Entities;

public class DatabaseTableSchema
{
    public string TableSchema { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string TableType { get; set; } = string.Empty;

    public string? Engine { get; set; }

    public long? TableRows { get; set; }

    public DateTime? CreateTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? TableComment { get; set; }

    public IReadOnlyList<DatabaseColumnSchema> Columns { get; set; } = [];
}
