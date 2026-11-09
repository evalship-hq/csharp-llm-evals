// tinyeval's worker: it reads each input up to a NUL byte on stdin, and writes the reply and a NUL byte.
// The client and the bot are built once per worker, not once per test case. Keep logs off stdout.
using System.Text;
using Anthropic;
using Anthropic.Exceptions;
using Support;

var bot = new SupportBot(new AnthropicClient()); // reads ANTHROPIC_API_KEY
var stdin = new BufferedStream(Console.OpenStandardInput());
var stdout = Console.OpenStandardOutput(); // unbuffered, so each reply goes out at once
var input = new MemoryStream();
for (int b; (b = stdin.ReadByte()) != -1;)
{
    if (b != 0)
    {
        input.WriteByte((byte)b);
        continue;
    }
    var reply = await Reply(bot, Encoding.UTF8.GetString(input.ToArray()));
    stdout.Write(Encoding.UTF8.GetBytes(reply + "\0"));
    input.SetLength(0);
}

// When the cause isn't the app (no key, a rate limit, a server error, a timeout or the network),
// tinyeval counts the run as couldn't run. Any other API error, like a bad model name, is the reply, so the eval fails.
static async Task<string> Reply(SupportBot bot, string input)
{
    try
    {
        return await bot.Answer(input);
    }
    catch (AnthropicApiException e)
    {
        var status = (int)e.StatusCode;
        var reason = $"{e.GetType().Name} {status}";
        return status is 401 or 408 or 429 or >= 500 ? $"#tinyeval couldnt_run {reason}\n" : reason;
    }
    catch (AnthropicIOException e) // the network: its inner exception says "Connection refused" or similar
    {
        return $"#tinyeval couldnt_run {e.InnerException?.Message ?? e.Message}\n";
    }
    catch (TaskCanceledException e) // the client's timeout
    {
        return $"#tinyeval couldnt_run {e.Message}\n";
    }
}
