namespace PapyriChecklistItems;

public class ParsedCheckListItem
{
    public ParsedCheckListItem(string title, string fullText)
    {
        Title = title;
        FullText = fullText;
    }

    public string Title { get; set; }
    private string FullText { get; set; }
    
}