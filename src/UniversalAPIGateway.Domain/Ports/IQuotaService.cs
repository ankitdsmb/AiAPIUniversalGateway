using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Domain.Ports;

public interface IQuotaService
{
    ValueTask<QuotaInfo> GetQuotaAsync(string subject, CancellationToken cancellationToken);

    ValueTask<bool> TryReserveAsync(string subject, long units, CancellationToken cancellationToken);

    ValueTask RecordUsageAsync(string subject, long units, CancellationToken cancellationToken);
}
