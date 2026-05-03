namespace SimpleMcpServer.Application.Abstractions;

public interface IDataProvider<T>
{
    Task<T?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
}
