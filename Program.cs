//
// Copyright(C) 2020 Tonci Jukic
// 
// MIT License: http://opensource.org/licenses/MIT
//
// Note: using NPOI library to generate Excel documents, but a more permanent solution is to be found.
//

using System.Reflection;
using System.Text.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SteamDataModel;

using Author = SteamDataModel.Author;

namespace SteamReviewGetter;

internal static class Program
{
    private static async Task<int> Main()
    {
        if (Environment.GetCommandLineArgs().Length < 2)
        {
            Console.WriteLine(@$"{Environment.NewLine}Missing game Steam App ID as parameter.{Environment.NewLine}{Environment.NewLine}Sample usage:{Environment.NewLine}.\srg.exe 2023810{Environment.NewLine}Where 2023810 is a sample Steam app ID.{Environment.NewLine}{Environment.NewLine}Please try again.");
            return 0;
        }
        
        ReviewFetcher c = new ReviewFetcher(Environment.GetCommandLineArgs()[1]);
        Console.WriteLine("Steam Review reader.");

        //ReviewFetcher c = new ReviewFetcher("1042490"); //large
        //ReviewFetcher c = new ReviewFetcher("2023810"); //small
        //ReviewFetcher c = new ReviewFetcher("1919450"); //empty

        Console.Write($"Fetching {c.GameSteamId}... ");
        using (var progress = new ProgressBar())
        {
            while (c.ActiveFlag)
            {
                await c.PerformQueryNext(progress);
            }
        }

        if (c.FinalDoc.reviews != null)
        {
            Console.WriteLine($"Done. Found and successfully saved {c.FinalDoc.reviews.Count} reviews.\n");

            Console.WriteLine("Writing to files...");
            string appPathMain = @$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\FPSRG";
            if (!Directory.Exists(appPathMain)) Directory.CreateDirectory(appPathMain);

            var j = JsonSerializer.Serialize(c.FinalDoc.reviews);

            try
            {
                await File.WriteAllTextAsync(@$"{appPathMain}\game-{c.GameSteamId}-steamreviews.json", j);
                Console.WriteLine(@$"Done. Saved JSON. ({appPathMain}\game-{c.GameSteamId}-steamreviews.json)");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error writing files. Check permissions? {e}");
            }

            var xlsxpath = @$"{appPathMain}\game-{c.GameSteamId}-steamreviews.xlsx";
            await using (var fs = new FileStream(xlsxpath, FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook = new XSSFWorkbook();

                ISheet excelSheetS = workbook.CreateSheet($"{c.GameSteamId} Summary");

                // Start from 1 to skip the first summary field (it's useful only per page, not in total)
                for (int summaryHeaderIdxItrr = 1;
                     summaryHeaderIdxItrr < typeof(QuerySummary).GetProperties().Length;
                     summaryHeaderIdxItrr++)
                {
                    IRow srow = excelSheetS.CreateRow(summaryHeaderIdxItrr);

                    srow.CreateCell(0)
                        .SetCellValue(
                            $"{(((ReaderInfoAttribute)typeof(QuerySummary).GetProperties()[summaryHeaderIdxItrr].GetCustomAttributes().Single(t => t.GetType() == typeof(ReaderInfoAttribute))).readableName)}");
                    //srow.CreateCell(0).SetCellValue($"{typeof(QuerySummary).GetProperties()[summaryHeaderIdxItrr].Name}");
                    srow.CreateCell(1)
                        .SetCellValue(
                            $"{typeof(QuerySummary).GetProperties()[summaryHeaderIdxItrr].GetValue(c.FinalDoc.query_summary, null)}");
                }

                ISheet excelSheet = workbook.CreateSheet($"{c.GameSteamId} Reviews");

                IRow row = excelSheet.CreateRow(0);
                int reviewColumnCount = typeof(Review).GetProperties().Length;
                for (int reviewHeaderIdxItrr = 0; reviewHeaderIdxItrr < reviewColumnCount; reviewHeaderIdxItrr++)
                {
                    if (reviewHeaderIdxItrr == 1)
                    {
                        row.CreateCell(reviewHeaderIdxItrr).SetCellValue("-");
                        continue;
                    }

                    row.CreateCell(reviewHeaderIdxItrr).SetCellValue(
                        $"{(((ReaderInfoAttribute)typeof(Review).GetProperties()[reviewHeaderIdxItrr].GetCustomAttributes().Single(t => t.GetType() == typeof(ReaderInfoAttribute))).readableName)}");
                    //row.CreateCell(reviewHeaderIdxItrr).SetCellValue($"{(typeof(Review).GetProperties()[reviewHeaderIdxItrr].Name)}");
                }

                for (int authorHeaderIdxItrr = 0;
                     authorHeaderIdxItrr < typeof(Author).GetProperties().Length;
                     authorHeaderIdxItrr++)
                {
                    row.CreateCell(authorHeaderIdxItrr + reviewColumnCount).SetCellValue(
                        $"{(((ReaderInfoAttribute)typeof(Author).GetProperties()[authorHeaderIdxItrr].GetCustomAttributes().Single(t => t.GetType() == typeof(ReaderInfoAttribute))).readableName)}");
                    //row.CreateCell(authorHeaderIdxItrr + reviewColumnCount).SetCellValue($"{typeof(Author).GetProperties()[authorHeaderIdxItrr].Name}");
                }

                for (int reviewEntryItrr = 0; reviewEntryItrr < c.FinalDoc.reviews.Count; reviewEntryItrr++)
                {
                    row = excelSheet.CreateRow(reviewEntryItrr + 1);

                    int rewiewColumntCount = typeof(Review).GetProperties().Length;

                    for (int reviewEntryHeaderItrr = 0;
                         reviewEntryHeaderItrr < rewiewColumntCount;
                         reviewEntryHeaderItrr++)
                    {
                        if (reviewEntryHeaderItrr == 1)
                        {
                            row.CreateCell(reviewEntryHeaderItrr).SetBlank();
                            continue;
                        }
                        
                        // Temp hack
                        //DateTimeOffset.FromUnixTimeSeconds(1660937760).LocalDateTime.ToUniversalTime()
                        if (reviewEntryHeaderItrr == 4 ||reviewEntryHeaderItrr == 5 ||reviewEntryHeaderItrr == 15)
                        {
                            row.CreateCell(reviewEntryHeaderItrr)
                                .SetCellValue($"{DateTimeOffset.FromUnixTimeSeconds((long)(typeof(Review).GetProperties()[reviewEntryHeaderItrr].GetValue(c.FinalDoc.reviews[reviewEntryItrr], null))).LocalDateTime.ToUniversalTime()}");
                            continue;
                        }
                        
                        row.CreateCell(reviewEntryHeaderItrr)
                            .SetCellValue(
                                $"{typeof(Review).GetProperties()[reviewEntryHeaderItrr].GetValue(c.FinalDoc.reviews[reviewEntryItrr], null)}");
                    }

                    for (int reviewEntryAuthorFieldItrr = 0;
                         reviewEntryAuthorFieldItrr < typeof(Author).GetProperties().Length;
                         reviewEntryAuthorFieldItrr++)
                    {
                        // Temp hack
                        //DateTimeOffset.FromUnixTimeSeconds(1660937760).LocalDateTime.ToUniversalTime()
                        if (reviewEntryAuthorFieldItrr == 6)
                        {
                            row.CreateCell(rewiewColumntCount + reviewEntryAuthorFieldItrr)
                                .SetCellValue($"{DateTimeOffset.FromUnixTimeSeconds((long)(typeof(Author).GetProperties()[reviewEntryAuthorFieldItrr].GetValue(c.FinalDoc.reviews[reviewEntryItrr].author, null))).LocalDateTime.ToUniversalTime()}");
                            continue;
                        }
                        row.CreateCell(rewiewColumntCount + reviewEntryAuthorFieldItrr)
                            .SetCellValue(
                                $"{typeof(Author).GetProperties()[reviewEntryAuthorFieldItrr].GetValue(c.FinalDoc.reviews[reviewEntryItrr].author, null)}");
                    }
                }

                try
                {
                    workbook.Write(fs);
                    Console.WriteLine(@$"Done. Saved XLSX. ({appPathMain}\game-{c.GameSteamId}-steamreviews.xlsx)");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error writing files. Check permissions? {e}");
                }
            }
        }

        return 0;
    }

    /*public static void WriteLine(string buffer, ConsoleColor foreground = ConsoleColor.DarkGreen, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        Console.ForegroundColor = foreground;
        Console.BackgroundColor = backgroundColor;
        Console.WriteLine(buffer);
        Console.ResetColor();
    }*/
    
}
