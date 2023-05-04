﻿using System.Collections.Generic;
using XOutput.UI;

namespace XOutput;

/// <summary>
///     Contains the language management.
/// </summary>
public sealed class LanguageModel : ModelBase
{
    private Dictionary<string, string> data;
    public static LanguageModel Instance { get; } = new();

    public Dictionary<string, string> Data
    {
        get => data;
        set
        {
            if (data != value)
            {
                data = value;
                OnPropertyChanged(nameof(Data));
            }
        }
    }

    public string Translate(string key)
    {
        return Translate(data, key);
    }

    public static string Translate(Dictionary<string, string> translation, string key)
    {
        if (translation == null || key == null || !translation.ContainsKey(key)) return key;
        return translation[key];
    }
}