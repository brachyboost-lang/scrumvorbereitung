using System.IO;
using System.Text.Json;
using ScrumPrep.Models;

namespace ScrumPrep.Services;

/// <summary>Lädt den Fragenkatalog aus Data\questions.json neben der Exe.</summary>
public class QuestionRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly List<Question> _questions;

    public QuestionRepository()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "questions.json");
        string json = File.ReadAllText(path);
        _questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions)
                     ?? throw new InvalidOperationException($"Fragenkatalog konnte nicht gelesen werden: {path}");
    }

    public IReadOnlyList<Question> All => _questions;

    public IReadOnlyList<string> Categories =>
        _questions.Select(q => q.Category).Distinct().OrderBy(c => c).ToList();

    public List<Question> ByCategory(string category) =>
        _questions.Where(q => q.Category == category).ToList();

    /// <summary>Zufällige Auswahl für die Prüfungssimulation.</summary>
    public List<Question> RandomExamSet(int count)
    {
        var shuffled = Shuffled(_questions);
        return shuffled.Take(Math.Min(count, shuffled.Count)).ToList();
    }

    public static List<Question> Shuffled(IEnumerable<Question> source)
    {
        var list = source.ToList();
        var rng = Random.Shared;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}
