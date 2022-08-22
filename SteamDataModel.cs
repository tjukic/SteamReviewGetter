namespace SteamDataModel;


public class Author
{
    [ReaderInfo("Author Steam ID")]
    public string? steamid { get; set; }
    [ReaderInfo("Author games owned")]
    public int num_games_owned { get; set; }
    [ReaderInfo("Author # of reviews")]
    public int num_reviews { get; set; }
    [ReaderInfo("Author total playtime")]
    public int playtime_forever { get; set; }
    [ReaderInfo("Author playtime last two weeks")]
    public int playtime_last_two_weeks { get; set; }
    [ReaderInfo("Author playtime at review")]
    public int playtime_at_review { get; set; }
    [ReaderInfo("Author last time played")]
    public long last_played { get; set; }
}

public class QuerySummary
{
    [ReaderInfo("Reviews last page")]
    public int num_reviews { get; set; }
    [ReaderInfo("Review score")]
    public int? review_score { get; set; }
    [ReaderInfo("Review score descr")]
    public string? review_score_desc { get; set; }
    [ReaderInfo("Total positive reviews")]
    public int? total_positive { get; set; }
    [ReaderInfo("Total negative reviews")]
    public int? total_negative { get; set; }
    [ReaderInfo("Total reviews")]
    public int? total_reviews { get; set; }
}

public class Review
{
    [ReaderInfo("Recommendation ID")]
    public string? recommendationid { get; set; }
    [ReaderInfo("Author")]
    public Author? author { get; set; }
    [ReaderInfo("Language")]
    public string? language { get; set; }
    [ReaderInfo("Review")]
    public string? review { get; set; }
    [ReaderInfo("Created on")]
    public long timestamp_created { get; set; }
    [ReaderInfo("Updated on")]
    public long timestamp_updated { get; set; }
    [ReaderInfo("Voted up?")]
    public bool voted_up { get; set; }
    [ReaderInfo("Up votes")]
    public int votes_up { get; set; }
    [ReaderInfo("Funny votes")]
    public int votes_funny { get; set; }
    [ReaderInfo("Score weight")]
    public double weighted_vote_score { get; set; }
    [ReaderInfo("Commented on")]
    public int comment_count { get; set; }
    [ReaderInfo("Purchased on Steam")]
    public bool steam_purchase { get; set; }
    [ReaderInfo("Received for free")]
    public bool received_for_free { get; set; }
    [ReaderInfo("EA review")]
    public bool written_during_early_access { get; set; }
    [ReaderInfo("Got dev response")]
    public string? developer_response { get; set; }
    [ReaderInfo("Dev responded on")]
    public long timestamp_dev_responded { get; set; }
}

public class SteamModelRoot
{
    [ReaderInfo("Success at query")]
    public int success { get; set; }
    [ReaderInfo("Game review summary")]
    public QuerySummary? query_summary { get; set; }
    [ReaderInfo("Reviews list")]
    public List<Review>? reviews { get; set; }
    [ReaderInfo("Last query cursor")]
    public string? cursor { get; set; }
}

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Class | System.AttributeTargets.Struct)]  
public class ReaderInfoAttribute : System.Attribute  
{  
    public string readableName;
    public ReaderInfoAttribute(string readableName)  
    {  
        this.readableName = readableName;
    }  
}  