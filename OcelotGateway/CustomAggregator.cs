using System.Net;
using Ocelot.Middleware;
using Ocelot.Multiplexer;

namespace OcelotGateway;

public class CustomAggregator : IDefinedAggregator
{
    public Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
    {
        return Task.FromResult(new DownstreamResponse(new HttpResponseMessage(HttpStatusCode.OK)));
    }
}