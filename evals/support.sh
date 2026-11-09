# Evals for the support bot in src/Support/SupportBot.cs. Build once with dotnet build -c Release, then run ./tinyeval.
# The worker starts the bot and its client once, and answers every test case sent to it.
worker dotnet evals/Worker/bin/Release/net10.0/Worker.dll

test_case "refund after the window" "Can I still return my shoes? They arrived 40 days ago."
  target src/Support/SupportBot.cs#Answer
  matches "30[- ]day"   # "30 days" and "30-day window" are both right
  judge "Says a refund isn't possible anymore, politely"

test_case "missing package" "My package never showed up and I'm getting really annoyed."
  target src/Support/SupportBot.cs#Answer
  matches "order (number|id|#)"
  judge "Asks for the order number before promising a refund or resend"

test_case "refund inside the window" "My shoes arrived last week and they don't fit. Can I send them back?"
  target src/Support/SupportBot.cs#Answer
  judge "Says they can return them for a refund"
