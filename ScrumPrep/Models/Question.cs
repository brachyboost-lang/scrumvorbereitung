namespace ScrumPrep.Models;

public class Question
{
    public int Id { get; set; }
    public string Category { get; set; } = "";
    public string Text { get; set; } = "";
    public List<string> Options { get; set; } = new();
    public List<int> Correct { get; set; } = new();
    public string Explanation { get; set; } = "";

    public bool IsMultiSelect => Correct.Count > 1;
}
