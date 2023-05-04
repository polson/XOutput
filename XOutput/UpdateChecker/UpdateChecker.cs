﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using XOutput.Logging;

namespace XOutput.UpdateChecker;

public sealed class UpdateChecker : IDisposable
{
    /// <summary>
    ///     GitHub URL to check the latest release version.
    /// </summary>
    private const string GithubURL = "https://raw.githubusercontent.com/csutorasa/XOutput/master/latest.version";

    private static readonly ILogger logger = LoggerFactory.GetLogger(typeof(UpdateChecker));
    private readonly HttpClient client = new();

    public UpdateChecker()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        client.DefaultRequestHeaders.Add("User-Agent", "System.Net.Http.HttpClient");
    }

    /// <summary>
    ///     Releases all resources.
    /// </summary>
    public void Dispose()
    {
        client.Dispose();
    }

    /// <summary>
    ///     Gets the string of the latest release from a http response.
    /// </summary>
    /// <param name="response">GitHub response</param>
    /// <returns></returns>
    private string GetLatestRelease(string response)
    {
        return response.Trim();
    }

    /// <summary>
    ///     Compares the current version with the latest release.
    /// </summary>
    /// <returns></returns>
    public async Task<VersionCompare> CompareRelease()
    {
        VersionCompare compare;
        HttpResponseMessage response = null;
        try
        {
            await logger.Debug("Getting " + GithubURL);
            response = await client.GetAsync(new Uri(GithubURL));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var latestRelease = GetLatestRelease(content);
            compare = Version.Compare(Version.AppVersion, latestRelease);
        }
        catch (Exception)
        {
            compare = VersionCompare.Error;
        }
        finally
        {
            response?.Dispose();
        }

        return await Task.Run(() => compare);
    }
}