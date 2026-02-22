namespace DeliverySaaS.Application.Archiving;

public interface IArchiveService
{
    Task<int> RunArchiveAsync(CancellationToken cancellationToken = default);
}
