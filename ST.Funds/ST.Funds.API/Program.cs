using Microsoft.EntityFrameworkCore;
using ST.Funds.Application.Config;
using ST.Funds.Application.Services.FundIngestion;
using ST.Funds.Data.DataContext;
using ST.Funds.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// DbContext (InMemory for assessment)
builder.Services.AddDbContext<FundsDbContext>(options =>
    options.UseInMemoryDatabase("FundsDb"));

// Bind FundSources from appsettings.json
builder.Services.Configure<List<FundSourceConfig>>(
    builder.Configuration.GetSection("FundSources"));

// HttpClient + Ingestion Service
builder.Services.AddHttpClient<IFundIngestionService, FundIngestionService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Custom middleware
app.UseMiddleware<ApiLogging>();

app.MapControllers();

// Trigger initial data load on startup
using (var scope = app.Services.CreateScope())
{
    var ingestion = scope.ServiceProvider.GetRequiredService<IFundIngestionService>();
    await ingestion.RefreshFundsAsync();
}

app.Run();
