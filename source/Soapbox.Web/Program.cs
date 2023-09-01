using System.Runtime.CompilerServices;
using Soapbox.Web.Extensions;
using Soapbox.Web.Services;

[assembly: InternalsVisibleTo("Soapbox.Tests")]

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("Config/site.json", false, true);
builder.Services.Configure(builder.Configuration, builder.Environment);

var app = builder.Build();

app.Configure(app.Environment);

app.Run();