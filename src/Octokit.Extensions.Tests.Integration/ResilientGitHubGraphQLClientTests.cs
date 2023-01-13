using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Octokit.Extensions.Tests.Integration
{
    public class ResilientGitHubGraphQLClientTests
    {
        [Fact]
        public async Task MakesWrappedOctokitRequest()
        {
            var connection = new ResilientGitHubGraphQLConnectionFactory()
                .Create(new GraphQL.ProductHeaderValue("Octokit.Extensions.Tests"), Helper.Token);

            var query = GetQuery();

            var json = await connection.Run(query);
            var results = JObject.Parse(json);

            Assert.Equal("octokit", results["data"]?["repository"]?["owner"]?["login"]?.Value<string>());
            Assert.Equal("octokit.net", results["data"]?["repository"]?["name"]?.Value<string>());
        }

        private static string GetQuery()
        {
            var rawQuery = @"query { 
  rateLimit {
    limit
    cost
    remaining
    resetAt
  }
  repository(owner: ""octokit"", name:""octokit.net"") { 
    owner {
      login
    },
    name
  }
}";
            var cleanedUpQuery = rawQuery
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\"", "\\\"");
            var query = $"{{\"query\": \"{cleanedUpQuery}\"}}";
            return query;
        }

        [Fact(Skip = "Caching is not yet supported by GitHub GraphQL - see https://github.com/renovatebot/renovate/issues/11419#issuecomment-1030164500")]
        public async Task MakesCachedWrappedOctokitRequest()
        {
            var connection = new ResilientGitHubGraphQLConnectionFactory()
                .Create(new GraphQL.ProductHeaderValue("Octokit.Extensions.Tests"),
                    Helper.Token,
                    new InMemoryCacheProvider(),
                    new ResilientPolicies().DefaultResilientPolicies);

            var query = GetQuery();

            var json = await connection.Run(query);
            var results = JObject.Parse(json);

            var remaining = results["data"]?["rateLimit"]?["remaining"]?.Value<string>();

            var cachedJson = await connection.Run(query);
            var cachedResults = JObject.Parse(cachedJson);
            var cachedRemaining = cachedResults["data"]?["rateLimit"]?["remaining"]?.Value<string>();

            Assert.Equal(remaining, cachedRemaining);
        }
    }
}
