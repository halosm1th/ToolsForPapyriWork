using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            {
                return index;
            }
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
                    parsedTokens.Add(ConvertJournalToEntry(ParseJournal(token.TokenText)));
                    break;

                case ChecklistTokenType.Volume:
                    parsedTokens.Add(ConvertVolumeToEntry(ParseVolume(token.TokenText)));
                    break;
            }
        }

        return parsedTokens;
    }

    private CheckListEntry ConvertJournalToEntry(CheckListJournal journal)
    {
        var titleVariations = GenerateTitleVariations(journal.Title);
        return new CheckListEntry(journal.Title, journal.FullText, journal.RawParts, titleVariations, journal.Editor);
    }

    private CheckListEntry ConvertVolumeToEntry(CheckListVolume volume)
    {
        var titleVariations = GenerateTitleVariations(volume.Title);
        return new CheckListEntry(volume.Title, volume.FullText, volume.RawParts, titleVariations, volume.Author, volume.Date);
    }

    private List<string> GenerateTitleVariations(string title)
    {
        var variations = new List<string> { title };

        if (title.Contains(","))
        {
            variations.AddRange(title.Split(',').Select(part => part.Trim()));
        }

        if (title.Contains("-"))
        {
            variations.AddRange(title.Split('-').Select(part => part.Trim()));
        }

        return variations.Distinct().ToList();
    }

    private CheckListVolume ParseVolume(string tokenText)
    {
        string? author = null, publicationYear = null, publicationLocation = null;
        var titleVariations = new List<string>();
        string[] otherData = Array.Empty<string>();

        var parts = tokenText.Split(new[] { ", ed.", ".", ";" }, StringSplitOptions.RemoveEmptyEntries);
        string title = parts[0].Trim();
        titleVariations.Add(title);

        if (parts.Length > 1)
        {
            for (int i = 1; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                if (Regex.IsMatch(part, "(18|19|20)\\d{2}"))
                {
                    publicationYear = Regex.Match(part, "(18|19|20)\\d{2}").Value;
                }
                else if (Regex.IsMatch(part, "\b(ed\\.|ed by|edited by|author)\b", RegexOptions.IgnoreCase))
                {
                    author = Regex.Replace(part, "\b(ed\\.|ed by|edited by|author)\b", "", RegexOptions.IgnoreCase).Trim();
                }
                else if (part.Contains("Online") || part.Contains("archive.org"))
                {
                    otherData = otherData.Append(part).ToArray();
                }
                else
                {
                    titleVariations.Add(part);
                }
            }
        }

        return new CheckListVolume(title, publicationYear, author, null, parts, tokenText, titleVariations);
    }

    private CheckListJournal ParseJournal(string tokenText)
    {
        string[] splits = tokenText.Contains("_")
            ? tokenText.Split('_')
            : tokenText.Contains(", ed.") ? tokenText.Split(", ed.") : new[] { tokenText };

        var journalTitle = splits[0].Trim();
        string? editor = null;

        if (splits.Length > 1)
        {
            editor = splits[1].Trim();
        }

        return new CheckListJournal(journalTitle, editor, null, splits, tokenText);
    }

    private CheckListHeader ParseHeader(string tokenText)
    {
        var header = Regex.Match(tokenText, ">(.*?)<").Groups[1].Value;
        return new CheckListHeader(header, tokenText);
    }

    public void TokenizeCheckList()
    {
        TokenList = ReturnTokenizeCheckList();
    }

    public List<CheckListToken> ReturnTokenizeCheckList()
    {
        var tokens = new List<CheckListToken>();
        bool insideInfoSection = false;

        int startOfPapyriInfoIndex = GetPapyriInfoIndex();

        for (int i = startOfPapyriInfoIndex; i < CheckListLines.Length; i++)
        {
            var lineType = DetermineLineType(CheckListLines[i]);

            if (insideInfoSection && lineType.TokenType == ChecklistTokenType.Appendix)
            {
                insideInfoSection = false;
            }

            if (insideInfoSection)
            {
                tokens.Add(lineType);
            }

            if (!insideInfoSection && lineType.TokenType == ChecklistTokenType.Header)
            {
                insideInfoSection = true;
                tokens.Add(lineType);
            }
        }

        return tokens;
    }

    private CheckListToken DetermineLineType(string line)
    {
        if (line.StartsWith("####")) return new CheckListToken(ChecklistTokenType.Other, line);
        if (line.StartsWith("#####")) return new CheckListToken(ChecklistTokenType.Other, line);
        if (line.StartsWith("## <a id=\"Appendix\">Appendix:")) return new CheckListToken(ChecklistTokenType.Appendix, line);
        if (line.StartsWith("###")) return new CheckListToken(ChecklistTokenType.Header, line.Remove(0, 4));
        if (line.StartsWith(" =")) return new CheckListToken(ChecklistTokenType.Journal, line.Remove(0, 4));
        if (line.StartsWith(" *")) return new CheckListToken(ChecklistTokenType.Volume, line.Remove(0, 3));

        return new CheckListToken(ChecklistTokenType.Other, line);
    }

    public List<ParsedCheckListBlock> StructureParsedData(List<ParsedCheckListItem> parsedData)
    {
        var parsedBlocks = new List<ParsedCheckListBlock>();
        var currentBlock = new ParsedCheckListBlock();

        foreach (var entry in parsedData)
        {
            if (entry is CheckListHeader header)
            {
                if (currentBlock.Header != null)
                {
                    parsedBlocks.Add(currentBlock);
                    currentBlock = new ParsedCheckListBlock();
                }

                currentBlock.SetHeader(header);
            }
            else if (entry is CheckListEntry checkListEntry)
            {
                currentBlock.AddItem(checkListEntry);
            }
        }

        if (currentBlock.Header != null || currentBlock.Entries.Count > 0)
        {
            parsedBlocks.Add(currentBlock);
        }

        return parsedBlocks;
    }
}
