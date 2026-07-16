using System.IO;
using System.Text.Json;
using ScrumPrep.Models;

namespace ScrumPrep.Services;

/// <summary>Speichert die Sitzungs-Historie als JSON unter %APPDATA%\ScrumPrep.</summary>
public class StatsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private List<SessionRecord> _history;

    public StatsService()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScrumPrep");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "history.json");
        _history = Load();
    }

    public IReadOnlyList<SessionRecord> History => _history;

    public void Add(SessionRecord record)
    {
        _history.Add(record);
        Save();
    }

    public void Clear()
    {
        _history = new List<SessionRecord>();
        Save();
    }

    private List<SessionRecord> Load()
    {
        if (!File.Exists(_filePath))
            return new List<SessionRecord>();
        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<SessionRecord>>(json) ?? new List<SessionRecord>();
        }
        catch (JsonException)
        {
            // Kaputte Datei soll den Start nicht verhindern; Historie beginnt dann neu.
            return new List<SessionRecord>();
        }
    }

    private void Save()
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_history, JsonOptions));
    }
}
