namespace PapyriChecklistItems;

class ParsedCheckListBlock
{
    public string ChecklistSectionName => _header?.Header ?? "None";
    private CheckListHeader? _header = null;

    private int index = 0;

    public bool CanAdd { private set; get; } = true;

    public List<CheckListEntry> Entries { get; } = new List<CheckListEntry>();

    public override string ToString()
    {
        return $"{ChecklistSectionName}: {Entries.Aggregate("", (h,t) => h += " " + t )}";
    }


    public bool HasNextEntry()
    {
        return index >= Entries.Count;
    }

    public void SetHeader(CheckListHeader header)
    {
        _header ??= header;
    }

    public void AddItem(ParsedCheckListItem entry)
    {
        if (_header == null && entry.GetType() == typeof(CheckListHeader))
        {
            _header = (CheckListHeader) entry;
        }else if (entry.GetType() != typeof(CheckListHeader))
        {
            if(CanAdd) AddEntry(entry as CheckListEntry);
        }
        
    }

    private void AddEntry(CheckListEntry entry)
    {
        Entries.Add(entry);
    }

    public void AddFinalItem(CheckListEntry finalEntry)
    {
        CanAdd = false;
        AddEntry(finalEntry);
    }
}