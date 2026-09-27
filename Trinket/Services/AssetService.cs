using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Trinket.Models;

namespace Trinket.Services;

/// <summary>
/// Handles storage for user-imported charm images and their metadata,
/// entirely under %LOCALAPPDATA%\Trinket\. Purely local disk I/O - nothing
/// here ever makes a network call, matching the project's privacy
/// requirements.
/// </summary>
public static class AssetService
{
    private static readonly string AppDataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Trinket");

    private static readonly string ImportedCharmsDirectory = Path.Combine(AppDataDirectory, "ImportedCharms");
    private static readonly string MetadataFilePath = Path.Combine(AppDataDirectory, "imported-charms.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static readonly string[] SupportedExtensions = { ".png", ".webp" };

    public static List<ImportedCharmMetadata> LoadMetadata()
    {
        try
        {
            if (!File.Exists(MetadataFilePath))
            {
                return new List<ImportedCharmMetadata>();
            }

            string json = File.ReadAllText(MetadataFilePath);
            return JsonSerializer.Deserialize<List<ImportedCharmMetadata>>(json, JsonOptions)
                   ?? new List<ImportedCharmMetadata>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load imported charm metadata: {ex}");
            return new List<ImportedCharmMetadata>();
        }
    }

    public static void SaveMetadata(List<ImportedCharmMetadata> metadata)
    {
        try
        {
            Directory.CreateDirectory(AppDataDirectory);
            string json = JsonSerializer.Serialize(metadata, JsonOptions);
            File.WriteAllText(MetadataFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save imported charm metadata: {ex}");
        }
    }

    /// <summary>
    /// Copies a user-selected file into the app's local storage under a
    /// generated unique name (avoiding collisions/overwrites), and returns
    /// the new file's full path.
    /// </summary>
    public static string CopyIntoAppData(string sourceFilePath)
    {
        Directory.CreateDirectory(ImportedCharmsDirectory);

        string extension = Path.GetExtension(sourceFilePath);
        string newFileName = $"{Guid.NewGuid():N}{extension}";
        string destination = Path.Combine(ImportedCharmsDirectory, newFileName);

        File.Copy(sourceFilePath, destination, overwrite: false);
        return destination;
    }

    public static string GetFullPath(string fileName) => Path.Combine(ImportedCharmsDirectory, fileName);

    /// <summary>Deletes a previously-copied file, used to clean up if the user cancels the import flow partway through.</summary>
    public static void DeleteFile(string fullPath)
    {
        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to delete cancelled import file: {ex}");
        }
    }
}