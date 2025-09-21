using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TweetFi.Services;

namespace TweetFi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TwitterController : ControllerBase
    {
        private readonly TwitterServiceV2 _twitterService;

        public TwitterController(TwitterServiceV2 twitterService)
        {
            _twitterService = twitterService;
        }

        // GET: api/twitter/latest/{username}
        [HttpGet("latest/{username}")]
        public async Task<IActionResult> GetLatestTweet(string username)
        {
            var tweet = await _twitterService.GetLatestTweetAsync(username);
            if (tweet == null)
                return NotFound(new { message = $"Nenhum tweet encontrado para @{username}" });

            return Ok(tweet);
        }

        // POST: api/twitter/like/{tweetId}
        [HttpPost("like/{tweetId}")]
        public async Task<IActionResult> LikeTweet(string tweetId)
        {
            var success = await _twitterService.LikeTweetAsync(tweetId);
            return success ? Ok(new { message = "Tweet curtido com sucesso!" }) 
                           : BadRequest(new { message = "Falha ao curtir o tweet." });
        }

        // POST: api/twitter/retweet/{tweetId}
        [HttpPost("retweet/{tweetId}")]
        public async Task<IActionResult> Retweet(string tweetId)
        {
            var success = await _twitterService.RetweetAsync(tweetId);
            return success ? Ok(new { message = "Retweet feito com sucesso!" })
                           : BadRequest(new { message = "Falha ao retweetar." });
        }

        // POST: api/twitter/reply/{tweetId}
        [HttpPost("reply/{tweetId}")]
        public async Task<IActionResult> Reply(string tweetId, [FromBody] string message)
        {
            var success = await _twitterService.ReplyTweetAsync(tweetId, message);
            return success ? Ok(new { message = "Resposta enviada com sucesso!" })
                           : BadRequest(new { message = "Falha ao responder o tweet." });
        }
    }
}
