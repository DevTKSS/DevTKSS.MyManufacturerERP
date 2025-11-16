using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    // Minimal compatibility shim for MediatR types used in this solution.
    // These are lightweight local definitions to avoid adding the MediatR package.

    public readonly struct Unit { public static readonly Unit Value = default; }

    public interface IRequest<TResponse> { }

    public interface IRequest : IRequest<Unit> { }

    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit> where TRequest : IRequest { }

    public interface INotification { }

    public interface INotificationHandler<in TNotification> where TNotification : INotification
    {
        Task Handle(TNotification notification, CancellationToken cancellationToken);
    }

    public interface IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }

    public interface IRequestPreProcessor<in TRequest> where TRequest : IRequest
    {
        Task Process(TRequest request, CancellationToken cancellationToken = default);
    }
}
