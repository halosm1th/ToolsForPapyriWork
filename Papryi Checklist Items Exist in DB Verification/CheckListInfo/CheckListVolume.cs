namespace PapyriChecklistItems;

internal record CheckListVolume
    (string VolumeTitle, string? Date, string? Author, string? Nos, string[] OtherInfo, string FullText) : CheckListEntry(VolumeTitle,
        Author, Date,
        FullText, new List<string>{ Author ?? "",Date ?? "",Nos ?? "", OtherInfo.Aggregate("", (h,t) => h = h + " " + t)}.ToArray())
{
    public override string ToString()
    {
        return VolumeTitle;
    }
};