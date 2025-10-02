using Serilog;

namespace FakeDatadogApi
{
    public static class RequestSimulator
    {
        private static readonly Random Random = new Random();
        private static readonly string[] Methods = { "GET", "POST", "PUT", "DELETE" };
        private static readonly string[] Paths =
        {
        "/api/users",
        "/api/users/{id}",
        "/api/orders",
        "/api/orders/{id}",
        "/api/products",
        "/api/products/{id}/details",
        "/api/auth/login",
        "/api/auth/register",
        "/api/cart",
        "/api/cart/checkout"
    };

        public static void LogRandomRequest()
        {
            string method = Methods[Random.Next(Methods.Length)];
            string path = Paths[Random.Next(Paths.Length)].Replace("{id}", Random.Next(1, 1000).ToString());
            Log.Information("Processing request {Method} {Path} at {Timestamp}",
                method, path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        }
    }
}
