namespace DeliverySaaS.Application.Common.Interfaces;

public interface IReplayProtectionService
{
    bool IsReplay(string key, TimeSpan ttl);
}
