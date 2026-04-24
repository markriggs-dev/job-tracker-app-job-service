using System.Text.Json;
using Confluent.Kafka;
using JobService.Core.Enums;
using JobService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JobService.Infrastructure.Messaging;

public class KafkaJobEventPublisher : IJobEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaJobEventPublisher> _logger;

    public KafkaJobEventPublisher(
        IConfiguration configuration,
        ILogger<KafkaJobEventPublisher> logger)
    {
        _logger = logger;

        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? "localhost:9092";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.Leader,
            MessageTimeoutMs = 5000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishStatusChangedAsync(
        Guid jobReqId,
        string userId,
        JobStatus previousStatus,
        JobStatus newStatus)
    {
        var payload = new
        {
            JobReqId = jobReqId,
            UserId = userId,
            PreviousStatus = previousStatus.ToString(),
            NewStatus = newStatus.ToString(),
            OccurredAt = DateTimeOffset.UtcNow
        };

        await PublishAsync("job.status.changed", jobReqId.ToString(), payload);
    }

    public async Task PublishApplicationSubmittedAsync(Guid jobReqId, string userId)
    {
        var payload = new
        {
            JobReqId = jobReqId,
            UserId = userId,
            OccurredAt = DateTimeOffset.UtcNow
        };

        await PublishAsync("job.application.submitted", jobReqId.ToString(), payload);
    }

    private async Task PublishAsync(string topic, string key, object payload)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = key,
                Value = JsonSerializer.Serialize(payload)
            };

            var result = await _producer.ProduceAsync(topic, message);

            _logger.LogInformation(
                "Published event to topic {Topic} partition {Partition} offset {Offset}",
                result.Topic, result.Partition, result.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex,
                "Failed to publish event to topic {Topic} with key {Key}",
                topic, key);
            throw;
        }
    }
}
