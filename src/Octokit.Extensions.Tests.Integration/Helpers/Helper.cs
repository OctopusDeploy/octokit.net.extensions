using System;

namespace Octokit.Extensions.Tests.Integration;

public static class Helper
{
    private static readonly Lazy<Credentials> _credentialsThunk = new(() =>
    {
        var githubToken = Environment.GetEnvironmentVariable("OCTOKIT_OAUTHTOKEN");

        if (githubToken != null)
            return new Credentials(githubToken);

        var githubUsername = Environment.GetEnvironmentVariable("OCTOKIT_GITHUBUSERNAME");
        var githubPassword = Environment.GetEnvironmentVariable("OCTOKIT_GITHUBPASSWORD");

        if (githubUsername == null || githubPassword == null)
            return Credentials.Anonymous;

        return new Credentials(githubUsername, githubPassword);
    });

    public static Credentials Credentials => _credentialsThunk.Value;
    public static string Token => Environment.GetEnvironmentVariable("OCTOKIT_GITHUBPASSWORD");
}