public class CheckListToken
{
    public ChecklistTokenType TokenType { get; }
    public string TokenText { get; }

    public CheckListToken(ChecklistTokenType tokenType, string tokenText)
    {
        TokenType = tokenType;
        TokenText = tokenText;
    }
}