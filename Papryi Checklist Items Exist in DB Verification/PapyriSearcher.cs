using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PapyriChecklistItems;

namespace Papryi_Checklist_Items_Exist_in_DB_Verification;

/// <summary>
/// This class will be used to search Papyri.Info for a given entry, and return its entry from the table
/// </summary>
class PapyriSearcher
{
    private const int AG_MIN = 0;
    private const string SearchUrl = "https://papyri.info/bibliosearch?q=";
    private bool _fullPrint;

    public BibliographyEntry BibliographySearch(string blockName, CheckListEntry searchItem, bool printText = false)
    {
        _fullPrint = printText;
        var page = GetSearchPage(blockName, searchItem.Title);

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"Starting Basic Search... ");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        if (HasHits(page))
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Hit found, getting and testing result.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            var results = GetResultsFromTable(page);
            (int count, BibliographyEntry? entry) res = new (AG_MIN, null);
            foreach (var result in results)
            {
                var cor = CorrectResult(result, searchItem.Title, searchItem.OtherData[0]);
                var ag = cor.Aggregate(0, (h, t) => t ? h + 1 : h);
                if (ag > res.count)
                {
                    res.count = ag;
                    res.entry = result;
                }
            }

            if (res.count > AG_MIN && res.entry != null)
            {
                return res.entry;
            }

            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Failed on base search.\nAttempting Advanced Search...\n");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        else
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("No hits on base search.\nAttempting Advanced Search...\n");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        return AdvancedSearch(blockName, searchItem.Title, searchItem.OtherData) ?? new BibliographyEntry();
    }

    private List<bool> CorrectResultInternal(BibliographyEntry result, string titleOfPapyri, string author)
    {
        var yearTry = Regex.Match(input: titleOfPapyri, pattern: "(18|19|20)[0-9]{2}");
        var results = new List<bool>();


        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"\tResult will be checked against:\n\t\tTitle: {titleOfPapyri}.\n\t\tYear: {yearTry.Value}." +
                $"\n\t\tAuthor: {author}.");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /* This does the same thing as better explained version below. It exists simply to show off what conditionals
         can do and the fun one can have with chaining them. This is not good code.
        return result.Name.Contains(titleOfPapyri) ? true :
            yearTry.Success && result.Name.Contains(yearTry.Value) ? true :
            result.Name.Contains(author); 
        */

        var titleTest = result.Name.Contains(titleOfPapyri);
        results.Add(titleTest);
        if (titleTest)
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"\tResult matched with title {titleOfPapyri} successfully.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        else
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\t\tFailed to match with {titleOfPapyri}. Trying year next");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        //Check if the title has something like looks like a year
        results.Add(yearTry.Success);
        if (yearTry.Success && result.Name.Contains(yearTry.Value))
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"\tResult matched with year {yearTry.Value} successfully.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        else
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\t\tFailed to match with year: {yearTry.Value}. Trying author next");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        
        var authorTry = result.Name.Contains(author);
        results.Add(authorTry);
        if (authorTry)
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"\tResult matched with author {author} successfully.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        else
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\t\tFailed to match with author {author}. Trying Comma Stripped title next.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        var commaStrippedTitle = titleOfPapyri.Split(',');
        var commaStrippedTest = result.Name.Contains(commaStrippedTitle[^1]);
        results.Add(commaStrippedTest);
        if (commaStrippedTest)
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(
                    $"\tResult matched with title with comma stripped {commaStrippedTitle[1]} successfully.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        else
        {

            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(
                    $"\t\tFailed to match with comma stripped {commaStrippedTitle[^1]}. Trying period stripped next");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        var periodStrippedTitle = titleOfPapyri.Split('.');
        var periodStrippedTest = result.Name.Contains(periodStrippedTitle[0]);
        results.Add(periodStrippedTest);
        if (periodStrippedTest)
        {

            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(
                    $"\tResult matched with title with comma stripped {periodStrippedTitle[0]} successfully.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        else
        {

            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(
                    $"\t\tFailed to match with comma stripped {periodStrippedTitle[0]}. Trying comma and period stripped next");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        var commaAndPeriodStrippedTitle = periodStrippedTitle[0].Split(",");
        var nametemp = commaAndPeriodStrippedTitle[^1];
        var resultName = result.Name;
        var commaAndPeriodStrippedTest = resultName.Contains(nametemp);
        results.Add(commaAndPeriodStrippedTest);
        if (commaAndPeriodStrippedTest)
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(
                    $"\tResult matched with title with comma stripped {commaAndPeriodStrippedTitle[1]} successfully.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        else
        {

            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(
                    $"\t\tFailed to match with comma and period stripped {commaAndPeriodStrippedTitle[^1]}.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        
        if (_fullPrint)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine($"NO CORRECT MATCHES FOUND IN TITLE, YEAR, OR AUTHOR of {result.Name}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

        return results;
    }

    //Advanced search is basically me massaging various errors I've found over time, trying to figure out how to best
    //make this all work, as such, its essentially the last stop check point offering various strategies, if none work,
    //then the user has to try and find the result themselves.
    private BibliographyEntry? AdvancedSearch(string title, string papyriNameAndNumber, string[] otherData)
    {
        var results = GetTitlelessSearchResult(papyriNameAndNumber);
        if (results != null && CorrectResults(results, papyriNameAndNumber, otherData[0], out var result)) return result;

        results = PeriodStrippedSearch(title, papyriNameAndNumber);
        if (results != null && CorrectResults(results, papyriNameAndNumber, otherData[0], out result)) return result;

        results = TitleSearchWithAuthor(title, otherData[0]);
        if (results != null && CorrectResults(results, papyriNameAndNumber, otherData[0], out result)) return result;

        results = CommaPeriodStrippedAuthorAddedSearch(otherData[0], title, papyriNameAndNumber);
        if (results != null && CorrectResults(results, papyriNameAndNumber, otherData[0], out result)) return result;

        results = CommaStrippedSearch(title, papyriNameAndNumber);
        if (results != null && CorrectResults(results, papyriNameAndNumber, otherData[0], out result)) return result;

        results = CommaPeriodStrippedSearch(title, papyriNameAndNumber);
        if (results != null && CorrectResults(results, papyriNameAndNumber, otherData[0], out result)) return result;

        results = NoTitleCommaOrPeriodSearch(papyriNameAndNumber);
        if (results != null && CorrectResults(results, papyriNameAndNumber, otherData[0], out result)) return result;

        return new BibliographyEntry($"FAILED TO FIND: {title} {papyriNameAndNumber}");
    }

    private bool CorrectResults(List<BibliographyEntry> results, string papyriNameAndNumber, string author,
        out BibliographyEntry? retVal)
    {
        (int successRate,  BibliographyEntry? result) strongestResult = new (AG_MIN, null);
        foreach (var result in results)
        {
            var corrected = CorrectResult(result, papyriNameAndNumber, author);
            var ag = corrected.Aggregate(0, (h, t) => t ? h++ : h);
            if (ag > strongestResult.successRate)
            {
                strongestResult.result = result;
                strongestResult.successRate = ag;
            }
        }

        retVal = strongestResult.result;
        return strongestResult.successRate > AG_MIN;
    }

    private List<bool> CorrectResult(BibliographyEntry result, string papyriNameAndNumber, string author)
    {
        var retVal = CorrectResultInternal(result, papyriNameAndNumber, author);
        if (!_fullPrint)
        {
            var ag = retVal.Aggregate(0, (h, t) => t ? (h + 1): h);
            //If there were any positive results
            if (ag > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write($"\tResult matched ({ag}/{retVal.Count}).");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($"\tNo matched result. {ag}/{retVal.Count}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        return retVal;
    }

    private List<BibliographyEntry>? NoTitleCommaOrPeriodSearch(string number)
    {
        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Trying Titleless Search...");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        var result = CommaPeriodTitleStripSearch(number);
        if (result != null) return result;

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.CursorLeft = Console.CursorLeft - 2;
            Console.WriteLine("Failed to find with Comma, Period, and Title Stripped search...");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        return null;
    }

    private List<BibliographyEntry>? CommaPeriodStrippedAuthorAddedSearch(string author, string title, string number)
    {
        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Trying Comma and Period Stripped, adding Author Search...");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        var result = CommaPeriodAuthorStripSearch(author, title, number);
        if (result != null) return result;

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.CursorLeft = Console.CursorLeft - 2;
            Console.WriteLine("Failed to find with Command and Period Stripped plus author search...");
            Console.WriteLine($"Searched for: {author} {title} {number}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        return null;
    }

    private List<BibliographyEntry>? CommaPeriodStrippedSearch(string title, string number)
    {

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Trying Comma and Period Stripped Search...");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        var result = PeriodCommaStripSearch(title, number);
        if (result != null) return result;
        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.CursorLeft = Console.CursorLeft - 2;
            Console.WriteLine("Failed to find with Command and Period Stripped search...");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        return null;
    }

    private List<BibliographyEntry>? CommaStrippedSearch(string title, string number)
    {

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Trying Comma Stripped Search...");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        var result = CommaStripSearch(title, number);
        if (result != null)
        {
            return result;
        }

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.CursorLeft = Console.CursorLeft - 2;
            Console.WriteLine("Failed to find with Comma Stripped search...");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        return null;
    }

    private List<BibliographyEntry>? PeriodStrippedSearch(string title, string number)
    {
        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Trying Period Stripped Search...");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        var result = PeriodStripSearch(title, number);
        if (result != null)
        {
            return result;
        }

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.CursorLeft = Console.CursorLeft - 2;
            Console.Write("Failed to find with period stripped search...\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        return null;
    }
    
    private List<BibliographyEntry>? TitleSearchWithAuthor(string title, string author)
    {
        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"Trying Number + Author Search... [{title} {author}]");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        var result = TitleWithAuthorSearch(title, author);
        if (result != null)
        {
            return result;
        }

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.CursorLeft = Console.CursorLeft - 2;
            Console.Write(" Failed to find with titleless search.\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        return null;
    }

    private List<BibliographyEntry>? GetTitlelessSearchResult(string number)
    {
        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Trying Titleless Search...");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        var result = TitlelessSearch(number);
        if (result != null)
        {
            return result;
        }

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.CursorLeft = Console.CursorLeft - 2;
            Console.Write(" Failed to find with titleless search.\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        return null;
    }

    private List<BibliographyEntry>? CommaPeriodTitleStripSearch(string number)
    {
        var numbSplit = number.Split(',');
        numbSplit = numbSplit[0].Split('.');
        var page = GetSearchPage("", numbSplit[0]);

        if (HasHits(page))
        {
            var results = GetResultsFromTable(page);
            return results;
        }

        return null;
    }
    
    private List<BibliographyEntry>? CommaPeriodAuthorStripSearch(string author, string title, string number)
    {
        var numbSplit = number.Split(',');
        numbSplit = numbSplit[0].Split('.');
        ///TODO fix this so it doesn't look like shit
        //title = author + " " + title;
        var text = $"{author} {numbSplit}";
        var page = GetSearchPage(title, text);

        if (HasHits(page))
        {
            var results = GetResultsFromTable(page);
            return results;
        }

        return null;
    }

    private List<BibliographyEntry>? PeriodCommaStripSearch(string title, string number)
    {
        var numbSplit = number.Split('.');
        numbSplit = numbSplit[0].Split(',');
        var page = GetSearchPage(title, numbSplit[^1]);

        if (HasHits(page))
        {
            var results = GetResultsFromTable(page);
            return results;
        }

        return null;
    }

    private List<BibliographyEntry>? CommaStripSearch(string title, string number)
    {
        var numbSplit = number.Split(',');
        var page = GetSearchPage(title, numbSplit[^1]);

        if (HasHits(page))
        {
            var results = GetResultsFromTable(page);
            return results;
        }

        return null;
    }

    private List<BibliographyEntry>? PeriodStripSearch(string title, string number)
    {
        var numbSplit = number.Split('.');
        var page = GetSearchPage(title, numbSplit[0]);

        if (HasHits(page))
        {
            var results = GetResultsFromTable(page);
            return results;
        }

        return null;
    }

    private List<BibliographyEntry>? TitlelessSearch(string number)
    {
        if (number == string.Empty) return null;
        
        var page = GetSearchPage("", number);

        if (HasHits(page))
        {
            var results = GetResultsFromTable(page);
            return results;
        }

        return null;
    }
    
    private List<BibliographyEntry>? TitleWithAuthorSearch(string title, string author)
    {
        var page = GetSearchPage(title, author);

        if (HasHits(page))
        {
            var results = GetResultsFromTable(page);
            return results;
        }

        return null;
    }

    private bool HasHits(HtmlDocument page)
    {
        //Check that hte page has some hits
        var paragraphNodes = page.DocumentNode.SelectNodes("//p");
        if (paragraphNodes.Any(x => x.InnerHtml.Contains("hits on")))
        {
            var node = page.DocumentNode.SelectNodes("//p")
                    .First(x => x.InnerHtml.Contains("hits on")) //Gets the HtML node
                    .ChildNodes.First(x => x.InnerHtml.Contains("hits on")) //this gets the body node
                ;
            var numberofHits = node.InnerText.Split(" ")[0];
            return Int32.TryParse(numberofHits, out int resultCount) && resultCount > 0;
        }

        return false;
    } 
    
    private List<BibliographyEntry> GetResultsFromTable(HtmlDocument page)
    {
        var results = new List<BibliographyEntry>();
        var tableParser = new TableParser();
        var searchResult = page.DocumentNode.SelectNodes("//table");
        if (searchResult != null && searchResult.Any())
        {
            foreach (var node in searchResult)
            {
                var result = tableParser.Parse(node.InnerText);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"\nPossible result: {result}. ");
                Console.ForegroundColor = ConsoleColor.Gray;
                results.Add(result);
            }
        }

        return results;
    }
    
    /*private BibliographyEntry GetResultFromTable(HtmlDocument page)
    {
        var result = new BibliographyEntry();
        var tableParser = new TableParser();
        var searchResult = page.DocumentNode.SelectNodes("//table");
        if (searchResult != null && searchResult.Any())
        {
            foreach (var node in searchResult)
            {
                result = tableParser.Parse(node.InnerText);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"\nPossible result: {result}\n");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        return result;
    }
*/
    private HtmlDocument GetSearchPage(string title, string number)
    {
        var requestClient = new HtmlWeb();
        var finalSearchUrl = SearchUrl + title + "+" + number;
        var doc = requestClient.Load(finalSearchUrl);

        return doc;
    }
}