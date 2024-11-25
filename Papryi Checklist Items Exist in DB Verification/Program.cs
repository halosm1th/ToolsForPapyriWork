namespace PapyriChecklistItems;
using System.Net;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
class PapyriChecklistInfo
{
    public static void Main()
    {
        var searcher = new PapyriSearcher();
        searcher.BibliographySearch("Actenstücke", "", new() {"1887", "London", "Paris"});
        Console.ReadLine();
    }
}

class PapyriSearcher
{
    private const string SearchURL = "https://papyri.info/bibliosearch?q=";


    public List<string> BibliographySearches(List<string> titles, List<string> numbers, List<List<string>> otherDatas)
    {
        var results = new List<string>() { };
        for(int i=0; i <titles.Count; i++)
        {
            var result = BibliographySearch(titles[i], numbers[i], otherDatas[i]);
            results.Add(result);
        }

        return results;
    }
    
    public string BibliographySearch(string title, string number, List<string> otherData)
    {

        var result = "";
        var page = GetSearchPage(title, number);

        if (HasHits(page))
        {
            result = GetResultFromTable(page, result);
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
    
    private string GetResultFromTable(HtmlDocument page, string result)
    {
        var searchResult = page.DocumentNode.SelectNodes("//table");
        if (searchResult != null && searchResult.Any())
        {
            foreach (var node in searchResult)
            {
                result = node.InnerText;
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