// tinyeval's worker: Worker.Run answers each test case with the bot. When the cause isn't the app (no key,
// a rate limit, a server error, a timeout or the network), it replies couldn't run. Any other error is the reply,
// so the eval fails.
using Anthropic;
using Support;
using Tinyeval;

SupportBot? bot = null;
// Built once, on the first input, after Worker.Run points Console.Out at stderr.
await Worker.Run(input => (bot ??= new SupportBot(new AnthropicClient())).Answer(input)); // reads ANTHROPIC_API_KEY
