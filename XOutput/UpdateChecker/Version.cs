using System;
using System.Linq;
using Serilog;


namespace XOutput.UpdateChecker;

/// <summary>
///     Version related informations.
/// </summary>
public static class Version
{
    /// <summary>
    ///     Current application version.
    /// </summary>
    public const string AppVersion = "3.31";

    /// <summary>
    ///     Compares the version with the current version.
    /// </summary>
    /// <param name="version">reference version</param>
    /// <returns></returns>
    public static VersionCompare Compare(string appVersion, string version)
    {
        try
        {
            Log.Debug("Current application version: " + appVersion);
            Log.Debug("Latest application version: " + version);
            var current = appVersion.Split('.').Select(t => int.Parse(t)).ToArray();
            var compare = version.Split('.').Select(t => int.Parse(t)).ToArray();
            for (var i = 0; i < 100; i++)
            {
                var currentNotPresent = i >= current.Length;
                var compareNotPresent = i >= compare.Length;
                if (compareNotPresent)
                {
                    if (currentNotPresent)
                        return VersionCompare.UpToDate;
                    return VersionCompare.NewRelease;
                }

                if (currentNotPresent)
                {
                    return VersionCompare.NeedsUpgrade;
                }

                var currentValue = current[i];
                var compareValue = compare[i];
                if (currentValue > compareValue) return VersionCompare.NewRelease;
                if (currentValue < compareValue) return VersionCompare.NeedsUpgrade;
            }

            return VersionCompare.Error;
        }
        catch (Exception)
        {
            return VersionCompare.Error;
        }
    }
}

/// <summary>
///     Version compare result enum.
/// </summary>
public enum VersionCompare
{
    NewRelease,
    UpToDate,
    NeedsUpgrade,
    Error
}