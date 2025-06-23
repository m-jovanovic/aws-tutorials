using Stackable.Api.Models;

namespace Stackable.Api.Services;

public class MessageHandlerFactory : IMessageHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MessageHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEventHandler<TEvent>? GetHandler<TEvent>(IServiceProvider? serviceProvider = null)
        where TEvent : BaseEvent
    {
        var sp = serviceProvider ?? _serviceProvider;

        return sp.GetService<IEventHandler<TEvent>>();
    }
} 