namespace ScrumPrep.Models;

public enum QuizMode
{
    Exam,
    Learn,
    Topic,
    Mistakes
}

/// <summary>Ergebnis einer einzelnen beantworteten Frage, wird für die Statistik gespeichert.</summary>
public class AnswerRecord
{
    public int QuestionId { get; set; }
    public string Category { get; set; } = "";
    public bool WasCorrect { get; set; }
}

/// <summary>Eine abgeschlossene Quiz-Sitzung, wie sie in der Historie landet.</summary>
public class SessionRecord
{
    public DateTime Date { get; set; }
    public QuizMode Mode { get; set; }
    public string? Category { get; set; }
    public List<AnswerRecord> Answers { get; set; } = new();

    public int Total => Answers.Count;
    public int CorrectCount => Answers.Count(a => a.WasCorrect);
    public double Percent => Total == 0 ? 0 : 100.0 * CorrectCount / Total;
}
