using System.ComponentModel;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace cwlogs.settings;

public class GlobalSettings : CommandSettings
{
    [CommandOption("-p|--profile <PROFILE>")]
    [Description("Das AWS-Profil, das verwendet werden soll.")]
    public string? Profile { get; [UsedImplicitly] init; }

    [CommandOption("-r|--region <REGION>")]
    [Description("Die AWS-Region.")]
    public string? Region { get; [UsedImplicitly] init; }

    public AmazonCloudWatchLogsClient CreateClient()
    {
        var options = new AmazonCloudWatchLogsConfig();
        if (!string.IsNullOrEmpty(Region))
        {
            options.RegionEndpoint = RegionEndpoint.GetBySystemName(Region);
        }

        AWSCredentials? credentials;

        if (!string.IsNullOrEmpty(Profile))
        {
            var chain = new CredentialProfileStoreChain();
            if (chain.TryGetAWSCredentials(Profile, out var profileCredentials))
            {
                credentials = profileCredentials;
            }
            else
            {
                throw new Exception($"Profil '{Profile}' konnte nicht gefunden werden.");
            }
        }
        else
        {
            // Fallback to default credential resolution which includes environment variables, SSO, etc.
            // AWS SDK for .NET handles AWS_PROFILE environment variable automatically if no profile is explicitly passed to the client.
            // DefaultAWSCredentialsIdentityResolver is the modern way to resolve credentials.
            var resolver = new Amazon.Runtime.Credentials.DefaultAWSCredentialsIdentityResolver();
            credentials = resolver.ResolveIdentity(options);
        }

        return credentials != null
            ? new AmazonCloudWatchLogsClient(credentials, options)
            : new AmazonCloudWatchLogsClient(options);
    }
}