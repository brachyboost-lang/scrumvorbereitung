using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using ScrumPrep.Models;
using ScrumPrep.Services;

namespace ScrumPrep.ViewModels;

/// <summary>
/// Führt eine Quiz-Sitzung durch. Drei Modi:
/// Exam = 80 Zufallsfragen mit 60-Minuten-Timer, Auflösung erst am Ende.
/// Learn/Topic = sofortige Auflösung mit Erklärung nach jeder Frage.
/// </summary>
public class QuizViewModel : ViewModelBase
{
    private const int ExamQuestionCount = 80;
    private static readonly TimeSpan ExamDuration = TimeSpan.FromMinutes(60);

    private readonly MainViewModel _main;
    private readonly List<Question> _questions;
    private readonly Dictionary<int, HashSet<int>> _selections = new(); // Frage-Index -> gewählte Options-Indizes
    private readonly DispatcherTimer? _timer;

    private int _index;
    private DateTime _examEnd;
    private string _timeRemainingText = "";
    private bool _isRevealed;

    public QuizViewModel(MainViewModel main, QuestionRepository repository, StatsService stats,
        QuizMode mode, string? category)
    {
        _main = main;
        Mode = mode;
        Category = category;

        _questions = mode switch
        {
            QuizMode.Exam => repository.RandomExamSet(ExamQuestionCount),
            QuizMode.Topic => QuestionRepository.Shuffled(repository.ByCategory(category!)),
            QuizMode.Mistakes => QuestionRepository.Shuffled(
                repository.All.Where(q => stats.CurrentMistakeIds().Contains(q.Id))),
            _ => QuestionRepository.Shuffled(repository.All)
        };

        if (mode == QuizMode.Exam)
        {
            _examEnd = DateTime.Now + ExamDuration;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) => UpdateTimer();
            _timer.Start();
            UpdateTimer();
        }

        CheckCommand = new RelayCommand(RevealAnswer, () => !_isRevealed && Options.Any(o => o.IsSelected));
        NextCommand = new RelayCommand(GoNext, CanGoNext);
        PreviousCommand = new RelayCommand(GoPrevious, () => IsExam && _index > 0);
        SubmitCommand = new RelayCommand(SubmitExam);
        FinishCommand = new RelayCommand(FinishLearnSession);
        QuitCommand = new RelayCommand(Quit);

        LoadQuestion();
    }

    public QuizMode Mode { get; }
    public string? Category { get; }
    public bool IsExam => Mode == QuizMode.Exam;
    public bool IsLearnLike => !IsExam;

    public string ModeTitle => Mode switch
    {
        QuizMode.Exam => "Exam Simulation",
        QuizMode.Topic => $"Topic Training – {Category}",
        QuizMode.Mistakes => "Mistake Training",
        _ => "Learn Mode"
    };

    public Question CurrentQuestion => _questions[_index];
    public string QuestionText => CurrentQuestion.Text;
    public string CategoryText => CurrentQuestion.Category;
    public string ProgressText => $"Question {_index + 1} of {_questions.Count}";
    public string SelectHint => CurrentQuestion.IsMultiSelect
        ? $"Select {CurrentQuestion.Correct.Count} answers."
        : "Select one answer.";

    public ObservableCollection<OptionViewModel> Options { get; } = new();

    public string TimeRemainingText
    {
        get => _timeRemainingText;
        private set => SetField(ref _timeRemainingText, value);
    }

    /// <summary>Learn-Modus: true, sobald die aktuelle Frage aufgelöst wurde.</summary>
    public bool IsRevealed
    {
        get => _isRevealed;
        private set
        {
            if (SetField(ref _isRevealed, value))
            {
                OnPropertyChanged(nameof(ShowExplanation));
                OnPropertyChanged(nameof(FeedbackText));
                OnPropertyChanged(nameof(WasAnsweredCorrectly));
            }
        }
    }

    public bool ShowExplanation => IsRevealed;
    public string Explanation => CurrentQuestion.Explanation;

    public bool WasAnsweredCorrectly => IsRevealed && IsCurrentAnswerCorrect();
    public string FeedbackText => !IsRevealed ? "" : IsCurrentAnswerCorrect() ? "Correct!" : "Wrong.";

    public string AnsweredCountText => IsExam
        ? $"Answered: {_selections.Count(s => s.Value.Count > 0)} / {_questions.Count}"
        : $"Answered: {_selections.Count} – Correct: {_selections.Keys.Count(i => IsAnswerCorrect(i))}";

    public RelayCommand CheckCommand { get; }
    public RelayCommand NextCommand { get; }
    public RelayCommand PreviousCommand { get; }
    public RelayCommand SubmitCommand { get; }
    public RelayCommand FinishCommand { get; }
    public RelayCommand QuitCommand { get; }

    public void StopTimer() => _timer?.Stop();

    private void LoadQuestion()
    {
        Options.Clear();
        var saved = _selections.TryGetValue(_index, out var sel) ? sel : null;
        for (int i = 0; i < CurrentQuestion.Options.Count; i++)
        {
            Options.Add(new OptionViewModel(
                i, CurrentQuestion.Options[i], saved?.Contains(i) ?? false, OnOptionSelected));
        }
        // Im Learn-Modus ist eine bereits beantwortete Frage nicht erneut erreichbar,
        // daher startet jede geladene Frage unaufgelöst.
        IsRevealed = false;

        OnPropertyChanged(nameof(CurrentQuestion));
        OnPropertyChanged(nameof(QuestionText));
        OnPropertyChanged(nameof(CategoryText));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(SelectHint));
        OnPropertyChanged(nameof(Explanation));
        OnPropertyChanged(nameof(AnsweredCountText));
    }

    private void OnOptionSelected(OptionViewModel changed)
    {
        if (changed.IsSelected && !CurrentQuestion.IsMultiSelect)
        {
            foreach (var option in Options.Where(o => o != changed))
                option.IsSelected = false;
        }
        SaveSelection();
    }

    private void SaveSelection()
    {
        _selections[_index] = Options.Where(o => o.IsSelected).Select(o => o.Index).ToHashSet();
        OnPropertyChanged(nameof(AnsweredCountText));
    }

    private bool IsAnswerCorrect(int questionIndex)
    {
        var selected = _selections.TryGetValue(questionIndex, out var sel) ? sel : new HashSet<int>();
        return selected.SetEquals(_questions[questionIndex].Correct);
    }

    private bool IsCurrentAnswerCorrect() => IsAnswerCorrect(_index);

    private void RevealAnswer()
    {
        SaveSelection();
        foreach (var option in Options)
        {
            option.IsEnabled = false;
            bool isCorrect = CurrentQuestion.Correct.Contains(option.Index);
            option.State = isCorrect
                ? (option.IsSelected ? OptionState.Correct : OptionState.Missed)
                : (option.IsSelected ? OptionState.WrongSelected : OptionState.Neutral);
        }
        IsRevealed = true;
        OnPropertyChanged(nameof(AnsweredCountText));
    }

    private bool CanGoNext()
    {
        if (_index >= _questions.Count - 1)
            return false;
        return IsExam || IsRevealed;
    }

    private void GoNext()
    {
        _index++;
        LoadQuestion();
    }

    private void GoPrevious()
    {
        _index--;
        LoadQuestion();
    }

    private void UpdateTimer()
    {
        var remaining = _examEnd - DateTime.Now;
        if (remaining <= TimeSpan.Zero)
        {
            TimeRemainingText = "00:00";
            StopTimer();
            MessageBox.Show("Time is up! The exam will be scored now.", "Exam Simulation",
                MessageBoxButton.OK, MessageBoxImage.Information);
            FinishSession(allQuestions: true);
            return;
        }
        TimeRemainingText = $"{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";
    }

    private void SubmitExam()
    {
        int unanswered = _questions.Count - _selections.Count(s => s.Value.Count > 0);
        string message = unanswered > 0
            ? $"You have {unanswered} unanswered question(s). Unanswered questions count as wrong.\n\nSubmit the exam?"
            : "Submit the exam?";
        if (MessageBox.Show(message, "Submit Exam", MessageBoxButton.YesNo, MessageBoxImage.Question)
            != MessageBoxResult.Yes)
            return;

        StopTimer();
        FinishSession(allQuestions: true);
    }

    private void FinishLearnSession()
    {
        if (!_selections.Any() || (!IsRevealed && _selections.Count == 1 && _selections.ContainsKey(_index)))
        {
            _main.ShowHome();
            return;
        }
        FinishSession(allQuestions: false);
    }

    private void Quit()
    {
        if (MessageBox.Show("Quit this session? Nothing will be saved.", "Quit",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        StopTimer();
        _main.ShowHome();
    }

    /// <summary>
    /// Wertet die Sitzung aus. Im Exam zählen alle Fragen (unbeantwortet = falsch),
    /// im Learn-Modus nur die bereits aufgelösten Fragen.
    /// </summary>
    private void FinishSession(bool allQuestions)
    {
        var record = new SessionRecord { Date = DateTime.Now, Mode = Mode, Category = Category };
        var included = new List<Question>();
        var givenAnswers = new Dictionary<int, List<int>>();

        for (int i = 0; i < _questions.Count; i++)
        {
            bool wasAnswered = _selections.ContainsKey(i);
            if (!allQuestions && !(wasAnswered && (i != _index || IsRevealed)))
                continue;

            var question = _questions[i];
            included.Add(question);
            givenAnswers[question.Id] = _selections.TryGetValue(i, out var sel)
                ? sel.OrderBy(x => x).ToList()
                : new List<int>();
            record.Answers.Add(new AnswerRecord
            {
                QuestionId = question.Id,
                Category = question.Category,
                WasCorrect = IsAnswerCorrect(i)
            });
        }

        _main.ShowResult(record, included, givenAnswers);
    }
}
