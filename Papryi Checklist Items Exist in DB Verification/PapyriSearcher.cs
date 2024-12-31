using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using HtmlAgilityPack;

namespace PapyriChecklistItems;

/// <summary>
/// This class will be used to search Papyri.Info for a given entry, and return its entry from the table
/// </summary>
class PapyriSearcher
{
    private const string SearchURL = "https://papyri.info/bibliosearch?q=";

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
            var result = BibliographySearch(titles[i],numbers[i], otherDatas[i]);
            results.Add(result);
        }

        return results;
    }
    
    public BibliographyEntry BibliographySearch(string title, string number, List<string> otherData)
    {

        var result = new BibliographyEntry();
        var page = GetSearchPage(title, number);

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
            result = AdvancedSearch(title, number, otherData);
        }
        
        return result;
    }

    //Advanced search is basically me massassing various errors I've found over time, trying to figure out how to best
    //make this all work, as such, its essentially the last stop check point offering various strategies, if none work,
    //then the user has to try and find the result themselves.
    private BibliographyEntry? AdvancedSearch(string title, string number, List<string> otherData)
    {
        BibliographyEntry? result = null;
        
        if (TitlelessSearch(number, out result)) return result;

        if (PeriodStrippedSearch(title, number, out result)) return result;
        if (TitleSearchWithAuthor(title, otherData[0], out result)) return result;

        if (CommaPeriodStrippedAuthorAddedSearch(otherData[0], title, number, out result)) return result;
        if (CommaStrippedSearch(title, number, out result)) return result;

        if (CommaPeriodStrippedSearch(title, number, out result)) return result;


        if (NoTitleCommaOrPeriodSearch(number, out result)) return result;

        return new BibliographyEntry($"FAILED TO FIND: {title} {number}");
    }

    private bool NoTitleCommaOrPeriodSearch(string number, out BibliographyEntry? result)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Titleless Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        result = CommaPeriodTitleStripSearch(number);
        if (result != null) return true;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.WriteLine("Failed to find with Comma, Period, and Title Stripped search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        return false;
    }

    private bool CommaPeriodStrippedAuthorAddedSearch(string author, string title, string number, out BibliographyEntry? result)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Comma and Period Stripped, adding Author Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        result = CommaPeriodAuthorStripSearch(author,title, number);
        if (result != null) return true;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.WriteLine("Failed to find with Command and Period Stripped plus author search...");
        Console.WriteLine($"Searched for: {author} {title} {number}");
        Console.ForegroundColor = ConsoleColor.Gray;
        return false;
    }

    private bool CommaPeriodStrippedSearch(string title, string number, out BibliographyEntry? result)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Comma and Period Stripped Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        result = CommaPeriodStripSearch(title, number);
        if (result != null) return true;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.WriteLine("Failed to find with Command and Period Stripped search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        return false;
    }

    private bool CommaStrippedSearch(string title, string number, out BibliographyEntry? result)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Comma Stripped Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        result = CommaStripSearch(title, number);
        if (result != null)
        {
            return true;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.WriteLine("Failed to find with Comma Stripped search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        return false;
    }

    private bool PeriodStrippedSearch(string title, string number, out BibliographyEntry? result)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Period Stripped Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        result = PeriodStripSearch(title, number);
        if (result != null)
        {
            return true;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.Write("Failed to find with period stripped search...\n");
        Console.ForegroundColor = ConsoleColor.Gray;

        return false;
    }
    
    private bool TitleSearchWithAuthor(string title, string author, out BibliographyEntry? result)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write($"Trying Number + Author Search... [{title} {author}]");
        Console.ForegroundColor = ConsoleColor.Gray;
        result = TitleWithAuthorSearch(title, author);
        if (result != null)
        {
            return true;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.Write(" Failed to find with titleless search.\n");
        Console.ForegroundColor = ConsoleColor.Gray;
        return false;
    }

    private bool TitlelessSearch(string number, out BibliographyEntry? result)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Trying Titleless Search...");
        Console.ForegroundColor = ConsoleColor.Gray;
        result = TitlelessSearch(number);
        if (result != null)
        {
            return true;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.CursorLeft = Console.CursorLeft - 2;
        Console.Write(" Failed to find with titleless search.\n");
        Console.ForegroundColor = ConsoleColor.Gray;
        return false;
    }

    private BibliographyEntry? CommaPeriodTitleStripSearch(string number)
    {
        var numbSplit = number.Split(',');
        numbSplit = numbSplit[0].Split('.');
        var page = GetSearchPage("", numbSplit[0]);

        if (HasHits(page))
        {
            return GetResultFromTable(page);
        }

        return null;
    }
    
    private BibliographyEntry? CommaPeriodAuthorStripSearch(string author, string title, string number)
    {
        var numbSplit = number.Split(',');
        numbSplit = numbSplit[0].Split('.');
        ///TODO fix this so it doesn't look like shit
        //title = author + " " + title;
        var page = GetSearchPage(title, author);

        if (HasHits(page))
        {
            return GetResultFromTable(page);
        }

        return null;
    }

    private BibliographyEntry? CommaPeriodStripSearch(string title, string number)
    {
        var numbSplit = number.Split(',');
        numbSplit = numbSplit[0].Split('.');
        var page = GetSearchPage(title, numbSplit[0]);

        if (HasHits(page))
        {
            return GetResultFromTable(page);
        }

        return null;
    }

    private BibliographyEntry? CommaStripSearch(string title, string number)
    {
        var numbSplit = number.Split(',');
        var page = GetSearchPage(title, numbSplit[0]);

        if (HasHits(page))
        {
            return GetResultFromTable(page);
        }

        return null;
    }

    private BibliographyEntry? PeriodStripSearch(string title, string number)
    {
        var numbSplit = number.Split('.');
        var page = GetSearchPage(title, numbSplit[0]);

        if (HasHits(page))
        {
            return GetResultFromTable(page);
        }

        return null;
    }

    private BibliographyEntry? TitlelessSearch(string number)
    {
        var page = GetSearchPage("", number);

        if (HasHits(page))
        {
            return GetResultFromTable(page);
        }

        return null;
    }
    
    private BibliographyEntry? TitleWithAuthorSearch(string title, string author)
    {
        var page = GetSearchPage(title, author);

        if (HasHits(page))
        {
            return GetResultFromTable(page);
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
    
    private BibliographyEntry GetResultFromTable(HtmlDocument page)
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

    private HtmlDocument GetSearchPage(string title, string number)
    {
        var RequestClient = new HtmlWeb();
        var finalSearchURL = SearchURL + title + "+" + number;
        var doc = RequestClient.Load(finalSearchURL);

        return doc;
    }
}