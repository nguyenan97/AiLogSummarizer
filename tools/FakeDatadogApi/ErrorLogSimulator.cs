using Serilog;

namespace FakeDatadogApi
{


    public static class ErrorLogSimulator
    {

        private static readonly Random Random = new Random();
        public static string SimulateRandomService()
        {
            var services = new string[] { "product-api", "order-api", "user-api", "inventory-api", "payment-api", "shipping-api" };
            // Randomly select 1-3 errors to simulate
            int errorCount = Random.Next(1, services.Length);
            return services[errorCount-1];    
        }
        public static string SimulateRandomEnv()
        {
            var envs = new string[] { "qc", "uat", "staging", "production" };
            // Randomly select 1-3 errors to simulate
            int errorCount = Random.Next(1, envs.Length);
            return envs[errorCount - 1];
        }
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
            var env = SimulateRandomEnv();
            var service=SimulateRandomService();
            try
            {
                string configValue = configuration["MissingConfigKey"] ?? throw new InvalidOperationException("Configuration key 'MissingConfigKey' not found.");
               
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags",$"env:{env},service:{service}").Error(ex, "Configuration error occurred.");
            }
        }

        private static void SimulateNullReferenceError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                string[]? array = null;
                var length = array!.Length;
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Null reference error in processing array.");

            }
        }

        private static void SimulateConnectionStringError(IConfiguration configuration)
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
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
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Database connection string error.");
            }
        }

        private static void SimulateDivideByZeroError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                int zero = 0;
                int result = 10 / zero;
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Arithmetic error in calculation.");
            }
        }

        private static void SimulateFileNotFoundError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                string content = File.ReadAllText("nonexistent.txt");
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Failed to read file due to FileNotFoundException.");
            }
        }

        private static void SimulateUnauthorizedAccessError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                File.WriteAllText("/root/unauthorized.txt", "Test");
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Failed to read file due to FileNotFoundException.");
            }
        }

        private static void SimulateTimeoutError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                throw new TimeoutException("Operation timed out while waiting for external service.");
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Timeout error during external service call.");
            }
        }

        private static void SimulateArgumentError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
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
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Invalid argument provided.");
            }
        }

        private static void SimulateFormatError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                int number = int.Parse("not_a_number");
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Failed to parse input string to number.");
            }
        }

        private static void SimulateKeyNotFoundError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                var dict = new Dictionary<string, string>();
                string value = dict["missing_key"];
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Key not found in dictionary.");
            }
        }

        private static void SimulateInvalidCastError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                object obj = "123";
                int number = (int)obj;
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Invalid cast operation.");
            }
        }

        private static void SimulateOverflowError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
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
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Arithmetic overflow occurred.");
            }
        }

        private static async Task SimulateHttpRequestError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("http://nonexistent.api.local");
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Failed to call external API.");
            }
        }

        private static void SimulateCustomAppError()
        {
            var env = SimulateRandomEnv();
            var service = SimulateRandomService();
            try
            {
                throw new ApplicationException("Custom business logic error: User quota exceeded.");
            }
            catch (Exception ex)
            {
                Log.ForContext("ddtags", $"env:{env},service:{service}").Error(ex, "Custom application error occurred.");
            }
        }

        // Simulate random errors with random delay

    }
}
