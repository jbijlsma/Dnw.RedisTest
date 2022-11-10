using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace Dnw.Redis.Api.IntegrationTests;

public class NginxContainerTests
{
    [Fact]
    public async Task Get()
    {
        // Given
        const ushort httpPort = 80;
        
        var nginxContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithName(Guid.NewGuid().ToString("D"))
            .WithImage("nginx")
            .WithPortBinding(httpPort, true)
            .Build();

        await nginxContainer.StartAsync()
            .ConfigureAwait(false);

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new UriBuilder("http", nginxContainer.Hostname, nginxContainer.GetMappedPublicPort(httpPort)).Uri;

        var httpResponseMessage = await httpClient.GetAsync(string.Empty)
            .ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
    }
}