using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace backend.application.Common.Behaviours
{
    public sealed class LoggingBehaviour<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

        public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
            => _logger = logger;

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            var name = typeof(TRequest).Name;
            _logger.LogInformation("Handling {Request}", name);

            var sw = Stopwatch.StartNew();
            var response = await next();
            sw.Stop();

            if (sw.ElapsedMilliseconds > 500)
                _logger.LogWarning("Slow request: {Request} took {Elapsed}ms", name, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation("Handled {Request} in {Elapsed}ms", name, sw.ElapsedMilliseconds);

            return response;
        }
    }
}
