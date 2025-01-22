using HtmlAgilityPack;

namespace PapyriChecklistItems;

class TableTokenCollection
{
    
    public string Author { get; set; } = "";
    public string Title { get; set; } = "";
    public string PublicationLocation { get; set; } = "";
    public string PublicationDate { get; set; } = "";

    public TableToken EntryNumberToken { get; private set; } = new TableToken();
    public TableToken PublisherNumberToken { get; private set; } = new TableToken();
    public List<TableToken> IntermediaryTokens { get; } = new List<TableToken>();
    public string IntermediaryText => IntermediaryTokens.Aggregate("", (h, t) => h = h + t.TokenText);

    public string EntryNumber => EntryNumberToken.TokenText;
    public string PublisherNumber => PublisherNumberToken.TokenText;

    /// <summary>
    /// Check if the Collection could add a new token. 
    /// </summary>
    public bool CouldAddEntryNumber => EntryNumberToken == null || EntryNumberToken == new TableToken();
    
    /// <summary>
    /// Try and add an entry number token to the token collection.
    /// </summary>
    /// <param name="number">The number to add</param>
    /// <returns>Will return true if could add successfully, return false is not.</returns>
    public bool AddEntryNumber(string number)
    {
        //If you can not add an entry number, return false because you cant create an entry.
        if (!CouldAddEntryNumber) return false;
        
        //Now make sure the number is a number, if the given value is not a number, return false.
        if (!Int32.TryParse(number, out var numb)) return false;
        
        //If it is a number, create a new token
        //Otherwise, you can create the entry:
        EntryNumberToken = new TableToken(TableTokenTypes.BibliographyNumber, number);
        return true;
    }
    
    
    /// <summary>
    /// Check if the Collection could add a new token. 
    /// </summary>
    public bool CouldAddPublisherEntry => PublisherNumberToken == null || PublisherNumberToken == new TableToken();
    
    /// <summary>
    /// Try and add an Publisher info token to the token collection.
    /// </summary>
    /// <param name="number">The number to add</param>
    /// <returns>Will return true if could add successfully, return false is not.</returns>
    public bool AddPublisherNumber(string publisherInfo)
    {
        //If you can not add an entry number, return false because you cant create an entry.
        if (!CouldAddPublisherEntry) return false;
        
        //TODO figure out why I was making sure it was a number
        //Now make sure the number is a number, if the given value is not a number, return false.
        //if (!Int32.TryParse(publisherInfo, out var numb)) return false;
        
        //If it is a number, create a new token
        //Otherwise, you can create the entry:
        PublisherNumberToken = new TableToken(TableTokenTypes.PublisherInfo, publisherInfo);
        return true;
    }

    public void AddTokenToCollection(string tokenText)
    {
        IntermediaryTokens.Add(new TableToken(TableTokenTypes.AuthorInfo, tokenText));
    }
    
    
    
}