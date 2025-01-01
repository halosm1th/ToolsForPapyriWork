namespace PapyriChecklistItems;

record CheckListHeader(string Header, string FullText) : ParsedCheckListItem(Header, FullText)
{
    public override string ToString()
    {
        return Header;
    }
};