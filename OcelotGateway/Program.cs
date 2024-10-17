using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using OcelotGateway;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.UseUrls("http://*:2999");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddOcelot("Routes", builder.Environment, MergeOcelotJson.ToFile, primaryConfigFile: "ocelot.json",
        reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddOcelot()
    .AddConsul<ServiceAddressConsulServiceBuilder>()
    .AddSingletonDefinedAggregator<CustomAggregator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.UseOcelot();

await app.RunAsync();
