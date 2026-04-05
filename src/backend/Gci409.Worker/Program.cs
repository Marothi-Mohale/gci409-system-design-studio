using Gci409.Application;
using Gci409.Infrastructure;
using Gci409.Infrastructure.Persistence;
using Gci409.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<GenerationWorker>();

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Gci409DbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

host.Run();
