using SendGrid;
using System.Text.Json;

namespace Sammlerplattform.Services
{
    public interface ITrackEventsCSV
    {
        void TrackException(Exception exception, string context = "", Dictionary<string, object>? metrics = null, string? userId = null);
        void TrackError(string context = "", Dictionary<string, object>? metrics = null, string? userId = null);
        void TrackInfo(string context = "", Dictionary<string, object>? metrics = null, string? userId = null);
        void TrackUserBehavior(string eventName, Dictionary<string, object>? metrics = null, string? userId = null);
        void TrackPerformance(string operation, TimeSpan duration, string? userId = null);
        void TrackEmailResponse(Response response, string toEmail, string subject);
    }

    public class EventTracker : ITrackEventsCSV
    {
        private readonly string _logDirectory;
        private readonly bool _enableConsoleOutput;

        public EventTracker(string logDirectory = "logs", bool enableConsoleOutput = true)
        {
            _logDirectory = logDirectory;
            _enableConsoleOutput = enableConsoleOutput;

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void TrackException(Exception exception, string context = "", Dictionary<string, object>? metrics = null, string? userId = null)
        {
            var errorData = new
            {
                Timestamp = DateTime.Now.ToString("O"),
                UserId = userId ?? "anonymous",
                Context = context,
                exception.Message,
                exception.StackTrace,
                ExceptionType = exception.GetType().Name,
                InnerException = exception.InnerException != null ? new
                {
                    Type = exception.InnerException.GetType().Name,
                    exception.InnerException.Message
                } : null,
                Metrics = metrics ?? []
            };

            WriteToFile("errors", errorData);
        }

        public void TrackError(string context = "", Dictionary<string, object>? metrics = null, string? userId = null)
        {
            var errorData = new
            {
                Type = "ERROR",
                Timestamp = DateTime.Now,
                UserId = userId ?? "anonymous",
                Context = context,
                Metrics = metrics ?? []
            };

            WriteToFile("warnings", errorData);
        }

        public void TrackInfo(string context = "", Dictionary<string, object>? metrics = null, string? userId = null)
        {
            var errorData = new
            {
                Type = "Info",
                Timestamp = DateTime.Now,
                UserId = userId ?? "anonymous",
                Context = context,
                Metrics = metrics ?? []
            };

            WriteToFile("infos", errorData);
        }

        public void TrackUserBehavior(string eventName, Dictionary<string, object>? metrics = null, string? userId = null)
        {
            var behaviorData = new
            {
                Type = "BEHAVIOR",
                Timestamp = DateTime.Now,
                UserId = userId ?? "anonymous",
                EventName = eventName,
                Metrics = metrics ?? []
            };

            WriteToFile("behavior", behaviorData);
        }

        public void TrackPerformance(string operation, TimeSpan duration, string? userId = null)
        {
            var perfData = new
            {
                Type = "PERFORMANCE",
                Timestamp = DateTime.Now,
                UserId = userId ?? "anonymous",
                Operation = operation,
                DurationMs = duration.TotalMilliseconds
            };

            WriteToFile("performance", perfData);
        }

        public void TrackEmailResponse(Response response, string toEmail, string subject)
        {
            var emailData = new
            {
                Type = "EMAIL",
                Timestamp = DateTime.Now,
                ToEmail = toEmail,
                Subject = subject,
                StatusCode = (int)response.StatusCode,
                response.Headers,
                response.Body
            };
            WriteToFile("emails", emailData);
        }

        private void WriteToFile(string category, object data)
        {
            try
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string filename = Path.Combine(_logDirectory, $"{category}_{date}.log");

                string json = JsonSerializer.Serialize(data);

                File.AppendAllText(filename, json + Environment.NewLine);

                if (_enableConsoleOutput)
                {
                    Console.WriteLine($"[{category}] {json}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write tracking data: {ex.Message}");
            }
        }
    }
}
