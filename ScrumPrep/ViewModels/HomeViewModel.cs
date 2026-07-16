using ScrumPrep.Models;
using ScrumPrep.Services;

namespace ScrumPrep.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private string? _selectedCategory;

    public HomeViewModel(MainViewModel main, QuestionRepository repository, StatsService stats)
    {
        Categories = repository.Categories;
        _selectedCategory = Categories.FirstOrDefault();
        QuestionCount = repository.All.Count;

        var mistakeIds = stats.CurrentMistakeIds();
        MistakeCount = repository.All.Count(q => mistakeIds.Contains(q.Id));

        StartExamCommand = new RelayCommand(() => main.StartQuiz(QuizMode.Exam));
        StartLearnCommand = new RelayCommand(() => main.StartQuiz(QuizMode.Learn));
        StartTopicCommand = new RelayCommand(
            () => main.StartQuiz(QuizMode.Topic, SelectedCategory),
            () => SelectedCategory != null);
        StartMistakesCommand = new RelayCommand(
            () => main.StartQuiz(QuizMode.Mistakes),
            () => MistakeCount > 0);
        ShowStatsCommand = new RelayCommand(main.ShowStats);
    }

    public int QuestionCount { get; }
    public int MistakeCount { get; }

    public string MistakeHint => MistakeCount == 0
        ? "No open mistakes – answer some questions first."
        : $"{MistakeCount} question(s) waiting for a correct answer.";
    public IReadOnlyList<string> Categories { get; }

    public string? SelectedCategory
    {
        get => _selectedCategory;
        set => SetField(ref _selectedCategory, value);
    }

    public RelayCommand StartExamCommand { get; }
    public RelayCommand StartLearnCommand { get; }
    public RelayCommand StartTopicCommand { get; }
    public RelayCommand StartMistakesCommand { get; }
    public RelayCommand ShowStatsCommand { get; }
}
