namespace PapyriChecklistItems;

internal record CheckListEntry(string Title, string Author, string Year, string FullText, string[] OtherData) : ParsedCheckListItem(Title, FullText);