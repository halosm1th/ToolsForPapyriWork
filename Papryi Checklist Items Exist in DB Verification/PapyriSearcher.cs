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
            Console.WriteLine($"Starting Search for {searchItem.Title}...");
            Console.ResetColor();
        }

        return AdvancedSearch(searchItem.Title, blockName, searchItem.OtherData, searchItem.Author, searchItem.Year) ?? new BibliographyEntry();
    }

    private BibliographyEntry? AdvancedSearch(string title, string source, string[] otherData, string? author, string? year)
    {
        var titleVariants = GenerateTitleVariants(title);
        var searchStrategies = GenerateSearchStrategies(source, titleVariants, author, year);

        BibliographyEntry? mostCorrectResult = null;
        int highestScore = AG_MIN;

        foreach (var strategy in searchStrategies)
        {
            var results = PerformSearch(strategy.Title, strategy.Source, strategy.Author, strategy.Year);
            if (results != null)
            {
                foreach (var result in results)
                {
                    var currentScore = EvaluateMatch(result, strategy.Title, strategy.Author, strategy.Year);
                    if (currentScore > highestScore)
                    {
                        highestScore = currentScore;
                        mostCorrectResult = result;
                    }
                }
            }
        }

        if (mostCorrectResult != null)
        {
            if (_fullPrint)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Most correct result: {mostCorrectResult.Name}");
                Console.ResetColor();
            }

            return mostCorrectResult;
        }

        if (_fullPrint)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("All search strategies failed.");
            Console.ResetColor();
        }

        return new BibliographyEntry($"FAILED TO FIND: {title}");
    }

    private List<(string Title, string? Source, string? Author, string? Year)> GenerateSearchStrategies(string source, List<string> titles, string? author, string? year)
    {
        var strategies = new List<(string Title, string? Source, string? Author, string? Year)>();
        
        var authors = new List<string>() {author};

        if (author != null)
        {
            var authorParts = author.Split('.');
            foreach (var p in authorParts)
            {
                authors.Add(p);
            }
        }


        foreach (var title in titles)
        {
            if (authors.Count > 0)
            {

                foreach (var auth in authors)
                {
                    //Title + Source
                    if (!string.IsNullOrEmpty(source)) strategies.Add((title, source, null, null));
                    //Title
                    strategies.Add((title, null, null, null));
                    //Title and Author
                    if (!string.IsNullOrEmpty(auth)) strategies.Add((title, null, auth, null));
                    //Title and Year
                    if (!string.IsNullOrEmpty(year)) strategies.Add((title, null, null, year));

                    //Title, Source, Author
                    if (!string.IsNullOrEmpty(auth) && !string.IsNullOrEmpty(source))
                        strategies.Add((title, source, author, null));
                    //Title, Source, Year
                    if (!string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(source))
                        strategies.Add((title, source, null, year));
                    //Title, Author, Year
                    if (!string.IsNullOrEmpty(auth) && !string.IsNullOrEmpty(year))
                        strategies.Add((title, null, auth, year));

                    //Title, Author, Year, and Source
                    if (!string.IsNullOrEmpty(auth) && !string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(source))
                        strategies.Add((title, source, auth, year));
                }
            }
            else
            {
                
                //Title + Source
                if (!string.IsNullOrEmpty(source)) strategies.Add((title, source, null, null));
                //Title
                strategies.Add((title, null, null, null));
                //Title and Author
                if (!string.IsNullOrEmpty(author)) strategies.Add((title, null, author, null));
                //Title and Year
                if (!string.IsNullOrEmpty(year)) strategies.Add((title, null, null, year));

                //Title, Source, Author
                if (!string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(source))
                    strategies.Add((title, source, author, null));
                //Title, Source, Year
                if (!string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(source))
                    strategies.Add((title, source, null, year));
                //Title, Author, Year
                if (!string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(year))
                    strategies.Add((title, null, author, year));

                //Title, Author, Year, and Source
                if (!string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(source))
                    strategies.Add((title, source, author, year));
            }
            
        }

        return strategies;
    }

    private List<string> GenerateTitleVariants(string title)
    {
        var variants = new List<string> { title };

        // Split title by common delimiters and create combinations
        var parts = Regex.Split(title, "[,:;\\n\\t]+")
            .Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToList();

        // Add each part as a standalone title
        variants.AddRange(parts);

        // Combine consecutive parts for partial matches
        for (int i = 0; i < parts.Count - 1; i++)
        {
            variants.Add(string.Join(" ", parts.Skip(i).Take(2)));
        }

        return variants.Distinct().ToList();
    }

    private List<BibliographyEntry>? PerformSearch(string title, string? source, string? author, string? year)
    {
        
        string searchQuery = $"{source}+" ?? ""; 
        searchQuery += $"{title}";
        if (!string.IsNullOrEmpty(author)) searchQuery += $"+{author}";
        if (!string.IsNullOrEmpty(year)) searchQuery += $"+{year}";

        var page = GetSearchPage(searchQuery);
        return HasHits(page) ? GetResultsFromTable(page) : null;
    }

    private int EvaluateMatch(BibliographyEntry result, string title , string? author, string? year)
    {
        int score = 0;

        // Full title match (case-insensitive)
        if (string.Equals(result.Title, title, StringComparison.OrdinalIgnoreCase))
        {
            score += 5; // Full title match score
        }
        else
        {
            // Partial title match scoring
            int partialTitleScore = CalculatePartialTitleScore(result.Title, title);
            score += partialTitleScore; // Add partial match score
        }

        // Author match (case-insensitive)
        if (!string.IsNullOrEmpty(author) && result.Author.Contains(author, StringComparison.OrdinalIgnoreCase))
        {
            score += 4; // Author match score
        }
        else
        {
            // Partial author match scoring
            if (result.Author != null && author != null)
            {
                int partialTitleScore = CalculatePartialAuthorScore(result.Author, author ?? "");
                score += partialTitleScore; // Add partial match score
            }
        }

        // Year match
        if (!string.IsNullOrEmpty(year) && result.PublicationDate == year)
        {
            score += 3; // Year match score
        }

        return score;
    }

    private int CalculatePartialTitleScore(string resultTitle, string targetTitle)
    {
        var matchPercentage = CalculateMatchPercentage(resultTitle, targetTitle);

        // Scale the score from 1 to 4
        if (matchPercentage >= 0.75)
            return 4; // High partial match
        if (matchPercentage >= 0.50)
            return 3; // Moderate partial match
        if (matchPercentage >= 0.25)
            return 2; // Low partial match
        return 1; // Minimal partial match
    }

    private int CalculatePartialAuthorScore(string resultTitle, string targetTitle)
    {
        var matchPercentage = CalculateMatchPercentage(resultTitle, targetTitle);

        // Scale the score from 1 to 3
        if (matchPercentage >= 0.66)
            return 3; // Moderate partial match
        if (matchPercentage >= 0.33)
            return 2; // Low partial match
        return 1; // Minimal partial match
    }
    
    private static double CalculateMatchPercentage(string resultTitle, string targetTitle)
    {
        // Normalize and tokenize titles for comparison
        var resultTokens = resultTitle.ToLower()
            .Split(new[] {' ', ',', '.', ';', ':'}, StringSplitOptions.RemoveEmptyEntries);
        var targetTokens = targetTitle.ToLower()
            .Split(new[] {' ', ',', '.', ';', ':'}, StringSplitOptions.RemoveEmptyEntries);

        // Calculate intersection of tokens
        var commonTokens = resultTokens.Intersect(targetTokens).Count();

        // Calculate score based on token overlap as a percentage of the target title
        double matchPercentage = (double) commonTokens / targetTokens.Length;

        // Penalize the score if result title is longer than target title
        double lengthRatio = (double) resultTitle.Length / targetTitle.Length;
        if (lengthRatio > 1.2) // Apply penalty if result title is more than 20% longer than target title
        {
            matchPercentage -= 0.1 * (lengthRatio - 1.0); // Decrease match percentage based on length difference
        }

        // Ensure match percentage is not negative
        return Math.Max(0, matchPercentage);
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
                if (node.InnerText != "\n")
                {
                    results.Add(tableParser.Parse(node.InnerText));
                }
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
