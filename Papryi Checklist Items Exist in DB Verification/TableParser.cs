namespace PapyriChecklistItems;

class TableParser
{

    //example entries to parse
    // 1. 48130. Ulrich Wilcken, Actenstücke aus der königlichen Bank zu Theben in den Museen von Berlin, London, Paris., (London 1886). 
    // 2. 95040. U. Wilcken , P. Viereck , and Fr. Krebs eds., Griechische Urkunden 1, (Berlin 1895). 
    // 3. 95047. Wilhelm Schubart and Ernst Kühn, Papyri und Ostraka der Ptolemäerzeit, (Berlin 1922). 
    // First each result has a number at the start. That can be counted on for every entry.
    //The middle section, until the parathetical, is the author info. the parenthetical is publisher info
    
    private string TextToParse = "";
    private int TextCounter = 0;
    private TableTokenTypes CurrentTokenType = TableTokenTypes.None;

    private char PeekChar()
    {
        if (TextCounter + 1 > TextToParse.Length)
        {
            return (char) 0;
        }

        return TextToParse[(TextCounter + 1)];
    }

    private char NextChar()
    {
        var retChar = TextToParse[TextCounter];
        TextCounter++;
        return retChar;
    }

    enum CurrentTokenMode
    {
        NewToken,
        MidToken,
        CloseToken,
        EndOfToken,
        EndOfText
    }

    private TableTokenCollection BasicTokenizer()
    {
        var tokenCollection = new TableTokenCollection();
        var currentMode = CurrentTokenMode.NewToken;
        //While we have characters to look at
        while (currentMode != CurrentTokenMode.EndOfText)
        {
            string textToken = "";

            //Go through the tokens in a line until the end of the token type is reached.
            while (currentMode != CurrentTokenMode.EndOfToken)
            {
                var nextChar = NextChar();
                if (EndOfToken(nextChar))
                {
                    currentMode = CurrentTokenMode.EndOfToken;
                }
                else
                {
                    if (CurrentTokenType == TableTokenTypes.None)
                    {
                        CurrentTokenType = TableTokenTypes.BibliographyNumber;
                    }

                    if (nextChar != '\n')
                    {
                        textToken += nextChar;
                    }
                }
            }

            //Set current token type
            if (CurrentTokenType == TableTokenTypes.BibliographyNumber)
            {
                CurrentTokenType = TableTokenTypes.AuthorInfo;
                currentMode = CurrentTokenMode.MidToken;
                if (tokenCollection.CouldAddEntryNumber)
                {
                    tokenCollection.AddEntryNumber(textToken);
                }
            }
            else if (CurrentTokenType == TableTokenTypes.AuthorInfo)
            {
                CurrentTokenType = TableTokenTypes.PublisherInfo;
                currentMode = CurrentTokenMode.CloseToken;
                tokenCollection.AddTokenToCollection(textToken);
            }
            else if (CurrentTokenType == TableTokenTypes.PublisherInfo)
            {
                CurrentTokenType = TableTokenTypes.None;
                currentMode = CurrentTokenMode.EndOfText;
                if (tokenCollection.CouldAddPublisherEntry) tokenCollection.AddPublisherNumber(textToken);
            }
        }
        
        return tokenCollection;
    }

    private bool EndOfToken(char next)
    {
        //This is the end case for at least one token
        if (CurrentTokenType == TableTokenTypes.BibliographyNumber && next == '.') return true;
        if (CurrentTokenType == TableTokenTypes.PublisherInfo &&  next == ')') return true;
        if (CurrentTokenType == TableTokenTypes.AuthorInfo && next == '(') return true;
        return false;
    }
    
    public BibliographyEntry Parse(string tableEntry)
    {
        TextToParse = tableEntry;

        var tokenCollection = BasicTokenizer();
        if (Int32.TryParse(tokenCollection.EntryNumber, out var numb))
            return new BibliographyEntry(tokenCollection.IntermediaryText, numb);

        return new BibliographyEntry(tokenCollection.IntermediaryText);
    }
}