using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using HtmlAgilityPack;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using OfficeOpenXml;
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

        var parsedResults = ParseResults(parsed);
        SaveResultsToCSV(parsedResults);
    }

    private static void SaveResultsToCSV(List<BibliographyEntry> resultsToSave)
    {
        int index = 2;
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("checklistEntries");
            worksheet.Cells[1, 1].Value = "ID Number";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Author";
            worksheet.Cells[1, 4].Value = "Date";
            worksheet.Cells[1, 5].Value = "Full Text";
            foreach (var result in resultsToSave)
            {
                worksheet.Cells[index, 1].Value = result.BibliographyNumber;
                worksheet.Cells[index, 2].Value = result.Name;
                worksheet.Cells[index, 3].Value = result.Author;
                worksheet.Cells[index, 4].Value = result.PublicationDate;
                worksheet.Cells[index, 5].Value = result.FullText;


                index++;
            }
            
            worksheet.Cells.AutoFitColumns(0);  //Autofit columns for all cells
                                                //// Change the sheet view to show it in page layout mode
            worksheet.View.PageLayoutView = true;
            var xlFile = FileUtil.GetCleanFileInfo("1.01-GettingStarted.xlsx");
                
            // Save our new workbook in the output directory and we are done!
            package.SaveAs(xlFile);
        }
    }

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

            if (finalResult != null && !entries.Contains(finalResult))
            {
                entries.Add(finalResult);
            }
        }

        return entries;
    }
    
     public class FileUtil
    {
        static DirectoryInfo _outputDir = null;
        public static DirectoryInfo OutputDir
        {
            get
            {
                return _outputDir;
            }
            set
            {
                _outputDir = value;
                if (!_outputDir.Exists)
                {
                    _outputDir.Create();
                }
            }
        } 
        public static FileInfo GetCleanFileInfo(string file)
        {
            var fi = new FileInfo(OutputDir.FullName + Path.DirectorySeparatorChar + file);
            if (fi.Exists)
            { 
                fi.Delete();  // ensures we create a new workbook
            } 
            return fi; 
        }
        public static FileInfo GetFileInfo(string file)
        {
            return new FileInfo(OutputDir.FullName + Path.DirectorySeparatorChar + file);
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
            var di = new DirectoryInfo(_outputDir.FullName + Path.DirectorySeparatorChar + directory);
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