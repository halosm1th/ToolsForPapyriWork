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
    HtmlWeb RequestClient = new HtmlWeb();
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
        var finalSearchURL = SearchURL + title + "+" + number;
        var page = RequestClient.Load(finalSearchURL);

        var searchResult = page.DocumentNode.SelectNodes("//td/").First();

        foreach (var node in searchResult.ChildNodes)
        {
            Console.WriteLine(node.InnerText);
        }
        
        return result;
    }
}