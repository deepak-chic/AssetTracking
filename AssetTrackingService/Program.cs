using AssetTrackingService.DependencyResolution;
using IoTPlatformLibrary.ServiceBus;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServiceCollection(builder.Configuration);
builder.Services.AddCosmoService(builder.Configuration);

var app = builder.Build();

app.Services.GetService(typeof(IServiceBusConsumer));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AssetTrackingService");
        c.RoutePrefix = string.Empty;
    });
}

app.MapControllers();

app.Run();

