using LSchecker;
using LSchecker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Text;

Console.OutputEncoding = Encoding.UTF8;
//read configuration
var Configuration = new ConfigurationBuilder()
   .SetBasePath(Path.Combine(AppContext.BaseDirectory))
   .AddJsonFile("appsettings.json", optional: true).Build();
string connectionString = Configuration.GetConnectionString("DefaultConnection");
// setup DI
var services = new ServiceCollection()
    .AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Information))
    .AddSingleton<IConfiguration>(Configuration)
    .AddSingleton<ICaller, Caller>()
    .AddDbContext<ApplicationContext>(options => options.UseSqlite(connectionString))
    .AddTransient<ILookupRunner, LookupRunner>()
    .AddTransient<IDataAnalyse, DataAnalyse>()
    .BuildServiceProvider();
// actual program
//todo add flags for timed usage and for weekly report (db cleaning)

if (args.Length > 0 && args[0] == "-report")
{
    var reporter = services.GetService<IDataAnalyse>();
    if (reporter != null)
        reporter.prepareReport();
}
else
{
    var runner = services.GetService<ILookupRunner>();
    if (runner != null)
        await runner.runLookupListCheckAsync();
}