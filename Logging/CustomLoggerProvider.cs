using System.Collections.Concurrent;

namespace MinhaAPI.Logging
{
    public class CustomLoggerProvider : ILoggerProvider //usada pra criar instacia de loggers personalizada
    {
        readonly CustomLoggerProviderConfiguration loggerConfig;
        readonly ConcurrentDictionary<string, CustomerLogger> loggers = new ConcurrentDictionary<string, CustomerLogger>();
        public CustomLoggerProvider(CustomLoggerProviderConfiguration config)
        {
            loggerConfig = config;
        }
        public ILogger CreateLogger(string categoryName)
        { 
            return loggers.GetOrAdd(categoryName, name => new CustomerLogger(name, loggerConfig));//retorna um logger existente caso n exista ele vai criar
        }
        public void Dispose() 
        { 
            loggers.Clear(); 
        }
    }
}
