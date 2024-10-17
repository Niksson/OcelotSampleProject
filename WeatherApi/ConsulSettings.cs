namespace WeatherApi;

public class ConsulSettings
{
    public string Address { get; set; }
    public string ServiceName { get; set; }
    public string ServiceID { get; set; }
    public int ServicePort { get; set; }
    public int ServiceGrpcPort { get; set; }
    public string Token { get; set; }
}