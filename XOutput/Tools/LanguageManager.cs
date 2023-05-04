using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using XOutput.Logging;

namespace XOutput.Tools;

/// <summary>
///     Contains the language management.
/// </summary>
public sealed class LanguageManager
{
    private static readonly ILogger logger = LoggerFactory.GetLogger(typeof(LanguageManager));
    private readonly Dictionary<string, Dictionary<string, string>> data = new();

    private string language;

    private LanguageManager()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var serializer = new JsonSerializer();
        foreach (var resourceName in assembly.GetManifestResourceNames().Where(s =>
                     s.StartsWith(assembly.GetName().Name + ".Resources.Languages.",
                         StringComparison.CurrentCultureIgnoreCase)))
        {
            var resourceKey = resourceName.Split('.')[3];
            using (var stream = new JsonTextReader(new StreamReader(assembly.GetManifestResourceStream(resourceName))))
            {
                data[resourceKey] = serializer.Deserialize<Dictionary<string, string>>(stream);
            }

            logger.Info(resourceKey + " language is loaded.");
        }

        Language = "English";
    }

    /// <summary>
    ///     Gets the singleton instance of the class.
    /// </summary>
    public static LanguageManager Instance { get; } = new();

    /// <summary>
    ///     Gets or sets the current language.
    /// </summary>
    public string Language
    {
        get => language;
        set
        {
            var v = value;
            if (!data.ContainsKey(v)) v = "English";
            if (language != v)
            {
                language = v;
                logger.Info("Language is set to " + language);
                LanguageModel.Instance.Data = data[language];
            }
        }
    }

    /// <summary>
    ///     Gets the available languages.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetLanguages()
    {
        return data.Keys;
    }
}