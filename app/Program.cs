using ArkitechDataApi.Models;
using ArkitechDataApi.Repositories;
using ArkitechDataApi.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// — MongoDB setup (unchanged) —
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings")
);
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MongoSettings>>().Value
);
builder.Services.AddSingleton<MongoRepository>();
builder.Services.AddSingleton<IDataService, DataService>();

// — MQTT setup — 
builder.Services.Configure<MqttSettings>(
    builder.Configuration.GetSection("MqttSettings")
);
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MqttSettings>>().Value
);

// **REGISTER the new HiveMQtt‐based subscriber**
builder.Services.AddHostedService<MqttSubscriberService>();

// — Controllers & Swagger (unchanged) —
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
