using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace ProcessManagement.Services.Logging
{
    public static class LoggerExtensions
    {
        public static void LogErrorWithContext(this ILogger logger, Exception ex, string message = "",
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            logger.LogError(ex, "{Message} | At {Member} in {File}:{Line}", message, member, file, line);
        }
    }
}


