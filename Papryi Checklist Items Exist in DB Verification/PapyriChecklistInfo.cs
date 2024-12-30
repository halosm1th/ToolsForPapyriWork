using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using HtmlAgilityPack;

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
        
        
        foreach (var p in parsed)
        {
            if (p.Entries.Count > 1)
            {
                foreach (var entry in p.Entries.Where(x => x.GetType() == typeof(CheckListVolume)))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Searching for: {p.ChecklistSectionName} {entry.Title}");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    var searcher = new PapyriSearcher();
                    searcher.BibliographySearch(p.ChecklistSectionName,
                        entry.Title, new() {});

                    
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Press any key to process next entry");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.ReadKey();

                }
            }
            else
            {
                
            }
        }

    }
}