using Projects;

var builder = DistributedApplication.CreateBuilder(args);

//var sql = builder.AddSqlServer("sql")
//    .WithLifetime(ContainerLifetime.Persistent);

//var db = sql.AddDatabase("db");

var api = builder
    .AddProject<WebApi>("api")
    //.WithReference(db)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
