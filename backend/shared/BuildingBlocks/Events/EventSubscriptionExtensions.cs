using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Events;

public static class EventSubscriptionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="THandler"/> for <typeparamref name="TEvent"/>. When
    /// "{queueUrlConfigKey}" is configured (staging/prod), also starts an SQS long-poll consumer;
    /// otherwise the handler is registered but nothing polls it, so services run locally/in unit
    /// tests without AWS credentials (handlers are tested directly instead).
    /// </summary>
    public static IServiceCollection AddEventSubscriber<TEvent, THandler>(
        this IServiceCollection services,
        IConfiguration configuration,
        string queueUrlConfigKey)
        where TEvent : IntegrationEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<IEventHandler<TEvent>, THandler>();

        var queueUrl = configuration[queueUrlConfigKey];
        if (string.IsNullOrWhiteSpace(queueUrl))
        {
            return services;
        }

        services.AddAWSService<IAmazonSQS>();
        services.AddSingleton(new SqsQueueUrl<TEvent>(queueUrl));
        services.AddHostedService<SqsEventConsumer<TEvent>>();
        return services;
    }
}
