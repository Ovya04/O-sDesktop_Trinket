namespace Trinket.Models;

/// <summary>
/// Persisted record of one user-imported charm: enough to reconstruct a
/// working CharmDefinition on the next app launch. The actual image file
/// lives alongside this metadata in the app's local ImportedCharms folder -
/// never uploaded anywhere.
/// </summary>
public class ImportedCharmMetadata
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public double AttachmentX { get; set; } = 0.5;
    public double AttachmentY { get; set; } = 0.1;
}