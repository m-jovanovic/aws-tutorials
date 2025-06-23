using Stackable.Api.Models;

namespace Stackable.Api.Services;

public interface IMessageHandlerFactory
{
    IEventHandler<TEvent>? GetHandler<TEvent>(IServiceProvider? serviceProvider = null) where TEvent : BaseEvent;
} 