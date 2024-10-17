using Consul;
using WeatherApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.WebHost.UseUrls("http://*:5032");

if (builder.Configuration.GetValue<bool>("Consul:UseConsul"))
{
    var consulSettings = new ConsulSettings();
    builder.Configuration.GetSection("Consul").Bind(consulSettings);
    builder.Services.AddSingleton(consulSettings);
    builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
    {
        var address = builder.Configuration.GetValue<string>("Consul:Address");
        consulConfig.Address = new Uri(address);
        consulConfig.Token = builder.Configuration.GetValue<string>("Consul:Token");
    }));
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.UseConsul();

await app.RunAsync();
