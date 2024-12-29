namespace PapyriChecklistItems;

record CheckListToken
{
    public CheckListToken(ChecklistTokenType tokenType, string tokenText)
    {
        TokenType = tokenType;
        TokenText = tokenText;
    }

    public ChecklistTokenType TokenType { get; }
    public string TokenText { get; }
}