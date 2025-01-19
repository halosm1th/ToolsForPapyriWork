namespace PapyriChecklistItems;

class ParsedCheckListBlock
{
    public string ChecklistSectionName => Header?.HeaderText ?? "None";
    public CheckListHeader? Header { get; private set; } = null;

    private int index = 0;

    public bool CanAdd { private set; get; } = true;

    public List<ParsedCheckListItem> Entries { get; set; } = new List<ParsedCheckListItem>();

    public override string ToString()
    {
        string headerText = Header != null ? $"Header: {Header.HeaderText}\n" : "No Header\n";
        string entriesText = Entries.Aggregate("", (current, entry) => current + entry.ToString() + "\n");
        return headerText + entriesText;
    }

    public bool HasNextEntry()
    {
        return index < Entries.Count;
    }

    public void SetHeader(CheckListHeader header)
    {
        if (Header == null)
        {
            Header = header;
        }
    }

    public void AddItem(ParsedCheckListItem item)
    {
        if (Header == null && item is CheckListHeader header)
        {
            Header = header;
        }
        else if (item is CheckListEntry entry)
        {
            if (CanAdd) Entries.Add(entry);
        }
    }

    public void AddFinalItem(CheckListEntry finalEntry)
    {
        CanAdd = false;
        Entries.Add(finalEntry);
    }
}