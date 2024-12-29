using System.ComponentModel.Design;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PapyriChecklistItems;

class ChecklistChecker
{

    
    private string filePath;
    private string[] CheckListLines;
    private List<CheckListToken> TokenList;
    
    
    public ChecklistChecker(string checklistLocation = "/checklist.md")
    {
        filePath = Directory.GetCurrentDirectory() + checklistLocation;
        CheckListLines = File.ReadAllLines(filePath);
    }

    private int GetPapyriInfoIndex()
    {
        int index = 0;

        for (; index < CheckListLines.Length; index++)
        {
            //The start of papyri sections are all ###
            if (CheckListLines[index].StartsWith("###"))
            {
                return index;
            }
        }

        throw new IndexOutOfRangeException("No header starting the Papyri Info section could be found at: " + filePath);
    }

    public void ParseTokenizedCheckList()
    {
        var parsedTokens = new List<ParsedCheckListItem>();
        
        foreach (var token in TokenList)
        {
            if (token.TokenType == ChecklistTokenType.Header)
            {
                parsedTokens.Add(ParseHeader(token.TokenText));
                Console.WriteLine(((CheckListHeader) parsedTokens.Last()).Header);
            }else if (token.TokenType == ChecklistTokenType.Journal)
            {
                parsedTokens.Add(ParseJournal(token.TokenText));
                Console.WriteLine(((CheckListJournal) parsedTokens.Last()).JournalTitle);

            }else if (token.TokenType == ChecklistTokenType.Volume)
            {
                parsedTokens.Add(ParseVolume(token.TokenText));
                Console.WriteLine(((CheckListVolume) parsedTokens.Last()).VolumeTitle);
            }
        }
    }

    //I, ed. F. Preisigke. Berlin/Leipzig 1922. &#91;WdG&#93; [Online: archive.org](https://archive.org/details/pst.000010146934)
    //I pt. 1, ed. A. Calderini. Cairo 1935. [Rp. CG 1972]; pt. 2, ed. A. Calderini. Madrid 1966; [Rp. CG 1972]
    //III, Nos. 972—1111, see [P.Mich.](#P.Mich.) VIII.
    //I, ed. H. Cuvigny and G. Wagner. 1986. (Institut Français d’Archéologie Orientale, Documents de Fouilles 24/1). Nos. 1—57. Nos. 40 and 49 are Coptic; no. 44 is termed Graeco-Coptic. [SEVPO]
    //III, Berlin and Leipzig 1926—1927. Nos. 6001—7269. [MF 1.35; rp. WdG]; [Online: archive.org](https://archive.org/details/sammelbuchgriech03unse)
    //I, ed. B.P. Grenfell, A.S. Hunt and J.G. Smyly. 1902. (Univ. of California Publications, Graeco-Roman Archaeology I; Egypt Exploration Society, Graeco-Roman Memoirs 4). Nos. 1—264. &#91;EES&#93; [Online: archive.org](https://archive.org/details/tebtunispapyri00brangoog)
    //I, Literarische Texte, ed. G. Zereteli. 1925. Nos. 1—24. [MF 2.1]
    //I (in 3 parts), ed. P.M. Meyer. Leipzig-Berlin 1911—1924. Pt. I, nos. 1—23; pt. II, nos. 24—56; pt. III, nos. 57—117. [MF 2.103: rp. CG] <ddb:p.hamb;1> [Online: archive.org](https://archive.org/details/griechischepapyr00meye)
    //I, Griechische Papyrusurkunden aus ptolemäischer und römischer Zeit, ed. H. Kling. 1924. (Schriften der hessischen Hochschulen, Universität Giessen 1924, 4). Nos. 1—16. [MF 2.20; rp. CG] <ddb:p.giss.univ;1> [Online: archive.org](https://archive.org/details/griechischepapyr00klin)
    //I, 2nd ed., ed. P.Schubert and I. Jornot with contributions by C. Wick. Geneva 2002. Nos. 1-10, 12-44, 66-78 and 80-81 of the 1st edition are reedited here. The other texts are of the Abinnaeus Archive and have already been reedited in [P.Abinn.](#P.Abinn.) [Bibliothèque Publique et Universitaire] <ddb:p.gen.2> [Online: réro](http://doc.rero.ch/record/27210)
    //Part I, Journal des Savants 1995. pages 65–119, nos. 1–5, all on papyrus. No. 1 has three Syriac characters at the end. Nos. 3 and 4 each has a signature in Syriac. No. 5 has a subscription in Latin. Reprinted SB XXII 15496–15500. [Online: Persée](https://www.persee.fr/doc/jds_0021-8103_1995_num_1_1_1584)
    //I, Demotic Texts from the Collection, ed. P.J. Frandsen with contributions by K.-Th. Zauzich, W.J. Tait and M. Chauveau. Copenhagen 1991. 4 Demotic texts are published by inventory number, P.dem.Carlsb. inv. 207, 230, 236 and 301. On pp. 129—140 Tait lists the published Carlsberg texts, Demotic, Greek and Coptic, numbered serially. Nos. 46—48 are reprinted in SB XVI 12342—12344; No. 51 is SB XVIII 13314; Nos. 53, 55 and 57 (+SB XII 11157) are SB XX 15023, 14952 and 15024 respectively. [MTP]
    //

    private CheckListVolume ParseVolume(string tokenText)
    {
        string[] splitParts;

        if (tokenText.Contains(", ed."))
        {
            splitParts = tokenText.Split(", ed.");
        }
        else
        {
            splitParts = tokenText.Split('.');
        }
        
        var volumeTitle = splitParts[0];


        string date = null;
        string? nos = null;
        List<string> other = new List<string>();
        for (int i = 1; i < splitParts.Length; i++)
        {
            other.Add(splitParts[i]);
        }
        
        return new CheckListVolume(volumeTitle,date,nos,other.ToArray(),tokenText);
    }

    
//= _Hellénisme dans l'Égypte du VIe siècle. La bibliothèque et l'oeuvre de Dioscore d'Aphrodité_, ed. J.-L. Fournet. Cairo 1999. (MIFAO 115). Nos. 1-51. [SEVPO]
//_The Antinoopolis Papyri_. London. <ddb:p.ant>
//_The Archive of Ammon Scholasticus of Panopolis_. <ddb:p.ammon>
    
    private CheckListJournal ParseJournal(string tokenText)
    {

        string[] splits;

        if (tokenText.Contains("_"))
        {
            splits = 
                tokenText.Split('_');
        }else if (tokenText.Contains(":"))
        {
            
            splits = 
                tokenText.Split(":");
        }else if (tokenText.Contains(", ed."))
        {
            
            splits = 
                tokenText.Split(", ed.");
        }
        else
        {
            splits = new[] {tokenText};

        }
        
        var journalTitle = splits[0];
        string? editor = null;
        string? location = null;
        List<string> other = new List<string>();
        for (int i = 1; i < splits.Length; i++)
        {
            other.Add(splits[i]);
        }
        
        return new CheckListJournal(journalTitle, editor,location,other.ToArray(),tokenText);
    }

    private CheckListHeader ParseHeader(string tokenText)
    {
        //Okay this is a bit confusing to look at. But consider the following line: ### <a id="BKT">BKT</a>
        //If we follow the split, first it should split into three parts: [### <a id="BKT"] [BKT</a] [] 
        //Then we take part 1 (indexed from 0) and split it again [BKT] [/a]
        //We take the part 0 of that, and we have the text located in that header.
        var header = "";
        if (tokenText.Contains("<") && tokenText.Contains(">"))
            header = tokenText.Split('>')[1].Split('<')[0];
        else header = tokenText.Split(".")[0];
        return new CheckListHeader(header, tokenText);
    }

    public void ParseTokenizedCheckList(List<CheckListToken> tokenList)
    {
        var oldTokens = TokenList;
        TokenList = tokenList;
        ParseTokenizedCheckList();
        TokenList = oldTokens;
    }

    
    //sets the internal tokenList itself
    public void TokenizeCheckList()
    {
        TokenList = ReturnTokenizeCheckList();
    }
        
        //Returns a token list
    public List<CheckListToken> ReturnTokenizeCheckList()
    {
        var tokens = new List<CheckListToken>();

        bool HeaderFound = false;
        
        var startOfPapyriInfoIndex = GetPapyriInfoIndex();
        //go line by line through the file
        for (; startOfPapyriInfoIndex < CheckListLines.Length; startOfPapyriInfoIndex++)
        {
            var lineType = DetermineLineType(startOfPapyriInfoIndex);
            if(HeaderFound) tokens.Add(lineType);
            if (!HeaderFound && lineType.TokenType == ChecklistTokenType.Header)
            {
                HeaderFound = true;
                tokens.Add(lineType);
            }
        }
        
        return tokens;
    }

    private CheckListToken DetermineLineType(int startOfPapyriInfoIndex)
    {

        var line = CheckListLines[startOfPapyriInfoIndex];

        //Strip out any of the possible headers which might get caught before the following real one.
        if(line.StartsWith("####")) 
            return new CheckListToken(ChecklistTokenType.Other, line);
        if(line.StartsWith("#####"))
            return new CheckListToken(ChecklistTokenType.Other, line);
        
        if (line.StartsWith("###"))
        {
            return new CheckListToken(ChecklistTokenType.Header, line.Remove(0,4));
        }
        
        if (line.StartsWith(" ="))
        {
            //This strips the first _ from the text, allowing the parser later to split on the remianing underscore
            //which are used in markdown to make the title italics.
            return new CheckListToken(ChecklistTokenType.Journal, line.Remove(0,4));
        }
        
        if (line.StartsWith(" *"))
        {
            return new CheckListToken(ChecklistTokenType.Volume, line.Remove(0,3));
        }
        
        return new CheckListToken(ChecklistTokenType.Other, line);
    }
}

record ParsedCheckListItem(string FullText);
record CheckListHeader (string Header, string FullText):ParsedCheckListItem (FullText);

record CheckListJournal (string JournalTitle, string? Editor, string? Location, string[] OtherInfo,  string FullText):ParsedCheckListItem (FullText);


record CheckListVolume (string VolumeTitle, string? Date, string? Nos, string[] OtherInfo, string FullText):ParsedCheckListItem (FullText);