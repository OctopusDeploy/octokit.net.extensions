using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Octokit.Internal;
using Polly;
using InMemoryCredentialStore = Octokit.GraphQL.Internal.InMemoryCredentialStore;

namespace Octokit.Extensions
{
    public class ResilientGitHubGraphQLConnectionFactory
    {
        private readonly ILogger logger;

        public ResilientGitHubGraphQLConnectionFactory(ILogger logger = null)
        {
            this.logger = logger;
        }
        
        public Octokit.GraphQL.Connection Create(
            GraphQL.ProductHeaderValue productHeaderValue,
            string token,
            ICacheProvider cacheProvider = null,
            params IAsyncPolicy[] policies)
        {
            if (policies is null || policies.Length == 0)
                policies = new ResilientPolicies(logger).DefaultResilientPolicies;

            var policy = policies.Length > 1 
                ? Policy.WrapAsync(policies) 
                : policies[0];
            
            var connection = new Octokit.GraphQL.Connection(
                productHeaderValue,
                GraphQL.Connection.GithubApiUri,
                new InMemoryCredentialStore(token),
                new HttpClient(new MyHandler { InnerHandler = GetHttpHandlerChain(policy, cacheProvider) })
            );

            return connection;
        }
        
        private HttpMessageHandler GetHttpHandlerChain(IAsyncPolicy policy, ICacheProvider cacheProvider)
        {
            var handler = HttpMessageHandlerFactory.CreateDefault();

            handler = new GitHubResilientHandler(handler, policy, this.logger);

            if (cacheProvider != null)
            {
                //handler = new HttpCacheHandler(handler,cacheProvider,logger); 
                throw new NotImplementedException("GraphQL caching is not supported by GitHub yet");
            }

            return handler;
        }
    }

    public class MyHandler : DelegatingHandler
    {
    }
}
