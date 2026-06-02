namespace OP.GATEWAY.Helpers
{
    public static class AppLogHelper
    {
        private static readonly string LogDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        private static readonly string LogFilePath = Path.Combine(LogDirectory, $"log_{DateTime.Today:yyyy-MM-dd}.txt");

        static AppLogHelper()
        {
            // Ensure the Logs folder exists
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        public static void WriteLog(string message)
        {
            string log = $"\n\n{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            File.AppendAllText(LogFilePath, log + Environment.NewLine);
        }
    }
}
