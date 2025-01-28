using System.Text.RegularExpressions;
using EPPlusSamples;
using OfficeOpenXml;


var archivePattern = new Regex( @"\[Online: archive\.org\]\((.*?)\)");
FileInfo checklistXLSX = FileUtil.GetFileInfo(Directory.GetCurrentDirectory(), "checklist.xlsx");
var checklistMD = File.ReadAllLines(Directory.GetCurrentDirectory() + "/shortList.md").ToList();
checklistMD = checklistMD.Where(x => x != "\n").ToList();
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

int index = 2;

 using (ExcelPackage package = new ExcelPackage(checklistXLSX))
            {
                //Open the first worksheet
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                foreach(var item in checklistMD.Where(x => x.Contains("Online: archive.org")))
                {

                    var archiveLink = archivePattern.Match(item);
                    if (archiveLink.Success)
                    {
                        var linkSplit = archiveLink.Value.Split('(');
                        var link = linkSplit[1];
                        if (linkSplit[1].Contains(")")) link = linkSplit[1].Split(')')[0];
                        if (archiveLink.Success) worksheet.Cells[index, 7].Value = link;
                    }

                    index++;
                }

                package.Save();
            }
     