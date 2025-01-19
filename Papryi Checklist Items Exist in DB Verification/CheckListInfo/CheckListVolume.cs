using PapyriChecklistItems;

public class CheckListVolume : ParsedCheckListItem
{
    public string Title { get; }
    public string? Date { get; }
    public string? Author { get; }
    public string? Nos { get; }
    public string[] RawParts { get; }
    public string FullText { get; }
    public List<string> TitleVariations { get; }

    public CheckListVolume(string title, string? date, string? author, string? nos, string[] rawParts, string fullText, List<string> titleVariations) : base(title,fullText)
    {
        Title = title;
        Date = date;
        Author = author;
        Nos = nos;
        RawParts = rawParts;
        FullText = fullText;
        TitleVariations = titleVariations;
    }
}