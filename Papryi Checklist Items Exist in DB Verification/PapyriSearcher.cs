using System.Net.Mime;
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
            var result = BibliographySearch(titles[i], numbers[i], otherDatas[i]);
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
            result = GetResultFromTable(page);
        }
        
        return result;
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
                Console.WriteLine(result);
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