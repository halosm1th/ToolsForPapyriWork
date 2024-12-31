namespace PapyriChecklistItems;

class BibliographyEntry
{
    /// <summary>
    /// A variable to check if an entry is empty or not
    /// </summary>
    public bool Empty { get; } = false;

    public bool HasBibliographyEntry { get; } = false;

    public string Name { get; }
    public int BibliographyNumber { get; }

    public BibliographyEntry(string name = "", int bibliographyNumber = -1)
    {
        //A tiny bit of error checking. Basically, if it has nothing, its an empty entry.
        //If it has no number, then the entry wasn't found in the DB. 
        if (name == string.Empty && bibliographyNumber == -1) Empty = true;
        if (name != string.Empty && bibliographyNumber != -1) HasBibliographyEntry = true;

        Name = name;
        BibliographyNumber = bibliographyNumber;
        
    }

    public override string ToString()
    {
        if (Empty) return "Empty Record";
        if (!HasBibliographyEntry) return $"No bibliography number found for: {Name}";
        return $"{BibliographyNumber} {Name}";
    }
}