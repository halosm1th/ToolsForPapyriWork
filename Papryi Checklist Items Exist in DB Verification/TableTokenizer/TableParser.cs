using System.Text.RegularExpressions;
using OfficeOpenXml.Style;

namespace PapyriChecklistItems;

class TableParser
{
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
        if (TextCounter + 1 < TextToParse.Length)
        {
            var retChar = TextToParse[TextCounter];
            TextCounter++;
            return retChar;
        }
        else
        {
            return (char) 0;
        }
    }

    enum CurrentTokenMode
    {
        NewToken,
        MidToken,
        CloseToken,
        EndOfToken,
        EndOfText
    }

    private TableTokenCollection BasicTokenizer(string collectionName)
    {
        var tokenCollection = new TableTokenCollection();
        tokenCollection.Collection = collectionName;
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
                    //If we haven't started, then the first bit of info is always the bib number.
                    if (CurrentTokenType == TableTokenTypes.None)
                    {
                        CurrentTokenType = TableTokenTypes.BibliographyNumber;
                    }

                    //Build the list of tokens
                    if (nextChar != '\n' && nextChar != '\0')
                    {
                        textToken += nextChar;
                    }
                }
            }

            //Set current token type
            if (CurrentTokenType == TableTokenTypes.BibliographyNumber)
            {
                //If we have a biblio number, the next piece of info is the authors name and the like
                CurrentTokenType = TableTokenTypes.AuthorInfo;
                currentMode = CurrentTokenMode.MidToken;
                if (tokenCollection.CouldAddEntryNumber)
                {
                    //Add the number value
                    tokenCollection.AddEntryNumber(textToken);
                    tokenCollection.BaseText += " " + textToken.Trim();
                }
            }
            else if (CurrentTokenType == TableTokenTypes.AuthorInfo)
            {
                //if we're dealing with author, after that comes title
                CurrentTokenType = TableTokenTypes.Title;
                currentMode = CurrentTokenMode.MidToken;
                tokenCollection.Author = textToken.Trim();
                tokenCollection.BaseText += " " + textToken.Trim();
            }
            else if (CurrentTokenType == TableTokenTypes.Title)
            {
                //if we're dealing with title, after that comes publisher info
                CurrentTokenType = TableTokenTypes.PublisherInfo;
                currentMode = CurrentTokenMode.CloseToken;
                tokenCollection.Title = textToken.Trim();
                tokenCollection.BaseText += " " + textToken.Trim();

            }
            else if (CurrentTokenType == TableTokenTypes.PublisherInfo)
            {
                CurrentTokenType = TableTokenTypes.None;
                currentMode = CurrentTokenMode.EndOfText;
                tokenCollection.BaseText += " " + textToken.Trim();
                ExtractPublicationInfo(tokenCollection, textToken.Trim());
            }
        }
        

        return tokenCollection;
    }

    private void ExtractPublicationInfo(TableTokenCollection tokenCollection, string publicationInfo)
    {
        // Extract location and date from publication info
        var match = Regex.Match(publicationInfo, "(\\d{4})");
        if (match.Success)
        {
            //tokenCollection.PublicationLocation = match.Groups[1].Value.Trim();
            tokenCollection.PublicationDate = match.Groups[1].Value.Trim();
        }
        else
        {
            tokenCollection.PublicationLocation = publicationInfo;
        }
    }

    private bool EndOfToken(char next)
    {
        //This is the end case for at least one token
        if (CurrentTokenType == TableTokenTypes.BibliographyNumber && next == '.') return true;
        if (CurrentTokenType == TableTokenTypes.AuthorInfo && next == ',') return true;
        if (CurrentTokenType == TableTokenTypes.Title && next == '(') return true;
        if (CurrentTokenType == TableTokenTypes.PublisherInfo && next == ')') return true;
        if (next == '\0') return true;
        return false;
    }

    public BibliographyEntry Parse(string tableEntry, string collection)
    {
        TextToParse = tableEntry;
        TextCounter = 0;
        CurrentTokenType = TableTokenTypes.None;

        var tokenCollection = BasicTokenizer(collection);

        return new BibliographyEntry(
            tokenCollection.Author,
            tokenCollection.Title,
            tokenCollection.PublicationDate,
            tokenCollection.EntryNumber,
            tokenCollection.BaseText,
            tokenCollection.Collection
        );
    }
}
