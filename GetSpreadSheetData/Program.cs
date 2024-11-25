// See https://aka.ms/new-console-template for more information

using System.Net;
using HtmlAgilityPack;
using Microsoft.VisualBasic;

class GetSpreadSheetData
{
    public static void Main()
    {
     Console.WriteLine("Creating Client");
     Console.WriteLine("Client Created");
     RunningCoreListEntry();

    }

    private static async Task RunningCoreListEntry()
    {
        try
        {
            Dictionary<string, List<string>> list = new()
            {
            };
            var text = File.ReadAllLines(Directory.GetCurrentDirectory() + "/input.txt");

            foreach (var item in text)
            {
                var parts = item.Split("	");
                if (!list.Keys.Contains(parts[0]))
                {
                    list.Add(parts[0], new () {parts[1]});
                }
                else
                {
                    list[parts[0]].Add(parts[1]);
                }
            }

            foreach (var item in list)
            {
                var MP3Text = item.Key;
                var itemName = item.Value;
                Console.Write($"\n{MP3Text}: {itemName.First()}");
                GetEntry(MP3Text, itemName);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Finished\nPress enter to exit");
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static async Task RunningCorePersonalEntry()
    {
        while (true)
        {
            Console.Write("\nEnter MP3 Number: ");
            var MP3Text = Console.ReadLine();
            Console.Write("Enter Item Name: ");
            var itemName = Console.ReadLine();
            GetEntry(MP3Text, itemName);
            Console.Write("Press any key to continue");
            Console.ReadKey();
        }
    }

    private static void GetEntry(string? MP3Text, string? itemName)
    {
        var client = new HtmlWeb();
        
        try
        {
            var mp3URL = $"http://www.cedopalmp3.uliege.be/cdp_MP3_display.aspx?numNot={MP3Text}";
            Console.WriteLine(": "+ mp3URL);

            var mp3EntryText = client.Load(mp3URL);
            var node = mp3EntryText.DocumentNode.SelectNodes("//*[@id='HyperLink_Trimegistos']");

            Console.Write("getting trismegistor URL. ");
            var trismegistorURL = node[0].Attributes.Where(x => x.Name == "href").First().Value;

            Console.Write($"retrieved: {trismegistorURL}\nLoading text. ");
            var trismegistorText = client.Load(trismegistorURL);
            Console.Write("Trismegistor Text loaded. Finding Publications\n");

            var publications = trismegistorText.DocumentNode
                .SelectNodes("//div[@id='text-publs']");
            var entry = publications[0].ChildNodes.First(x => x.InnerText.Contains(itemName));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Entry found: " + entry.InnerText + "\n\n\n\n");
                Console.ForegroundColor = ConsoleColor.Gray;
            


        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static void GetEntry(string? MP3Text, List<string?> itemNames)
    {
        var client = new HtmlWeb();
        
        try
        {
            var mp3URL = $"http://www.cedopalmp3.uliege.be/cdp_MP3_display.aspx?numNot={MP3Text}";
            Console.WriteLine(": "+ mp3URL);

            var mp3EntryText = client.Load(mp3URL);
            var node = mp3EntryText.DocumentNode.SelectNodes("//*[@id='HyperLink_Trimegistos']");

            Console.Write("getting trismegistor URL. ");
            var trismegistorURL = node[0].Attributes.Where(x => x.Name == "href").First().Value;

            Console.Write($"retrieved: {trismegistorURL}\nLoading text. ");
            var trismegistorText = client.Load(trismegistorURL);
            Console.Write("Trismegistor Text loaded. Finding Publications\n");

            var publications = trismegistorText.DocumentNode
                .SelectNodes("//div[@id='text-publs']");
            var entriesToPrint = new List<string>();
            foreach (var pub in publications)
            {
                foreach (var entry in pub.ChildNodes.Where(x => x.InnerText.Trim() != "" 
                                                                && x.InnerText.Trim() != "Publications"))
                {
                    foreach (var item in itemNames)
                    {
                        foreach (var piece in item.Split(' '))
                        {
                            if (entry.InnerText.Contains(piece) && !entriesToPrint.Contains(entry.InnerText))
                            {
                                    entriesToPrint.Add(entry.InnerText);
                            }
                        }
                    }
                }
            }

            foreach (var entry in entriesToPrint)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Entry found: " + entry);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            //Good version
            /*
            foreach (var item in itemNames)
            {
                var entry = publications[0].ChildNodes.First(x => x.InnerText.Contains(item));
                Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Entry found: " + entry.InnerText);
                    Console.ForegroundColor = ConsoleColor.Gray;
                
            }
            */


        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}