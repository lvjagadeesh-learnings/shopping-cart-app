using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Events;

public static class EventPublisherExtensions
{
    /// <summary>
    /// Registers SNS-backed publishing when "Sns:TopicArn" is configured (staging/prod), otherwise
    /// falls back to a no-op logger so services run locally/in unit tests without AWS credentials.
    /// </summary>
    public static IServiceCollection AddEventPublishing(this IServiceCollection services, IConfiguration configuration)
    {
        var topicArn = configuration[$"{SnsOptions.SectionName}:TopicArn"];

        if (string.IsNullOrWhiteSpace(topicArn))
        {
            services.AddSingleton<IEventPublisher, NullEventPublisher>();
            return services;
        }

        services.Configure<SnsOptions>(configuration.GetSection(SnsOptions.SectionName));
        services.AddAWSService<IAmazonSimpleNotificationService>();
        services.AddSingleton<IEventPublisher, SnsEventPublisher>();
        return services;
    }
}
