namespace PapyriChecklistItems;

class TableParser
{
    private string TextToParse = "";
    private int TextCounter = 0;
    private TableTokenTypes CurrentTokenType = TableTokenTypes.None;

    private char PeekChar()
    {
        return TextCounter < TextToParse.Length ? TextToParse[TextCounter] : (char)0;
    }

    private char NextChar()
    {
        if (TextCounter >= TextToParse.Length)
        {
            return (char)0;
        }
        return TextToParse[TextCounter++];
    }

    private bool EndOfToken(char next)
    {
        return CurrentTokenType switch
        {
            TableTokenTypes.BibliographyNumber => next == '.',
            TableTokenTypes.AuthorInfo => next == '(',
            TableTokenTypes.PublisherInfo => next == ')',
            _ => false
        };
    }

    private bool IsEndOfEntry()
    {
        return TextCounter >= TextToParse.Length;
    }

    private TableTokenCollection BasicTokenizer()
    {
        var tokenCollection = new TableTokenCollection();
        CurrentTokenType = TableTokenTypes.BibliographyNumber; // Start with the bibliography number

        while (!IsEndOfEntry())
        {
            string textToken = "";

            // Read until the end of the current token type
            while (!IsEndOfEntry())
            {
                var nextChar = PeekChar();
                if (EndOfToken(nextChar))
                {
                    NextChar(); // Consume the token-ending character
                    break;
                }
                textToken += NextChar();
            }

            textToken = textToken.Trim();

            if (!string.IsNullOrEmpty(textToken))
            {
                switch (CurrentTokenType)
                {
                    case TableTokenTypes.BibliographyNumber:
                        tokenCollection.AddEntryNumber(textToken);
                        CurrentTokenType = TableTokenTypes.AuthorInfo;
                        break;
                    case TableTokenTypes.AuthorInfo:
                        tokenCollection.AddTokenToCollection(textToken);
                        CurrentTokenType = TableTokenTypes.PublisherInfo;
                        break;
                    case TableTokenTypes.PublisherInfo:
                        tokenCollection.AddPublisherNumber(textToken);
                        CurrentTokenType = TableTokenTypes.None;
                        break;
                }
            }

            // Handle cases where we didn't switch token types correctly
            if (CurrentTokenType != TableTokenTypes.None && IsEndOfEntry())
            {
                tokenCollection.AddTokenToCollection(textToken); // Add whatever was left
            }
        }

        return tokenCollection;
    }

    public BibliographyEntry Parse(string tableEntry)
    {
        TextToParse = tableEntry;
        TextCounter = 0;
        CurrentTokenType = TableTokenTypes.BibliographyNumber;

        var tokenCollection = BasicTokenizer();

        // Debugging output
        Console.WriteLine($"Entry Number: {tokenCollection.EntryNumber}");
        Console.WriteLine($"Intermediary Text: {tokenCollection.IntermediaryText}");
        Console.WriteLine($"Publisher Number: {tokenCollection.PublisherNumber}");

        if (int.TryParse(tokenCollection.EntryNumber, out var numb))
        {
            return new BibliographyEntry(
                $"{tokenCollection.IntermediaryText} {tokenCollection.PublisherNumber}", numb);
        }

        return new BibliographyEntry(tokenCollection.IntermediaryText);
    }
}
