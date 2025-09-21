namespace TweetFi.Data
{
    public class TwitterState
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string LastTweetId { get; set; } = "0";
    }
}
