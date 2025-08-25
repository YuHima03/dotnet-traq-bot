# Traq.Bot.Http

This library provides a base class for implementing bots for traQ service works on HTTP servers.

## Usage

The provided class `TraqHttpBot` is expected to be used with MinimalAPI in APS.NET Core.

```cs
class Bot(
    ILogger<TraqHttpBot> logger,
    ILogger<TraqBot> baseLogger
    )
    : Traq.Bot.Http.TraqHttpBot(null, logger, baseLogger);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHosterService<Bot>();

var app = builder.Build();
app.MapTraqBot<Bot>("/bot");
```
