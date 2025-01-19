using PapyriChecklistItems;

public class CheckListJournal : ParsedCheckListItem
{
    public string Title { get; }
    public string? Editor { get; }
    public string? Date { get; }
    public string[] RawParts { get; }
    public string FullText { get; }

    public CheckListJournal(string title, string? editor, string? date, string[] rawParts, string fullText)
    : base(title, fullText)
    {
        Title = title;
        Editor = editor;
        Date = date;
        RawParts = rawParts;
        FullText = fullText;
    }
}