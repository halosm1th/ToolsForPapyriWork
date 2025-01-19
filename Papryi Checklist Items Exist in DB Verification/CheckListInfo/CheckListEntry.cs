namespace PapyriChecklistItems;

internal class CheckListEntry : ParsedCheckListItem
{
    public string Title { get; set; } 
    public string FullText { get; set; }
    public string[] OtherData { get; set; }
    public List<string> TitleVariations { get; set; }
    public string? Author { get; set; }
    public string? PublicationYear { get; set; }
    public string? PublicationLocation { get; set; }

    public CheckListEntry(string title, string fullText, string[] otherData, List<string> titleVariations, string author, string? publicationYear = null, string? publicationLocation = null) 
        : base(title, fullText)
    {
        Title = title;
        FullText = fullText;
        OtherData = otherData;
        TitleVariations = titleVariations;
        Author = author;
        PublicationYear = publicationYear;
        PublicationLocation = publicationLocation;
    }

    public override string ToString()
    {
        return $"Title: {Title}, Author: {Author}, Publication Year: {PublicationYear}, Publication Location: {PublicationLocation}, Other Data: [{string.Join(", ", OtherData)}], Title Variations: [{string.Join(", ", TitleVariations)}]";
    }
}