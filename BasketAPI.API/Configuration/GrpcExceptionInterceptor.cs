using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace BasketAPI.API.Configuration;

public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var call = continuation(request, context);

        return new AsyncUnaryCall<TResponse>(
            HandleResponse(call.ResponseAsync),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> responseTask)
    {
        try
        {
            return await responseTask;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC call failed with status code {StatusCode}: {Message}",
                ex.StatusCode, ex.Message);

            switch (ex.StatusCode)
            {
                case StatusCode.NotFound:
                    throw new NotFoundException("The requested resource was not found", ex);
                case StatusCode.Internal:
                    throw new InvalidOperationException("Internal server error in gRPC service", ex);
                case StatusCode.Unavailable:
                    throw new ServiceUnavailableException("The gRPC service is currently unavailable", ex);
                default:
                    throw;
            }
        }
    }
}

public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
