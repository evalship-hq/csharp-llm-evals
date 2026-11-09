using Anthropic;
using Anthropic.Models.Messages;

namespace Support;

/// <summary>A web shop's support bot. Build it once: the client holds the HTTP connections.</summary>
public class SupportBot(AnthropicClient client)
{
    static readonly string SystemPrompt = ReadPrompt();

    /// <summary>Replies to one customer message.</summary>
    public async Task<string> Answer(string message)
    {
        var response = await client.Messages.Create(new MessageCreateParams
        {
            Model = "claude-opus-5-5",
            MaxTokens = 1024,
            // Opus 5.5 always thinks; low effort is enough for a short reply.
            OutputConfig = new OutputConfig { Effort = Effort.Low },
            System = SystemPrompt,
            Messages = [new UserMessageParam { Content = message }],
        });
        return string.Concat(response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
    }

    // prompts/support.md is embedded in the assembly, so the bot finds it from any working directory.
    static string ReadPrompt()
    {
        using var stream = typeof(SupportBot).Assembly.GetManifestResourceStream("support.md")!;
        return new StreamReader(stream).ReadToEnd();
    }
}
