namespace PapyriChecklistItems;
using System.Net;
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