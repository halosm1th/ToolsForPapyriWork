using System.Text.RegularExpressions;
using PapyriChecklistItems;

namespace Papryi_Checklist_Items_Exist_in_DB_Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class Entry
{
    public string Header { get; set; }
    public string Name { get; set; }
    public string Editors { get; set; }
    public string Year { get; set; }
    public string Numbers { get; set; }
    public List<SubEntry> SubEntries { get; set; } = new List<SubEntry>();
}

class SubEntry
{
    public string Name { get; set; }
    public string Editors { get; set; }
    public string Year { get; set; }
}

class ChecklistParserV2
{
    
    private string filePath;
    private string CheckListText;
    private List<CheckListToken> TokenList;

    public ChecklistParserV2(string checklistLocation = "/checklist.md")
    {
        filePath = Directory.GetCurrentDirectory() + checklistLocation;
        CheckListText = File.ReadAllText(filePath);
    }
    
    public List<ParsedCheckListItem> LoadEntries()
    {
        var items = new List<ParsedCheckListItem>();
      
                    // Start parsing after the table of contents
                    string contentAfterTOC = CheckListText.Substring(CheckListText.IndexOf("## Table of Contents"));

        // Regex to match headers and entries
        string headerPattern = "## <a id=\".+?\">(?<Header>.+?)</a>"; // Matches headers
        string entryPattern = @"= _(?<Name>.+?)_, ed\. (?<Editors>.+?)\. (?<Year>\d{4})(?:\. Nos\. (?<Numbers>.+?))?"; // Matches entries
        string subEntryPattern = @"\* (?<Name>.+?), ed\. (?<Editors>.+?)\. (?<Year>\d{4})"; // Matches subentries

        var entries = new List<Entry>();

        foreach (Match headerMatch in Regex.Matches(contentAfterTOC, headerPattern))
        {
            var header = headerMatch.Groups["Header"].Value;

            // Extract all entries under the current header
            int headerIndex = contentAfterTOC.IndexOf(headerMatch.Value);
            int nextHeaderIndex = contentAfterTOC.IndexOf("## ", headerIndex + 1);

            string headerContent = nextHeaderIndex == -1
                ? contentAfterTOC.Substring(headerIndex)
                : contentAfterTOC.Substring(headerIndex, nextHeaderIndex - headerIndex);

            foreach (Match entryMatch in Regex.Matches(headerContent, entryPattern))
            {
                var entry = new Entry
                {
                    Header = header,
                    Name = entryMatch.Groups["Name"].Value,
                    Editors = entryMatch.Groups["Editors"].Value,
                    Year = entryMatch.Groups["Year"].Value,
                    Numbers = entryMatch.Groups["Numbers"].Value
                };

                // Extract subentries under the current entry
                int entryIndex = headerContent.IndexOf(entryMatch.Value);
                int nextEntryIndex = headerContent.IndexOf("= _", entryIndex + 1);

                string entryContent = nextEntryIndex == -1
                    ? headerContent.Substring(entryIndex)
                    : headerContent.Substring(entryIndex, nextEntryIndex - entryIndex);

                foreach (Match subEntryMatch in Regex.Matches(entryContent, subEntryPattern))
                {
                    var subEntry = new SubEntry
                    {
                        Name = subEntryMatch.Groups["Name"].Value,
                        Editors = subEntryMatch.Groups["Editors"].Value,
                        Year = subEntryMatch.Groups["Year"].Value
                    };
                    entry.SubEntries.Add(subEntry);
                }

                entries.Add(entry);
            }
        }

        // Output the structured data
        foreach (var entry in entries)
        {
            Console.WriteLine($"Header: {entry.Header}");
            Console.WriteLine($"Name: {entry.Name}");
            Console.WriteLine($"Editors: {entry.Editors}");
            Console.WriteLine($"Year: {entry.Year}");
            Console.WriteLine($"Numbers: {entry.Numbers}");

            if (entry.SubEntries.Count > 0)
            {
                Console.WriteLine("  SubEntries:");
                foreach (var subEntry in entry.SubEntries)
                {
                    Console.WriteLine($"    - Name: {subEntry.Name}");
                    Console.WriteLine($"      Editors: {subEntry.Editors}");
                    Console.WriteLine($"      Year: {subEntry.Year}");
                }
            }

            Console.WriteLine(new string('-', 60));
        }

        return items;
    }
}
