using Consul;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

namespace WeatherApi;

public static class ConsulRegistrationMiddleware
{
    public static async Task<IApplicationBuilder> UseConsul(this IApplicationBuilder app)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("AppExtensions");
            var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            var consulConfig = app.ApplicationServices.GetRequiredService<ConsulSettings>();
            IPAddress localIp = null;
            if (GetLocalIPv4(NetworkInterfaceType.Ethernet) == null)
            {
                localIp = GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            }
            else
            {
                localIp = GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }
            var uri = new Uri($"http://{localIp}:{consulConfig.ServicePort}");
            var registrationID = $"{consulConfig.ServiceID}-{uri.Port}";
            var registration = new AgentServiceRegistration()
            {
                ID = registrationID,
                Name = consulConfig.ServiceName,
                Address = $"{uri.Host}",
                Port = uri.Port,
                Tags = new[] { consulConfig.ServiceName },
                Meta = new Dictionary<string, string>
                {
                    {"ServiceGrpcPort", consulConfig.ServiceGrpcPort.ToString() }
                },
                Checks = new[]
                {
                    new AgentCheckRegistration()
                    {
                        HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}/health",
                        Notes = "Checks /health on "+ consulConfig.ServiceName +" Service",
                        Timeout = TimeSpan.FromSeconds(3),
                        Interval = TimeSpan.FromSeconds(10),
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(3600)
                    }
                }

            };

            logger.LogInformation("Registering with Consul");
            await consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            await consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("Unregistering from Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            });

            return app;
    }
    private static IPAddress GetLocalIPv4(NetworkInterfaceType networkInterfaceType)
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(i =>
            i.NetworkInterfaceType == networkInterfaceType && i.OperationalStatus == OperationalStatus.Up);

        foreach (var networkInterface in networkInterfaces)
        {
            var adapterProperties = networkInterface.GetIPProperties();

            if (adapterProperties.GatewayAddresses.FirstOrDefault() == null)
                continue;
            return networkInterface.GetIPProperties().UnicastAddresses
                .Select(x => x.Address).FirstOrDefault(y => y.AddressFamily == AddressFamily.InterNetwork);
        }

        return null;
    }
}