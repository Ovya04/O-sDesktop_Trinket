using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trinket.Models;

namespace Trinket.Services;

/// <summary>
/// Reads and writes the list of saved Trinket presets to
/// %LOCALAPPDATA%\Trinket\trinkets.json. Separate from settings.json (which
/// only ever holds the single current live configuration).
/// </summary>
public static class TrinketService
{
    private static readonly string AppDataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Trinket");

    private static readonly string TrinketsFilePath = Path.Combine(AppDataDirectory, "trinkets.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static List<TrinketDefinition> Load()
    {
        try
        {
            if (!File.Exists(TrinketsFilePath))
            {
                return new List<TrinketDefinition>();
            }

            string json = File.ReadAllText(TrinketsFilePath);
            return JsonSerializer.Deserialize<List<TrinketDefinition>>(json, JsonOptions)
                   ?? new List<TrinketDefinition>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load trinkets: {ex}");
            return new List<TrinketDefinition>();
        }
    }

    public static void Save(List<TrinketDefinition> trinkets)
    {
        try
        {
            Directory.CreateDirectory(AppDataDirectory);
            string json = JsonSerializer.Serialize(trinkets, JsonOptions);
            File.WriteAllText(TrinketsFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save trinkets: {ex}");
        }
    }
}