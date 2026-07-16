using System.Windows;
using ScrumPrep.Models;
using ScrumPrep.Services;

namespace ScrumPrep.ViewModels;

public class SessionSummary
{
    public SessionSummary(SessionRecord record)
    {
        Date = record.Date.ToString("dd.MM.yyyy HH:mm");
        Mode = record.Mode switch
        {
            QuizMode.Exam => "Exam",
            QuizMode.Topic => $"Topic ({record.Category})",
            QuizMode.Mistakes => "Mistakes",
            _ => "Learn"
        };
        Score = $"{record.CorrectCount}/{record.Total}";
        Percent = $"{record.Percent:F0}%";
        PassInfo = record.Mode == QuizMode.Exam ? (record.Percent >= 85 ? "passed" : "failed") : "";
    }

    public string Date { get; }
    public string Mode { get; }
    public string Score { get; }
    public string Percent { get; }
    public string PassInfo { get; }
}

public class StatsViewModel : ViewModelBase
{
    private readonly StatsService _stats;
    private readonly MainViewModel _main;

    public StatsViewModel(MainViewModel main, StatsService stats)
    {
        _main = main;
        _stats = stats;
        Rebuild();

        HomeCommand = new RelayCommand(main.ShowHome);
        ClearCommand = new RelayCommand(ClearHistory, () => Sessions.Count > 0);
    }

    public List<SessionSummary> Sessions { get; private set; } = new();
    public List<CategoryStat> CategoryStats { get; private set; } = new();
    public string? WeakestCategoryText { get; private set; }
    public bool HasData => Sessions.Count > 0;

    public RelayCommand HomeCommand { get; }
    public RelayCommand ClearCommand { get; }

    private void Rebuild()
    {
        Sessions = _stats.History
            .OrderByDescending(r => r.Date)
            .Select(r => new SessionSummary(r))
            .ToList();

        CategoryStats = _stats.History
            .SelectMany(r => r.Answers)
            .GroupBy(a => a.Category)
            .OrderBy(g => g.Key)
            .Select(g => new CategoryStat(g.Key, g.Count(a => a.WasCorrect), g.Count()))
            .ToList();

        var weakest = CategoryStats.OrderBy(c => c.Percent).FirstOrDefault();
        WeakestCategoryText = weakest == null
            ? null
            : $"Weakest area: {weakest.Name} ({weakest.Percent:F0}%) – focus your training there.";

        OnPropertyChanged(nameof(Sessions));
        OnPropertyChanged(nameof(CategoryStats));
        OnPropertyChanged(nameof(WeakestCategoryText));
        OnPropertyChanged(nameof(HasData));
    }

    private void ClearHistory()
    {
        if (MessageBox.Show("Delete the complete history? This cannot be undone.", "Clear History",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;
        _stats.Clear();
        Rebuild();
    }
}
