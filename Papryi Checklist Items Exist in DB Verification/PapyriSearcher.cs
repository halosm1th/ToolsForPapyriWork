using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace PapyriChecklistItems;

/// <summary>
/// This class will be used to search Papyri.Info for a given entry, and return its entry from the table
/// </summary>
class PapyriSearcher
{
    private const string SearchURL = "https://papyri.info/bibliosearch?q=";

    /*
/// <summary>
/// Searches a list of various bibliography entries and returns their number
/// </summary>
/// <param name="titles">The titles of the jounral/collection</param>
/// <param name="numbers">The volume numbers</param>
/// <param name="otherDatas">Other data to be used in determining the entry, such as year, or authors name</param>
/// <returns>Returns a list of tuples strings and int which are the papyri.info bibliography numbers for the given item in the format of (name, number)</returns>
    public List<BibliographyEntry> BibliographySearches(List<string> titles, List<string> numbers, List<List<string>> otherDatas)
    {
        var results = new List<BibliographyEntry>() { };
        for(int i=0; i <titles.Count; i++)
        {
            var result = BibliographySearch(titles[i],numbers[i], otherDatas[i].ToArray());
            results.Add(result);
        }

        return results;
    }

public BibliographyEntry BibliographySearch(string blockName, string title, string[] other)
{

    var result = new BibliographyEntry();
    var page = GetSearchPage(blockName,title);

    if (HasHits(page))
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"Starting Basic Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        result = GetResultFromTable(page);
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("No hits on base search.\nAttempting Advanced Search...\n");
        Console.ForegroundColor = ConsoleColor.Gray;
        result = AdvancedSearch(blockName,title, other);
    }
        
    return result;
}
*/
    
    public BibliographyEntry BibliographySearch(string blockName, CheckListEntry searchItem)
    {
        var page = GetSearchPage(blockName,searchItem.Title);
        
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"Starting Basic Search... ");
        Console.ForegroundColor = ConsoleColor.Gray;
        if (HasHits(page))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Hit found, getting and testing result.");
            Console.ForegroundColor = ConsoleColor.Gray;
            var results = GetResultsFromTable(page);
            foreach(var result in results){
                //If  we find the correct answer, return early, because we did it!
                if (correctResult(result, searchItem.Title, searchItem.OtherData[0])) 
                    return result;
            }
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Failed on base search.\nAttempting Advanced Search...\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("No hits on base search.\nAttempting Advanced Search...\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        
        return AdvancedSearch(blockName,searchItem.Title, searchItem.OtherData) ?? new BibliographyEntry();
    }

    private bool correctResult(BibliographyEntry result, string titleOfPapyri, string author)
    {
        var yearTry = Regex.Match(input: titleOfPapyri, pattern: "(18|19|20)[0-9]{2}");
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\tResult will be checked against:\n\t\tTitle: {titleOfPapyri}.\n\t\tYear: {yearTry.Value}." +
                          $"\n\t\tAuthor: {author}.");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        /* This does the same thing as better explained version below. It exists simply to show off what conditionals
         can do and the fun one can have with chaining them. This is not good code.
        return result.Name.Contains(titleOfPapyri) ? true :
            yearTry.Success && result.Name.Contains(yearTry.Value) ? true :
            result.Name.Contains(author); 
        */
        
        var titleTest = result.Name.Contains(titleOfPapyri);
        if (titleTest)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\tResult matched with title {titleOfPapyri} successfully.");
            Console.ForegroundColor = ConsoleColor.Gray;
            return true;
        }
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"\t\tFailed to match with {titleOfPapyri}. Trying year next");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        //Check if the title has something like looks like a year
        if (yearTry.Success && result.Name.Contains(yearTry.Value))
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\tResult matched with year {yearTry.Value} successfully.");
            Console.ForegroundColor = ConsoleColor.Gray;
            return true;
        }
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"\t\tFailed to match with year: {yearTry.Value}. Trying author next");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        var authorTry = result.Name.Contains(author);
        if (authorTry)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\tResult matched with author {author} successfully.");
            Console.ForegroundColor = ConsoleColor.Gray;
            return true;
        }
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"\t\tFailed to match with author {author}. Trying Comma Stripped title next.");
        Console.ForegroundColor = ConsoleColor.Gray;

        var commaStrippedTitle = titleOfPapyri.Split(',');
        var commaStrippedTest = result.Name.Contains(commaStrippedTitle[1]);
        if (commaStrippedTest)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\tResult matched with title with comma stripped {commaStrippedTitle[1]} successfully.");
            Console.ForegroundColor = ConsoleColor.Gray;
            return true;
        }
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"\t\tFailed to match with comma stripped {commaStrippedTitle[1]}. Trying period stripped next");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        var periodStrippedTitle = titleOfPapyri.Split('.');
        var periodStrippedTest = result.Name.Contains(periodStrippedTitle[0]);
        if (periodStrippedTest)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\tResult matched with title with comma stripped {periodStrippedTitle[0]} successfully.");
            Console.ForegroundColor = ConsoleColor.Gray;
            return true;
        }
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"\t\tFailed to match with comma stripped {periodStrippedTitle[0]}. Trying comma and period stripped next");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        var commaAndPeriodStrippedTitle = periodStrippedTitle[0].Split(",");
        var nametemp = commaAndPeriodStrippedTitle[1];
        var resultName = result.Name;
        var commaAndPeriodStrippedTest = resultName.Contains(nametemp);
        if (commaAndPeriodStrippedTest)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\tResult matched with title with comma stripped {commaAndPeriodStrippedTitle[1]} successfully.");
            Console.ForegroundColor = ConsoleColor.Gray;
            return true;
        }
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"\t\tFailed to match with comma and period stripped {commaAndPeriodStrippedTitle[1]}.");
        Console.ForegroundColor = ConsoleColor.Gray;

        /*
        var matchTry = result.Name.Contains(result.MatchingText);
        if (matchTry)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\tResult matched with matching text: {result.MatchingText}.");
            Console.ForegroundColor = ConsoleColor.Gray;
            return true;
        }
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"\t\tFailed to match with matching text {result.MatchingText}.");
        Console.ForegroundColor = ConsoleColor.Gray;
        */
        
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine($"NO CORRECT MATCHES FOUND IN TITLE, YEAR, OR AUTHOR of {result.Name}");
        Console.ForegroundColor = ConsoleColor.Gray;
        return false;
    }

    //Advanced search is basically me massassing various errors I've found over time, trying to figure out how to best
    //make this all work, as such, its essentially the last stop check point offering various strategies, if none work,
    //then the user has to try and find the result themselves.
    private BibliographyEntry? AdvancedSearch(string title, string PapyriNameAndNumber, string[] otherData)
    {
        List<BibliographyEntry>? results = null;
        BibliographyEntry? result = null;
        
        results = GetTitlelessSearchResult(PapyriNameAndNumber);
        if (results != null && correctResults(results,PapyriNameAndNumber, otherData[0], out result)) return result;

        results = PeriodStrippedSearch(title, PapyriNameAndNumber);
        if (results != null && correctResults(results,PapyriNameAndNumber, otherData[0], out result)) return result;

        results = TitleSearchWithAuthor(title, otherData[0]);
        if (results != null && correctResults(results,PapyriNameAndNumber, otherData[0], out result)) return result;
        
        results = CommaPeriodStrippedAuthorAddedSearch(otherData[0], title, PapyriNameAndNumber);
        if (results != null && correctResults(results,PapyriNameAndNumber, otherData[0], out result)) return result;
        
        results = CommaStrippedSearch(title, PapyriNameAndNumber);
        if (results != null && correctResults(results,PapyriNameAndNumber, otherData[0], out result)) return result;
        
        results = CommaPeriodStrippedSearch(title, PapyriNameAndNumber);
        if (results != null && correctResults(results,PapyriNameAndNumber, otherData[0], out result)) return result;
        
        results = NoTitleCommaOrPeriodSearch(PapyriNameAndNumber);
        if (results != null && correctResults(results,PapyriNameAndNumber, otherData[0], out result)) return result;

        return new BibliographyEntry($"FAILED TO FIND: {title} {PapyriNameAndNumber}");
    }

    private bool correctResults(List<BibliographyEntry> results, string papyriNameAndNumber, string author, out BibliographyEntry? retVal)
    {
        foreach (var result in results)
        {
            if (correctResult(result, papyriNameAndNumber, author))
            {
                retVal = result;
                return true;
            }
        }

        retVal = null;
        return false;
    }

    private List<BibliographyEntry>? NoTitleCommaOrPeriodSearch(string number)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Titleless Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        var result = CommaPeriodTitleStripSearch(number);
        if (result != null) return result;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.WriteLine("Failed to find with Comma, Period, and Title Stripped search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        return null;
    }

    private List<BibliographyEntry>? CommaPeriodStrippedAuthorAddedSearch(string author, string title, string number)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Comma and Period Stripped, adding Author Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        var result = CommaPeriodAuthorStripSearch(author,title, number);
        if (result != null) return result;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.WriteLine("Failed to find with Command and Period Stripped plus author search...");
        Console.WriteLine($"Searched for: {author} {title} {number}");
        Console.ForegroundColor = ConsoleColor.Gray;
        return null;
    }

    private List<BibliographyEntry>? CommaPeriodStrippedSearch(string title, string number)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Comma and Period Stripped Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        var result = PeriodCommaStripSearch(title, number);
        if (result != null) return result;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.WriteLine("Failed to find with Command and Period Stripped search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        return null;
    }

    private List<BibliographyEntry>? CommaStrippedSearch(string title, string number)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Comma Stripped Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        var result = CommaStripSearch(title, number);
        if (result != null)
        {
            return result;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.WriteLine("Failed to find with Comma Stripped search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        return null;
    }

    private List<BibliographyEntry>? PeriodStrippedSearch(string title, string number)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Period Stripped Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        var result = PeriodStripSearch(title, number);
        if (result != null)
        {
            return result;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.Write("Failed to find with period stripped search...\n");
        Console.ForegroundColor = ConsoleColor.Gray;

        return null;
    }
    
    private List<BibliographyEntry>? TitleSearchWithAuthor(string title, string author)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write($"Trying Number + Author Search... [{title} {author}]");
        Console.ForegroundColor = ConsoleColor.Gray;
        var result = TitleWithAuthorSearch(title, author);
        if (result != null)
        {
            return result;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.Write(" Failed to find with titleless search.\n");
        Console.ForegroundColor = ConsoleColor.Gray;
        return null;
    }

    private List<BibliographyEntry>? GetTitlelessSearchResult(string number)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Titleless Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        var result = TitlelessSearch(number);
        if (result != null)
        {
            return result;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.Write(" Failed to find with titleless search.\n");
        Console.ForegroundColor = ConsoleColor.Gray;
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
        var page = GetSearchPage(title, numbSplit[numbSplit.Length-1]);

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
        var page = GetSearchPage(title, numbSplit[1]);

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
                Console.Write($"\nPossible result: {result}\n");
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
        var RequestClient = new HtmlWeb();
        var finalSearchURL = SearchURL + title + "+" + number;
        var doc = RequestClient.Load(finalSearchURL);

        return doc;
    }
}