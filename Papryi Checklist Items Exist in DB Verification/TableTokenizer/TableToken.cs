namespace PapyriChecklistItems;

record TableToken
{
    public TableToken()
    {
    }

    public TableTokenTypes TokenType { get; } = TableTokenTypes.None;
    public string TokenText { get;} = "";
    public TableToken(TableTokenTypes tokenType, string tokenText)
    {
        TokenType = tokenType;
        TokenText = tokenText;
    }

    
    
}