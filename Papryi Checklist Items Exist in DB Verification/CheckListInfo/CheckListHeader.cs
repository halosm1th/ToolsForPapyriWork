using PapyriChecklistItems;

public class CheckListHeader : ParsedCheckListItem
{
    public string HeaderText { get; }
    public string RawText { get; }

    public CheckListHeader(string headerText, string rawText) : base(headerText, rawText)
    {
        HeaderText = headerText;
        RawText = rawText;
    }
}