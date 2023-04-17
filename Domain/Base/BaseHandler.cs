using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Domain.Base
{
    internal abstract class BaseHandler<TRequest, TResult> : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        protected readonly ILogger _logger;
        protected readonly string _name;

        protected BaseHandler(ILogger logger)
        {
            _logger = logger;
            _name = GetType().Name;
        }

        public async Task<TResult> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug($"Start to execute {_name}. Input {request}");

                TResult result = await HandleInternal(request, cancellationToken);

                _logger.LogDebug($"Executed {_name}. Result {result}");

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception raised. Input: {request}");
                throw;
            }
        }

        protected abstract Task<TResult> HandleInternal(TRequest reqest, CancellationToken cancellationToken);
    }
}