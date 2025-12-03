namespace ShepherdEplan.Services.Common
{
    public sealed class LogService
    {
        private readonly string _logFilePath;

        public LogService(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Info(string message) => Write("INFO", message);
        public void Warn(string message) => Write("WARN", message);
        public void Error(string message, Exception? ex = null)
            => Write("ERROR", ex is null ? message : $"{message}{Environment.NewLine}{ex}");

        private void Write(string level, string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";

            var directory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.AppendAllLines(_logFilePath, new[] { line });
        }
    }
}
