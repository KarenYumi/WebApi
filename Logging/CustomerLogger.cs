namespace MinhaAPI.Logging
{
    public class CustomerLogger : ILogger //implementa a interface log com os métodos necessários para registrar as mensagens
    {
        readonly string loggerName;
        readonly CustomLoggerProviderConfiguration loggerConfig;
        public CustomerLogger(string name, CustomLoggerProviderConfiguration config)
        {
            loggerName = name;//recebe o nome da categoria
            loggerConfig = config;
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel == LogLevel.Information;//verifica se o nível de log desejado está habilitado com base na configuração, caso n esteja n será registrado
        }
        public IDisposable BeginScope<TState>(TState state)//cria um escopo para mensagens de log NÂO EsTAMOS USANDO POR ISSO ESTÀ NULL, PORQUE ELE È OBRIGAORIO
        {
            return null;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
                Exception exception, Func<TState, Exception, string> formatter)
        {
            string mensagem = $"{logLevel.ToString()}: {eventId.Id} - {formatter(state, exception)}"; //verifica se o nivel de log é permitido e se for, vai formatar a mensagem

            EscreverTextoNoArquivo(mensagem);
        }

        private void EscreverTextoNoArquivo(string mensagem) //ecreve a mensagem, descreve o caminho
        {
            string caminhoArquivoLog = @"C:\Users\ksilva\Documents\Curso_WebAPI\MinhaAPI - Copia\_Log.txt";
            using (StreamWriter streamWriter = new StreamWriter(caminhoArquivoLog, true))
            {
                try
                {
                    streamWriter.WriteLine(mensagem);
                    streamWriter.Close();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}