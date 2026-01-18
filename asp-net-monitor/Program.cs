using FeatBit.Sdk.Server.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add OpenAPI support (.NET 10 best practice)
builder.Services.AddOpenApi();

// Configure FeatBit with dependency injection
builder.Services.AddFeatBit(options =>
{
    options.EnvSecret = builder.Configuration["FeatBit:EnvSecret"] ?? "your-env-secret";
    options.StreamingUri = new Uri(builder.Configuration["FeatBit:StreamingUri"] ?? "wss://app-eval.featbit.co");
    options.EventUri = new Uri(builder.Configuration["FeatBit:EventUri"] ?? "https://app-eval.featbit.co");
    options.StartWaitTime = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("FeatBit:StartWaitTimeSeconds", 3));
    options.DisableEvents = builder.Configuration.GetValue<bool>("FeatBit:DisableEvents", false);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();
