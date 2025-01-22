namespace PapyriChecklistItems;

internal record CheckListJournal(string JournalTitle, string? Editor, string Year, string? Location, string[] OtherInfo,
    string FullText) : CheckListEntry(JournalTitle, Editor, Year, FullText, new List<string>{ Editor ?? "", Location ?? "", OtherInfo.Aggregate("", (h,t) => h = h + " " + t)}.ToArray())
{
    public override string ToString()
    {
        return JournalTitle;
    }
};