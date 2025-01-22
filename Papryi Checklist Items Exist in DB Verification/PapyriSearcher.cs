using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PapyriChecklistItems;

namespace Papryi_Checklist_Items_Exist_in_DB_Verification;

class PapyriSearcher
{
    private const int AG_MIN = 0;
    private const string SearchUrl = "https://papyri.info/bibliosearch?q=";
    private bool _fullPrint;

    public BibliographyEntry BibliographySearch(string blockName, CheckListEntry searchItem, bool printText = false)
    {
        _fullPrint = printText;

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Starting Advanced Search for {searchItem.Title}...");
            Console.ResetColor();
        }

        return AdvancedSearch(searchItem.Title, searchItem.OtherData, searchItem.Author, searchItem.Year) ?? new BibliographyEntry();
    }

    private BibliographyEntry? AdvancedSearch(string title, string[] otherData, string? author, string? year)
    {
        var titleVariants = GenerateTitleVariants(title);
        var searchStrategies = GenerateSearchStrategies(titleVariants, author, year);

        foreach (var strategy in searchStrategies)
        {
            var results = PerformSearch(strategy.Title, strategy.Author, strategy.Year);
            if (results != null && CorrectResults(results, strategy.Title, strategy.Author, strategy.Year, out var bestMatch))
            {
                return bestMatch;
            }
        }

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("All search strategies failed.");
            Console.ResetColor();
        }

        return new BibliographyEntry($"FAILED TO FIND: {title}");
    }

    private List<(string Title, string? Author, string? Year)> GenerateSearchStrategies(List<string> titles, string? author, string? year)
    {
        var strategies = new List<(string Title, string? Author, string? Year)>();

        foreach (var title in titles)
        {
            strategies.Add((title, null, null));
            if (!string.IsNullOrEmpty(author)) strategies.Add((title, author, null));
            if (!string.IsNullOrEmpty(year)) strategies.Add((title, null, year));
            if (!string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(year)) strategies.Add((title, author, year));
        }

        return strategies;
    }

    private List<string> GenerateTitleVariants(string title)
    {
        var variants = new List<string> { title };

        // Split title by common delimiters and create combinations
        var parts = Regex.Split(title, "[,:;\n\t]+").Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToList();

        // Add each part as a standalone title
        variants.AddRange(parts);

        // Combine consecutive parts for partial matches
        for (int i = 0; i < parts.Count - 1; i++)
        {
            variants.Add(string.Join(" ", parts.Skip(i).Take(2)));
        }

        return variants.Distinct().ToList();
    }

    private List<BibliographyEntry>? PerformSearch(string title, string? author, string? year)
    {
        var searchQuery = title;
        if (!string.IsNullOrEmpty(author)) searchQuery += $"+{author}";
        if (!string.IsNullOrEmpty(year)) searchQuery += $"+{year}";

        var page = GetSearchPage(searchQuery);
        return HasHits(page) ? GetResultsFromTable(page) : null;
    }

    private bool CorrectResults(List<BibliographyEntry> results, string title, string? author, string? year, out BibliographyEntry? bestMatch)
    {
        bestMatch = null;
        var strongestMatch = (score: AG_MIN, entry: (BibliographyEntry?)null);

        foreach (var result in results)
        {
            var matchScore = EvaluateMatch(result, title, author, year);
            if (matchScore > strongestMatch.score)
            {
                strongestMatch = (matchScore, result);
            }
        }

        bestMatch = strongestMatch.entry;
        return strongestMatch.score > AG_MIN;
    }

    private int EvaluateMatch(BibliographyEntry result, string title, string? author, string? year)
    {
        int score = 0;

        if (result.Name.Contains(title, StringComparison.OrdinalIgnoreCase)) score++;
        if (!string.IsNullOrEmpty(author) && result.Name.Contains(author, StringComparison.OrdinalIgnoreCase)) score++;
        if (!string.IsNullOrEmpty(year) && result.Name.Contains(year)) score++;

        return score;
    }

    private bool HasHits(HtmlDocument page)
    {
        var paragraphNodes = page.DocumentNode.SelectNodes("//p");
        return paragraphNodes != null && paragraphNodes.Any(x => x.InnerHtml.Contains("hits on"));
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
                results.Add(tableParser.Parse(node.InnerText));
            }
        }

        return results;
    }

    private HtmlDocument GetSearchPage(string query)
    {
        var requestClient = new HtmlWeb();
        var finalSearchUrl = SearchUrl + query;
        return requestClient.Load(finalSearchUrl);
    }
}
