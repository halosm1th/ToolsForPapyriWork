using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PapyriChecklistItems;

namespace Papryi_Checklist_Items_Exist_in_DB_Verification;

/// <summary>
/// Searches Papyri.info for a given entry and returns the best matching result.
/// </summary>
class PapyriSearcher
{
    private const string SearchUrl = "https://papyri.info/bibliosearch?q=";

    public BibliographyEntry BibliographySearch(string blockName, CheckListEntry searchItem)
    {
        var strategies = GenerateStrategies(blockName, searchItem);
        BibliographyEntry bestMatch = null;
        int highestScore = 0;

        foreach (var query in strategies)
        {
            var page = GetSearchPage(query);

            if (HasHits(page))
            {
                var results = GetResultsFromTable(page);

                foreach (var result in results)
                {
                    int score = CalculateMatchScore(result, searchItem);
                    if (score > highestScore)
                    {
                        highestScore = score;
                        bestMatch = result;
                    }
                }
            }
        }

        return bestMatch;
    }

    private List<string> GenerateStrategies(string blockName, CheckListEntry searchItem)
    {
        var strategies = new List<string>();

        foreach (var title in searchItem.TitleVariations)
        {
            strategies.Add(title);
            strategies.Add($"{title} {blockName}");
            strategies.Add($"{title} {searchItem.Author}");
            strategies.Add($"{title} {searchItem.Author} {blockName}");
            if (!string.IsNullOrEmpty(searchItem.PublicationYear))
            {
                strategies.Add($"{title} {searchItem.Author} {searchItem.PublicationYear}");
                strategies.Add($"{title} {searchItem.Author} {blockName} {searchItem.PublicationYear}");
            }
        }

        return strategies.Distinct().ToList();
    }

    private int CalculateMatchScore(BibliographyEntry result, CheckListEntry searchItem)
    {
        int score = 0;
        string normalizedResultName = NormalizeText(result.Name ?? "" );
        string normalizedAuthor = NormalizeText(searchItem.Author ?? "");

        foreach (var title in searchItem.TitleVariations)
        {
            string normalizedTitle = NormalizeText(title);
            if (IsSimilar(normalizedResultName, normalizedTitle))
            {
                score += 5;
            }
        }

        if (!string.IsNullOrEmpty(searchItem.PublicationYear) && normalizedResultName.Contains(searchItem.PublicationYear))
        {
            score += 3;
        }

        if (normalizedResultName.Contains(normalizedAuthor))
        {
            score += 2;
        }

        score -= Math.Abs(normalizedResultName.Length - searchItem.Title.Length);

        return score;
    }

    private string NormalizeText(string text)
    {
        return string.Concat(
            text.Normalize(System.Text.NormalizationForm.FormD)
                .Where(ch => char.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark))
            .ToLowerInvariant();
    }

    private bool IsSimilar(string source, string target, int threshold = 3)
    {
        return LevenshteinDistance(source, target) <= threshold;
    }

    private int LevenshteinDistance(string source, string target)
    {
        var matrix = new int[source.Length + 1, target.Length + 1];
        for (int i = 0; i <= source.Length; i++) matrix[i, 0] = i;
        for (int j = 0; j <= target.Length; j++) matrix[0, j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }

    private bool HasHits(HtmlDocument page)
    {
        var paragraphNodes = page.DocumentNode.SelectNodes("//p");
        return paragraphNodes?.Any(x => x.InnerHtml.Contains("hits on")) ?? false;
    }

    private List<BibliographyEntry> GetResultsFromTable(HtmlDocument page)
    {
        var results = new List<BibliographyEntry>();
        var tableRows = page.DocumentNode.SelectNodes("//tr[contains(@class, 'result-record')]");

        if (tableRows != null)
        {
            foreach (var row in tableRows)
            {
                var link = row.SelectSingleNode(".//a");
                if (link != null)
                {
                    var content = link.InnerText.Trim();
                    var bibNumberMatch = Regex.Match(content, "^(\\d+)(?:\\.\\s|\\.)");

                    if (bibNumberMatch.Success)
                    {
                        var number = int.Parse(bibNumberMatch.Groups[1].Value);
                        var entryText = content.Substring(bibNumberMatch.Value.Length).Trim();
                        results.Add(new BibliographyEntry(entryText, number));
                    }
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

public class BibliographyEntry
{
    public string Name { get; }
    public int Number { get; }

    public BibliographyEntry(string name, int number)
    {
        Name = name;
        Number = number;
    }
}
