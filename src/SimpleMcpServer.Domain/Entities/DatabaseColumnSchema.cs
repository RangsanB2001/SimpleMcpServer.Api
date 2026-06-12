namespace SimpleMcpServer.Domain.Entities;

public class DatabaseColumnSchema
{
    public string TableName { get; set; } = string.Empty;

    public string ColumnName { get; set; } = string.Empty;

    public int OrdinalPosition { get; set; }

    public string DataType { get; set; } = string.Empty;

    public string ColumnType { get; set; } = string.Empty;

    public string IsNullable { get; set; } = string.Empty;

    public string? ColumnKey { get; set; }

    public string? ColumnDefault { get; set; }

    public string? Extra { get; set; }

    public string? ColumnComment { get; set; }
}
