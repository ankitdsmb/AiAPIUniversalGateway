using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Abstractions;

public interface ITaskClassifier
{
    TaskType Classify(GatewayRequest request);
}
