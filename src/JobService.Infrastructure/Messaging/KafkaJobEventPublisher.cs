using System.Text.Json;
using Confluent.Kafka;
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

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.Leader,
            MessageTimeoutMs = 5000,
            ApiVersionRequest = true,
            BrokerVersionFallback = "2.0.0",
            ApiVersionFallbackMs = 0,
            SecurityProtocol = SecurityProtocol.Plaintext
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishJobCreatedAsync(
        Guid jobReqId, string userId, string? userEmail,
        string companyName, string roleTitle,
        string? sourceUrl, string? companyCareerPortalUrl, string? jobDescription,
        DateOnly dateDiscovered, DateOnly? applicationExpiryDate, DateTime? interviewDate,
        DateTimeOffset occurredAt)
    {
        var payload = new
        {
            JobReqId = jobReqId,
            UserId = userId,
            UserEmail = userEmail,
            CompanyName = companyName,
            RoleTitle = roleTitle,
            SourceUrl = sourceUrl,
            CompanyCareerPortalUrl = companyCareerPortalUrl,
            JobDescription = jobDescription,
            DateDiscovered = dateDiscovered,
            ApplicationExpiryDate = applicationExpiryDate,
            InterviewDate = interviewDate,
            OccurredAt = occurredAt
        };

        await PublishAsync("job.application.created", jobReqId.ToString(), payload);
    }

    public async Task PublishJobUpdatedAsync(
        Guid jobReqId, string userId, string? userEmail,
        string companyName, string roleTitle,
        string? sourceUrl, string? companyCareerPortalUrl, string? jobDescription,
        DateOnly dateDiscovered, DateOnly? applicationExpiryDate, DateOnly? dateSubmitted, DateTime? interviewDate,
        DateTimeOffset occurredAt)
    {
        var payload = new
        {
            JobReqId = jobReqId,
            UserId = userId,
            UserEmail = userEmail,
            CompanyName = companyName,
            RoleTitle = roleTitle,
            SourceUrl = sourceUrl,
            CompanyCareerPortalUrl = companyCareerPortalUrl,
            JobDescription = jobDescription,
            DateDiscovered = dateDiscovered,
            ApplicationExpiryDate = applicationExpiryDate,
            DateSubmitted = dateSubmitted,
            InterviewDate = interviewDate,
            OccurredAt = occurredAt
        };

        await PublishAsync("job.application.updated", jobReqId.ToString(), payload);
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
