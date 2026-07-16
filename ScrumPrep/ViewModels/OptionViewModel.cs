namespace ScrumPrep.ViewModels;

public enum OptionState
{
    Neutral,
    Correct,        // richtige Option (nach dem Aufdecken)
    WrongSelected,  // gewählt, aber falsch
    Missed          // richtig, aber nicht gewählt
}

public class OptionViewModel : ViewModelBase
{
    private readonly Action<OptionViewModel> _onSelected;
    private bool _isSelected;
    private bool _isEnabled = true;
    private OptionState _state = OptionState.Neutral;

    public OptionViewModel(int index, string text, bool initiallySelected, Action<OptionViewModel> onSelected)
    {
        Index = index;
        Text = text;
        _isSelected = initiallySelected; // bewusst am Setter vorbei, damit der Callback beim Laden nicht feuert
        _onSelected = onSelected;
    }

    public int Index { get; }
    public string Text { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetField(ref _isSelected, value))
                _onSelected(this);
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }

    public OptionState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }
}
