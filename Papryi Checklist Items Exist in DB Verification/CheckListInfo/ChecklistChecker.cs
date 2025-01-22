using System.Text.RegularExpressions;

namespace PapyriChecklistItems;

class ChecklistChecker
{
    private string filePath;
    private string[] CheckListLines;
    private List<CheckListToken> TokenList;

    public ChecklistChecker(string checklistLocation = "/checklist.md")
    {
        filePath = Directory.GetCurrentDirectory() + checklistLocation;
        CheckListLines = File.ReadAllLines(filePath);
    }

    private int GetPapyriInfoIndex()
    {
        for (int index = 0; index < CheckListLines.Length; index++)
        {
            if (CheckListLines[index].StartsWith("###"))
                return index;
        }

        throw new IndexOutOfRangeException("No header starting the Papyri Info section could be found at: " + filePath);
    }

    public List<ParsedCheckListItem> ParseTokenizedCheckList()
    {
        var parsedTokens = new List<ParsedCheckListItem>();

        foreach (var token in TokenList)
        {
            switch (token.TokenType)
            {
                case ChecklistTokenType.Header:
                    parsedTokens.Add(ParseHeader(token.TokenText));
                    break;
                case ChecklistTokenType.Journal:
                    parsedTokens.Add(ParseJournal(token.TokenText));
                    break;
                case ChecklistTokenType.Volume:
                    parsedTokens.Add(ParseVolume(token.TokenText));
                    break;
            }
        }

        return parsedTokens;
    }

    private CheckListVolume ParseVolume(string tokenText)
    {
        var volumeTitle = ExtractTitle(tokenText);
        var author = ExtractAuthor(tokenText);
        var date = ExtractDate(tokenText);

        return new CheckListVolume(volumeTitle, date, author, null, new[] { tokenText }, tokenText);
    }

    private CheckListJournal ParseJournal(string tokenText)
    {
        var journalTitle = ExtractTitle(tokenText);
        var editor = ExtractAuthor(tokenText);
        var date = ExtractDate(tokenText);

        return new CheckListJournal(journalTitle, editor, date, null, new[] { tokenText }, tokenText);
    }

    private CheckListHeader ParseHeader(string tokenText)
    {
        var header = tokenText.Contains("<") && tokenText.Contains(">")
            ? tokenText.Split('>')[1].Split('<')[0]
            : tokenText;
        return new CheckListHeader(header, tokenText);
    }

    public void TokenizeCheckList()
    {
        TokenList = ReturnTokenizeCheckList();
    }

    public List<CheckListToken> ReturnTokenizeCheckList()
    {
        var tokens = new List<CheckListToken>();

        bool headerFound = false;
        int startOfPapyriInfoIndex = GetPapyriInfoIndex();

        for (; startOfPapyriInfoIndex < CheckListLines.Length; startOfPapyriInfoIndex++)
        {
            var lineType = DetermineLineType(startOfPapyriInfoIndex);
            if (headerFound && lineType.TokenType == ChecklistTokenType.Appendix)
            {
                headerFound = false;
            }

            if (headerFound)
                tokens.Add(lineType);

            if (!headerFound && lineType.TokenType == ChecklistTokenType.Header)
            {
                headerFound = true;
                tokens.Add(lineType);
            }
        }

        return tokens;
    }

    private CheckListToken DetermineLineType(int index)
    {
        var line = CheckListLines[index].Trim();

        if (line.StartsWith("####") || line.StartsWith("#####"))
            return new CheckListToken(ChecklistTokenType.Other, line);
        if (line.StartsWith("## <a id=\"Appendix\">Appendix:"))
            return new CheckListToken(ChecklistTokenType.Appendix, line);
        if (line.StartsWith("###"))
            return new CheckListToken(ChecklistTokenType.Header, line[4..]);
        if (line.StartsWith("="))
            return new CheckListToken(ChecklistTokenType.Journal, line[4..]);
        if (line.StartsWith("*"))
            return new CheckListToken(ChecklistTokenType.Volume, line[3..]);

        return new CheckListToken(ChecklistTokenType.Other, line);
    }

    private string ExtractTitle(string text)
    {
        var match = Regex.Match(text, "_(.*?)_");
        return match.Success ? match.Groups[1].Value : text.Split(',')[0].Trim();
    }

    private string? ExtractAuthor(string text)
    {
        var match = Regex.Match(text, "ed\\.\\s([^,\\.]+)");
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private string? ExtractDate(string text)
    {
        var match = Regex.Match(text, "\b(\\d{4})\b");
        return match.Success ? match.Value : null;
    }

    public List<ParsedCheckListBlock> StructureParsedData(List<ParsedCheckListItem> parsedData)
    {
        var parsedBlocks = new List<ParsedCheckListBlock>();
        var currentBlock = new ParsedCheckListBlock();
        var first = true;
        
        foreach (var entry in parsedData)
        {
            if (entry is CheckListHeader header)
            {
                if (currentBlock.Entries.Any() || currentBlock.ChecklistSectionName != null)
                {
                    if (first) first = false;
                        else
                    {
                        parsedBlocks.Add(currentBlock);
                    }
                    currentBlock = new ParsedCheckListBlock();
                }
                currentBlock.SetHeader(header);
            }
            else if (entry is CheckListEntry item)
            {
                currentBlock.AddItem(item);
            }
            else
            {
                currentBlock.AddItem(entry);
            }
        }

        if (currentBlock.Entries.Any() || currentBlock.ChecklistSectionName != null)
        {
            parsedBlocks.Add(currentBlock);
        }

        return parsedBlocks;
    }
}
