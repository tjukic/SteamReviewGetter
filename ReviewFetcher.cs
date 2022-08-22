using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using SteamDataModel;

namespace SteamReviewGetter;

public class ReviewFetcher
{
    public bool ActiveFlag = true;
    public string GameSteamId;
    const string QueryFilter = "recent"; // keep recent for working
    private int QueryPageNum = 100; // max is 100, def. is 20
    private string? MutableCursorString = "*"; // * for the first query, cursor for the rest

    readonly Regex _regex;
    string _regexPattern;

    private SteamModelRoot _responseDoc;
    public SteamModelRoot FinalDoc;
    public ReviewFetcher([NotNull] string gameSteamId)
    {
        GameSteamId = gameSteamId;
        _responseDoc = new SteamModelRoot();
        FinalDoc = new SteamModelRoot();

        _regexPattern = @"(""weighted_vote_score"":)("".*?""),";
        _regex = new Regex(_regexPattern, RegexOptions.Compiled);
    }

    public async Task PerformQueryNext(ProgressBar progress)
    {
        HttpClient httpClient = new HttpClient();
        StringBuilder sb = new StringBuilder();

        sb.AppendFormat($"https://store.steampowered.com/appreviews/{GameSteamId}?json=1&cursor={MutableCursorString}&filter={QueryFilter}&num_per_page={QueryPageNum}");
        
        var streamResult = await httpClient.GetByteArrayAsync(sb.ToString());        
        string s = Encoding.UTF8.GetString(streamResult);

        // We need to fix Valve's incorrect JSON formatting of the 'double' entry; either make stringified doubles a number
        // or make zeroes a string and then reinterpret (much more work)
        /* viz loop: // foreach (Match m in Regex.Matches(s, pattern)) { Console.WriteLine("'{0}' found at index {1}.\nThe new one will be {2}", m.Value, m.Index, m.Groups[1].Value + m.Groups[2].Value.Replace("\"", "")); } */
        string rs = _regex.Replace(s, m => m.Groups[1].Value + m.Groups[2].Value.Replace("\"", "") + ",");

        // JSON should now be properly formatted and fit for deserialization
        try
        {
            var doc = JsonSerializer.Deserialize<SteamModelRoot>(rs);
            
            if (doc != null)
            {
                _responseDoc = doc;
                if(_responseDoc.query_summary?.total_reviews != null) {
                    FinalDoc = _responseDoc;
                }

                if(FinalDoc.reviews is { Count: > 0 } && 
                   _responseDoc.query_summary?.total_reviews == null &&
                   FinalDoc.query_summary?.total_reviews != null)
                {
                    if (_responseDoc.reviews != null)
                    {
                        FinalDoc.reviews.AddRange(_responseDoc.reviews);
                        if (FinalDoc.query_summary.total_reviews == FinalDoc.reviews.Count)
                        {
                            progress.Report(100.0f);
                            ActiveFlag = false;
                        }

                        progress.Report((double)((float)FinalDoc.reviews.Count /
                                                 ((int)FinalDoc.query_summary.total_reviews)));
                    }
                }
                MutableCursorString = HttpUtility.UrlEncode(_responseDoc.cursor);
            }

        } catch (Exception e) { throw new Exception($"JSON failed to deserialize. Details: {e}"); }
    }
}