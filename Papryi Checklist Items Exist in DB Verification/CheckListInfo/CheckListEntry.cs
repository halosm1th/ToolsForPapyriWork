namespace PapyriChecklistItems;

internal record CheckListEntry(string Title, string FullText, string[] OtherData) : ParsedCheckListItem(Title, FullText);