using Stackable.Api.Models;

namespace Stackable.Api.Services;

public interface IEventHandler<in TEvent> where TEvent : BaseEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
} 