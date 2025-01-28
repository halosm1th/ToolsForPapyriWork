using System.Runtime.CompilerServices;

namespace PapyriChecklistItems;

class BibliographyEntry
{
    /// <summary>
    /// A variable to check if an entry is empty or not
    /// </summary>
    public bool Empty { get; } = false;
    public string Collection { get; set; }

    public bool HasBibliographyEntry { get; } = false;
    
    public string ArchiveLink { get; }

    public string Name { get; }
    public string Author { get; }
    public string Title { get; }
    public string FullText { get; }
    public string PublicationDate { get; }

    public string BibliographyNumber { get; }

    public BibliographyEntry(string author, string title, 
        string publicationDate,string bibliographyNumber, string fullText, string collection, string archiveLink)
    {
        //A tiny bit of error checking. Basically, if it has nothing, its an empty entry.
        //If it has no number, then the entry wasn't found in the DB. 
        if (title == string.Empty && bibliographyNumber == "") Empty = true;
        if (title != string.Empty && bibliographyNumber != "") HasBibliographyEntry = true;

        Name = title;
        Collection = collection;
        Title = title;
        PublicationDate = publicationDate;
        Author = author;
        BibliographyNumber = bibliographyNumber;
        FullText = fullText;
        ArchiveLink = archiveLink;
    }

    public override string ToString()
    {
        if (Empty) return "Empty Record";
        if (!HasBibliographyEntry) return $"No bibliography number found for: {Name}";
        return $"{BibliographyNumber} {Name} by {Author} published: {PublicationDate}";
    }
}