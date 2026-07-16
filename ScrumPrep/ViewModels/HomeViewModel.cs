using ScrumPrep.Models;
using ScrumPrep.Services;

namespace ScrumPrep.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private string? _selectedCategory;

    public HomeViewModel(MainViewModel main, QuestionRepository repository)
    {
        Categories = repository.Categories;
        _selectedCategory = Categories.FirstOrDefault();
        QuestionCount = repository.All.Count;

        StartExamCommand = new RelayCommand(() => main.StartQuiz(QuizMode.Exam));
        StartLearnCommand = new RelayCommand(() => main.StartQuiz(QuizMode.Learn));
        StartTopicCommand = new RelayCommand(
            () => main.StartQuiz(QuizMode.Topic, SelectedCategory),
            () => SelectedCategory != null);
        ShowStatsCommand = new RelayCommand(main.ShowStats);
    }

    public int QuestionCount { get; }
    public IReadOnlyList<string> Categories { get; }

    public string? SelectedCategory
    {
        get => _selectedCategory;
        set => SetField(ref _selectedCategory, value);
    }

    public RelayCommand StartExamCommand { get; }
    public RelayCommand StartLearnCommand { get; }
    public RelayCommand StartTopicCommand { get; }
    public RelayCommand ShowStatsCommand { get; }
}
