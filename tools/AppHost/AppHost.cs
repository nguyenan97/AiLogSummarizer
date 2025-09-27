using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder
    .AddProject<WebApi>("api")
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
