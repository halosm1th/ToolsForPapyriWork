using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using HtmlAgilityPack;
using Papryi_Checklist_Items_Exist_in_DB_Verification;

namespace PapyriChecklistItems;
using System.Net;
using Microsoft.VisualBasic;
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
        BibliographyEntry finalResult;
        foreach (var p in parsed)
        {
            if (p.Entries.Count > 1 && p.Entries != null)
            {
                foreach (var entry in p.Entries.Where(x => x is CheckListVolume))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"\nSearching for: {p.ChecklistSectionName} {entry.Title}");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    var searcher = new PapyriSearcher();
                    finalResult = searcher.BibliographySearch(p.ChecklistSectionName,
                        entry, fullText);


                    Console.ForegroundColor = ConsoleColor.Green;
                    var oldColour = Console.BackgroundColor;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine($"\nBest result is: {finalResult}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("\nPress any key to process next entry. Press r or f to rerun and print all info");
                    Console.BackgroundColor = oldColour;
                    var key = Console.ReadKey();
                    if (key.KeyChar.ToString().ToLower() == "r" || key.KeyChar.ToString().ToLower() == "f")
                    {
                        ParseResults(parsed, true);
                    }
                }
            }
            else
            {
                var entry = p.Entries.First();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"\nSearching for: {p.ChecklistSectionName} {entry.Title}");
                Console.ForegroundColor = ConsoleColor.Gray;

                var searcher = new PapyriSearcher();
                finalResult = searcher.BibliographySearch(p.ChecklistSectionName,
                    entry);


                Console.ForegroundColor = ConsoleColor.Green;
                var oldColour = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine($"\nBest result is: {finalResult}");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("\nPress any key to process next entry. Press r or f to rerun and print all info");
                Console.BackgroundColor = oldColour;
                var key = Console.ReadKey();
                if (key.KeyChar.ToString().ToLower() == "r" || key.KeyChar.ToString().ToLower() == "f")
                {
                    ParseResults(parsed, true);
                }
            }
        }
    }
}