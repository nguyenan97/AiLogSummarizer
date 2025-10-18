using Microsoft.Extensions.Configuration;
using Serilog;

namespace Infrastructure.Services.FakeLogs
{


    public static class ErrorLogSimulator
    {

        private static readonly Random Random = new Random();
        public static async Task SimulateRandomErrors(IConfiguration configuration)
        {
            var errorActions = new List<(string, Action)>
        {
            ("ConfigurationError", () => SimulateConfigurationError(configuration)),
            ("NullReferenceError", SimulateNullReferenceError),
            ("ConnectionStringError", () => SimulateConnectionStringError(configuration)),
            ("DivideByZeroError", SimulateDivideByZeroError),
            ("FileNotFoundError", SimulateFileNotFoundError),
            ("UnauthorizedAccessError", SimulateUnauthorizedAccessError),
            ("TimeoutError", SimulateTimeoutError),
            ("ArgumentError", SimulateArgumentError),
            ("FormatError", SimulateFormatError),
            ("KeyNotFoundError", SimulateKeyNotFoundError),
            ("InvalidCastError", SimulateInvalidCastError),
            ("OverflowError", SimulateOverflowError),
            ("HttpRequestError", async () => await SimulateHttpRequestError()),
            ("CustomAppError", SimulateCustomAppError)
        };

            // Randomly select 1-3 errors to simulate
            int errorCount = Random.Next(1, 4);
            var selectedErrors = errorActions.OrderBy(_ => Random.Next()).Take(errorCount).ToList();

            foreach (var (name, action) in selectedErrors)
            {
                // Random delay to simulate real-world timing (0-2 seconds)
                int delayMs = Random.Next(0, 2000);
                await Task.Delay(delayMs);
                Log.Information("Simulating error: {ErrorName} with delay {DelayMs}ms", name, delayMs);
                if (action.Method.ReturnType == typeof(Task))
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    await ((Task)action.DynamicInvoke()).ConfigureAwait(false);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
                else
                {
                    action();
                }
            }
        }
        private static void SimulateConfigurationError(IConfiguration configuration)
        {
            try
            {
                string configValue = configuration["MissingConfigKey"] ?? throw new InvalidOperationException("Configuration key 'MissingConfigKey' not found.");
                Log.Information("Config value: {ConfigValue}", configValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Configuration error occurred.");
            }
        }

        private static void SimulateNullReferenceError()
        {
            try
            {
                string[]? array = null;
                var length = array!.Length;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Null reference error in processing array.");
            }
        }

        private static void SimulateConnectionStringError(IConfiguration configuration)
        {
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                string connectionString = configuration["Database:ConnectionString"];
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Invalid or missing database connection string.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database connection string error.");
            }
        }

        private static void SimulateDivideByZeroError()
        {
            try
            {
                int zero = 0;
                int result = 10 / zero;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Arithmetic error in calculation.");
            }
        }

        private static void SimulateFileNotFoundError()
        {
            try
            {
                string content = File.ReadAllText("nonexistent.txt");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read file due to FileNotFoundException.");
            }
        }

        private static void SimulateUnauthorizedAccessError()
        {
            try
            {
                File.WriteAllText("/root/unauthorized.txt", "Test");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unauthorized access to file system.");
            }
        }

        private static void SimulateTimeoutError()
        {
            try
            {
                throw new TimeoutException("Operation timed out while waiting for external service.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Timeout error during external service call.");
            }
        }

        private static void SimulateArgumentError()
        {
            try
            {
                string invalidInput = "";
                if (string.IsNullOrEmpty(invalidInput))
                {
                    throw new ArgumentException("Input parameter cannot be empty.", nameof(invalidInput));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Invalid argument provided.");
            }
        }

        private static void SimulateFormatError()
        {
            try
            {
                int number = int.Parse("not_a_number");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to parse input string to number.");
            }
        }

        private static void SimulateKeyNotFoundError()
        {
            try
            {
                var dict = new Dictionary<string, string>();
                string value = dict["missing_key"];
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Key not found in dictionary.");
            }
        }

        private static void SimulateInvalidCastError()
        {
            try
            {
                object obj = "123";
                int number = (int)obj;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Invalid cast operation.");
            }
        }

        private static void SimulateOverflowError()
        {
            try
            {
                checked
                {
                    int max = int.MaxValue;
                    int overflow = max + 1;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Arithmetic overflow occurred.");
            }
        }

        private static async Task SimulateHttpRequestError()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("http://nonexistent.api.local");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to call external API.");
            }
        }

        private static void SimulateCustomAppError()
        {
            try
            {
                throw new ApplicationException("Custom business logic error: User quota exceeded.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Custom application error occurred.");
            }
        }

        // Simulate random errors with random delay

    }
}
