using Stackable.Api.Models;

namespace Stackable.Api.Services;

public interface IMessagePublisher
{
    Task PublishEventAsync<TEvent>(TEvent @event, string queueUrl) where TEvent : BaseEvent;
} 