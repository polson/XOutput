﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;


namespace XOutput.Tools;

public class DependencyEmbedder
{
    private readonly List<KeyValuePair<string, string>> packages = new();

    /// <summary>
    ///     Gets the singleton instance of the class.
    /// </summary>
    public static DependencyEmbedder Instance { get; } = new();

    public void AddPackage(string package)
    {
        AddPackage(package, package);
    }

    public void AddPackage(string package, string dllFile)
    {
        packages.Add(new KeyValuePair<string, string>(package, dllFile));
    }

    public void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        foreach (var package in packages)
            if (args.Name.StartsWith(package.Key))
            {
                Log.Information("Loading " + package.Value + ".dll from embedded resources");
                return LoadAssemblyFromResource(Assembly.GetExecutingAssembly().GetName().Name + "." + package.Value +
                                                ".dll");
            }

        return null;
    }

    private Assembly LoadAssemblyFromResource(string resourceName)
    {
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            var assemblyData = new byte[stream.Length];
            stream.Read(assemblyData, 0, assemblyData.Length);
            return Assembly.Load(assemblyData);
        }
    }
}