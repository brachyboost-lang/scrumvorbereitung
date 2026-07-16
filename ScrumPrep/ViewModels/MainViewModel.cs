using ScrumPrep.Models;
using ScrumPrep.Services;

namespace ScrumPrep.ViewModels;

/// <summary>Hält die aktuelle Ansicht und stellt die Navigation zwischen den Views bereit.</summary>
public class MainViewModel : ViewModelBase
{
    private readonly QuestionRepository _repository = new();
    private readonly StatsService _stats = new();

    private ViewModelBase _currentViewModel = null!;

    public MainViewModel()
    {
        ShowHome();
    }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (_currentViewModel is QuizViewModel quiz)
                quiz.StopTimer();
            SetField(ref _currentViewModel, value);
        }
    }

    public void ShowHome() => CurrentViewModel = new HomeViewModel(this, _repository);

    public void StartQuiz(QuizMode mode, string? category = null) =>
        CurrentViewModel = new QuizViewModel(this, _repository, mode, category);

    public void ShowResult(SessionRecord record, List<Question> questions, Dictionary<int, List<int>> givenAnswers)
    {
        _stats.Add(record);
        CurrentViewModel = new ResultViewModel(this, record, questions, givenAnswers);
    }

    public void ShowStats() => CurrentViewModel = new StatsViewModel(this, _stats);
}
