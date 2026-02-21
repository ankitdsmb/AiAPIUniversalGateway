using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Domain.Ports;

public interface IRequestLogRepository
{
    ValueTask AddAsync(RequestLog requestLog, CancellationToken cancellationToken);
}
