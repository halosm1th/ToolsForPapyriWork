using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using HtmlAgilityPack;
using OfficeOpenXml;
using System.Drawing;
using System.Text.RegularExpressions;
using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace PapyriChecklistItems;
using System.Net;
using Microsoft.VisualBasic;
class Entry
{
    public string Name { get; set; }
    public List<SubEntry> SubEntries { get; set; } = new List<SubEntry>();
}

class SubEntry
{
    public string Title { get; set; }
    public string Date { get; set; }
    public string Authors { get; set; }
    public string EntryNumber { get; set; }
    public string ArchiveLink { get; set; }
    public string PublicationLocations { get; set; }
}

class PapyriChecklistInfo
{
    public static void Main()
    {
        var checkListEntries = LoadChecklistEntries();

        //var parsedResults = ParseResults(parsed);
        //Console.ForegroundColor = ConsoleColor.Green;
        //Console.WriteLine("Checklist parsed");
        //SaveResultsToCSV(parsedResults);
    }

    private static List<Entry> LoadChecklistEntries()
    {
        string filePath = Directory.GetCurrentDirectory() + "/Fullchecklist.md";
        var entries = new List<Entry>();
        Entry currentEntry = null;

        // Regex patterns
        var entryHeaderPattern = new Regex("### <a id=\"\"(.*?)\">(.*?)<\\/a>");
        var singleEntryPattern = new Regex(@"= _(.*?)_, (.*?\d{4}.*?\.|\d{4})(.*?)\. ed\. (.*?),? (.*?)\. (\d{4}).*?\[Online: archive\.org\]\((.*?)\)");
        var subEntryPattern = new Regex(@"\* (\w+), (.*?)\. ed\. (.*?)\. (\d{4}).*?\[Online: archive\.org\]\((.*?)\)");

        foreach (var line in File.ReadLines(filePath))
        {
            // Check for new top-level entry
            var entryMatch = entryHeaderPattern.Match(line);
            if (entryMatch.Success)
            {
                currentEntry = new Entry { Name = entryMatch.Groups[2].Value };
                entries.Add(currentEntry);
                continue;
            }

            // Check for single entry within an entry block
            if (currentEntry != null)
            {
                var singleMatch = singleEntryPattern.Match(line);
                if (singleMatch.Success)
                {
                    currentEntry.SubEntries.Add(new SubEntry
                    {
                        Title = singleMatch.Groups[1].Value.Trim(),
                        PublicationLocations = singleMatch.Groups[2].Value.Trim(),
                        Date = singleMatch.Groups[6].Value.Trim(),
                        Authors = singleMatch.Groups[4].Value.Trim(),
                        ArchiveLink = singleMatch.Groups[7].Value.Trim()
                    });
                    continue;
                }

                // Check for subentries (multiple * entries)
                var subEntryMatch = subEntryPattern.Match(line);
                if (subEntryMatch.Success)
                {
                    currentEntry.SubEntries.Add(new SubEntry
                    {
                        Title = subEntryMatch.Groups[2].Value.Trim(),
                        Authors = subEntryMatch.Groups[3].Value.Trim(),
                        Date = subEntryMatch.Groups[4].Value.Trim(),
                        EntryNumber = subEntryMatch.Groups[1].Value.Trim(),
                        ArchiveLink = subEntryMatch.Groups[5].Value.Trim()
                    });
                }
            }
        }

        // Output results
        foreach (var entry in entries)
        {
            Console.WriteLine($"Entry: {entry.Name}");
            foreach (var sub in entry.SubEntries)
            {
                Console.WriteLine($"  Title: {sub.Title}");
                Console.WriteLine($"  Publication Locations: {sub.PublicationLocations}");
                Console.WriteLine($"  Date: {sub.Date}");
                Console.WriteLine($"  Authors: {sub.Authors}");
                Console.WriteLine($"  Entry Number: {sub.EntryNumber}");
                Console.WriteLine($"  Archive Link: {sub.ArchiveLink}");
                Console.WriteLine();
            }
        }

        return entries;
    }

    static string ExtractArchiveLink(string line)
    {
        var archiveLinkPattern = new Regex(@"\[Online: archive\.org\]\((.*?)\)");
        var match = archiveLinkPattern.Match(line);
        return match.Success ? match.Groups[1].Value : "";
    }

    private static void SaveResultsToCSV(List<BibliographyEntry> resultsToSave)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Saving to CSV");
        Console.ForegroundColor = ConsoleColor.Blue;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage())
        {
            int index = 2;
            var worksheet = package.Workbook.Worksheets.Add("checklistEntries");
            worksheet.Cells[1, 1].Value = "ID Number";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Collection";
            worksheet.Cells[1, 4].Value = "Author";
            worksheet.Cells[1, 5].Value = "Date";
            worksheet.Cells[1, 6].Value = "Archive Link";
            worksheet.Cells[1, 7].Value = "Full Text";
            foreach (var result in resultsToSave)
            {
                worksheet.Cells[index, 1].Value = result.BibliographyNumber;
                worksheet.Cells[index, 2].Value = result.Name;
                worksheet.Cells[index, 3].Value = result.Collection;
                worksheet.Cells[index, 4].Value = result.Author;
                worksheet.Cells[index, 5].Value = result.PublicationDate;
                worksheet.Cells[index, 7].Value = result.ArchiveLink;
                worksheet.Cells[index, 6].Value = result.FullText;


                index++;
            }
            
            worksheet.Cells.AutoFitColumns(0);  //Autofit columns for all cells
                                                //// Change the sheet view to show it in page layout mode
            //worksheet.View.PageLayoutView = true;
            var xlFile = FileUtil.GetCleanFileInfo("checklist.xlsx");
                
            // Save our new workbook in the output directory and we are done!
            package.SaveAs(xlFile);
        }

        Console.WriteLine("Saved to xlsx");
    }
/*
    private static List<BibliographyEntry> ParseResults(List<ParsedCheckListBlock> parsed, bool fullText = false)
    {
        BibliographyEntry finalResult = null;
        var entries = new List<BibliographyEntry>();
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
                    
                    entries.Add(finalResult);

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("\nPress any key to process next entry. Press r or f to rerun and print all info");
                    Console.BackgroundColor = oldColour;
                    var key = new ConsoleKeyInfo(); //Console.ReadKey();
                    if (key.KeyChar.ToString().ToLower() == "r" || key.KeyChar.ToString().ToLower() == "f")
                    {
                        ParseResults(parsed, true);
                    }
                }
            }
            else
            {
                if (p.Entries.Any())
                {
                    var entry = p.Entries.First();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"\nSearching for: {p.ChecklistSectionName} {entry.Title}");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    var searcher = new PapyriSearcher();
                    finalResult = searcher.BibliographySearch(p.ChecklistSectionName,
                        entry);

                    entries.Add(finalResult);

                    Console.ForegroundColor = ConsoleColor.Green;
                    var oldColour = Console.BackgroundColor;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine($"\nBest result is: {finalResult}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    // Console.WriteLine("\nPress any key to process next entry. Press r or f to rerun and print all info");
                    Console.BackgroundColor = oldColour;
                    // var key = Console.ReadKey();
                    // if (key.KeyChar.ToString().ToLower() == "r" || key.KeyChar.ToString().ToLower() == "f")
                    // {
                    //ParseResults(parsed, true);
                    // }
                }
            }
        }

        return entries;
    }
    */
    
     public class FileUtil
    {
        public static FileInfo GetCleanFileInfo(string file)
        {
            var fi = new FileInfo(Directory.GetCurrentDirectory() + "/" + file);
            if (fi.Exists)
            { 
                fi.Delete();  // ensures we create a new workbook
            } 
            return fi; 
        }
        public static FileInfo GetFileInfo(string file)
        {
            return new FileInfo(Directory.GetCurrentDirectory() + "/" + file);
        }

        public static FileInfo GetFileInfo(DirectoryInfo altOutputDir, string file, bool deleteIfExists = true)
        {
            var fi = new FileInfo(altOutputDir.FullName + Path.DirectorySeparatorChar + file);
            if (deleteIfExists && fi.Exists)
            {
                fi.Delete();  // ensures we create a new workbook
            }
            return fi;  
        }


        internal static DirectoryInfo GetDirectoryInfo(string directory)
        {
            var di = new DirectoryInfo(Directory.GetCurrentDirectory() + "/" + directory);
            if (!di.Exists)
            {
                di.Create();
            }
            return di;
        }
        /// <summary>
        /// Returns a fileinfo with the full path of the requested file
        /// </summary>
        /// <param name="directory">A subdirectory</param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FileInfo GetFileInfo(string directory, string file)
        {
            var rootDir = GetRootDirectory().FullName;
            return new FileInfo(Path.Combine(rootDir, directory, file));
        }

        public static DirectoryInfo GetRootDirectory()
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            while (!currentDir.EndsWith("bin"))
            {
                currentDir = Directory.GetParent(currentDir).FullName.TrimEnd('\\');
            }
            return new DirectoryInfo(currentDir).Parent;
        }

        public static DirectoryInfo GetSubDirectory(string directory, string subDirectory)
        {
            var currentDir = GetRootDirectory().FullName;
            return new DirectoryInfo(Path.Combine(currentDir, directory, subDirectory));
        }
    }
}