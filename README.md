# LLM evals for C#

A C# support bot whose AI call is tested on every pull request with [tinyeval](https://tinyeval.com).

## Run it

Clone this repo or use it as a template. You need the .NET 10 SDK.

```sh
export ANTHROPIC_API_KEY=sk-ant-...
dotnet tool restore       # the tinyeval command, once per clone
dotnet build -c Release   # again after each change to the C# code
dotnet tinyeval
```

```
support
  ✓ refund after the window 6.5s
  ✓ missing package 5.9s
  ✓ refund inside the window 7.0s

All 3 evals passed (eval-results.json)
```

## What's here

- `src/Support/SupportBot.cs`: the bot. `Answer(message)` sends the message to Claude with the official Anthropic C# SDK.
- `prompts/support.md`: the system prompt, embedded in the assembly.
- `evals/Worker/Program.cs`: answers each test case with the bot, through the `Tinyeval` package's worker helper.
- `evals/support.sh`: three test cases, each checked by a regex, a judge or both.
- `.config/dotnet-tools.json`: the `tinyeval` command, from the `Tinyeval.Tool` package.
- `.github/workflows/evals.yml`: builds the code and runs the evals on every pull request and push to main.
- `.github/dependabot.yml`: weekly pull requests for new package and action versions.
- `LlmEvals.slnx` and `global.json`: the solution and its .NET SDK version.

## Use it for your app

1. Replace `Answer` with your AI call. Keep the worker, so your app starts once per worker and not once per test case.
2. Write your test cases in `evals/`. The [check lines](https://github.com/evalship-hq/tinyeval#writing-evals) cover regexes, JSON, your own graders and a model judge.

When the API call fails for a reason that isn't your code, such as a missing key, a rate limit or a timeout, `Worker.Run` replies couldn't run. tinyeval counts those test cases apart from the failures. An exception from your own code becomes the reply, so the eval fails. For another cause that isn't your code, throw `CouldntRunException`.

In an ASP.NET Core app, reference your app's project from the worker, the way `Worker.csproj` references `Support.csproj`. Build the service provider on the first input, the way `Program.cs` builds the bot, with the services your AI call needs. Resolve your service from it and call it for each input. `Worker.Run` has pointed `Console.Out` at stderr by then, so console logging stays out of the replies.

## In CI

Add `ANTHROPIC_API_KEY` as a repository secret. The bot and tinyeval's judge both use it. The workflow builds the worker, then runs each test case 3 times. A failing eval fails the job, so it can gate merges. The results upload as the `eval-results` artifact. Without the secret, the eval step is skipped with a notice. Dependabot pull requests get no secrets, so their eval step skips with the same notice.

## Update tinyeval

Dependabot updates `Tinyeval` and `Tinyeval.Tool` together, in one pull request. By hand, run `dotnet tool update Tinyeval.Tool` and `dotnet add evals/Worker package Tinyeval`. Keep the two on the same version.

## Compare pull requests with main

A fresh CI run has no earlier run to compare with. [evalship](https://evalship.com) reads `eval-results.json`, compares each pull request with main, tells flaky evals from real drops and comments on the pull request.

## License

MIT
