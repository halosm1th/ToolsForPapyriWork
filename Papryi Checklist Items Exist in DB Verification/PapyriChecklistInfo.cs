using System;
using System.Collections.Generic;
using System.Linq;
using Papryi_Checklist_Items_Exist_in_DB_Verification;
using PapyriChecklistItems;

namespace PapyriChecklistItems
{
    class PapyriChecklistInfo
    {
        public static void Main()
        {
            var checkListParser = new ChecklistChecker();
            checkListParser.TokenizeCheckList();
            var result = checkListParser.ParseTokenizedCheckList();
            var parsed = checkListParser.StructureParsedData(result);

            ParseResults(parsed);
        }

        private static void ParseResults(List<ParsedCheckListBlock> parsed, bool fullText = false)
        {
            foreach (var block in parsed)
            {
                foreach (var entry in block.Entries.OfType<CheckListEntry>())
                {
                    SearchAndDisplayResults(block.ChecklistSectionName, entry, fullText);
                }
            }
        }

        private static void SearchAndDisplayResults(string sectionName, CheckListEntry entry, bool fullText)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nSearching for: {sectionName} {entry.Title}");
            Console.ForegroundColor = ConsoleColor.Gray;

            var searcher = new PapyriSearcher();
            var results = searcher.BibliographySearch(sectionName, entry);

            if (results != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Found result: {results}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No results found.");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nPress any key to process the next entry. Press r or f to rerun and print all info.");
            var key = Console.ReadKey();
            if (key.KeyChar.ToString().ToLower() == "r" || key.KeyChar.ToString().ToLower() == "f")
            {
                ParseResults(
                    new List<ParsedCheckListBlock>
                        {new ParsedCheckListBlock {Entries = new List<ParsedCheckListItem> {entry}}}, true);
            }
        }
    }
}
