using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace ProcessManagement.Services.Logging
{
    public class LoggingCircuitHandler : CircuitHandler
    {
        private readonly ILogger<LoggingCircuitHandler> _logger;

        public LoggingCircuitHandler(ILogger<LoggingCircuitHandler> logger)
        {
            _logger = logger;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Circuit {Id} opened", circuit.Id);
            return Task.CompletedTask;
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Circuit {Id} closed", circuit.Id);
            return Task.CompletedTask;
        }
    }
}


