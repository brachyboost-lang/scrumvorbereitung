using ScrumPrep.Models;

namespace ScrumPrep.ViewModels;

public class CategoryStat
{
    public CategoryStat(string name, int correct, int total)
    {
        Name = name;
        Correct = correct;
        Total = total;
    }

    public string Name { get; }
    public int Correct { get; }
    public int Total { get; }
    public double Percent => Total == 0 ? 0 : 100.0 * Correct / Total;
    public string DisplayText => $"{Name}: {Correct}/{Total} ({Percent:F0}%)";
}

/// <summary>Eine falsch beantwortete Frage in der Ergebnis-Durchsicht.</summary>
public class MissedQuestion
{
    public MissedQuestion(Question question, List<int> given)
    {
        QuestionText = question.Text;
        Category = question.Category;
        YourAnswer = given.Count == 0
            ? "(not answered)"
            : string.Join("\n", given.Select(i => question.Options[i]));
        CorrectAnswer = string.Join("\n", question.Correct.Select(i => question.Options[i]));
        Explanation = question.Explanation;
    }

    public string QuestionText { get; }
    public string Category { get; }
    public string YourAnswer { get; }
    public string CorrectAnswer { get; }
    public string Explanation { get; }
}

public class ResultViewModel : ViewModelBase
{
    private const double ExamPassPercent = 85.0;

    public ResultViewModel(MainViewModel main, SessionRecord record,
        List<Question> questions, Dictionary<int, List<int>> givenAnswers)
    {
        Record = record;

        CategoryStats = record.Answers
            .GroupBy(a => a.Category)
            .OrderBy(g => g.Key)
            .Select(g => new CategoryStat(g.Key, g.Count(a => a.WasCorrect), g.Count()))
            .ToList();

        var wrongIds = record.Answers.Where(a => !a.WasCorrect).Select(a => a.QuestionId).ToHashSet();
        MissedQuestions = questions
            .Where(q => wrongIds.Contains(q.Id))
            .Select(q => new MissedQuestion(q, givenAnswers.GetValueOrDefault(q.Id) ?? new List<int>()))
            .ToList();

        HomeCommand = new RelayCommand(main.ShowHome);
        StatsCommand = new RelayCommand(main.ShowStats);
    }

    public SessionRecord Record { get; }
    public List<CategoryStat> CategoryStats { get; }
    public List<MissedQuestion> MissedQuestions { get; }

    public string Title => Record.Mode switch
    {
        QuizMode.Exam => "Exam Result",
        QuizMode.Topic => $"Topic Training Result – {Record.Category}",
        QuizMode.Mistakes => "Mistake Training Result",
        _ => "Learn Session Result"
    };

    public string ScoreText => $"{Record.CorrectCount} of {Record.Total} correct ({Record.Percent:F1}%)";

    public bool IsExam => Record.Mode == QuizMode.Exam;
    public bool Passed => Record.Percent >= ExamPassPercent;
    public string PassFailText => Passed ? "PASSED" : "FAILED";
    public string PassHint => $"Passing score: {ExamPassPercent:F0}%";
    public bool HasMissedQuestions => MissedQuestions.Count > 0;

    public RelayCommand HomeCommand { get; }
    public RelayCommand StatsCommand { get; }
}
