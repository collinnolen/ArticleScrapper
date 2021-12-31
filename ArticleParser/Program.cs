using CommandLine;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ArticleParser
{
    class Program
    {
        public class Options
        {
            [Option('p', "parse", SetName = "parse", HelpText = "Parse the passed in URL")]
            public string? Parse { get; set; }

            [Option('d', "debug", HelpText = "Enable debug mode.")]
            public bool Debug { get; set; }
        }

        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       SetupSerilog(o.Debug);

                       if (!string.IsNullOrEmpty(o.Parse))
                       {
                           PageParser.Parse(o.Parse);
                       }
                       else
                       {
                           Log.Logger.Information("Command not recognized.");
                       }
                   });
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                Log.Logger.Debug(ex.StackTrace);
            }
        }

        private static void SetupSerilog(bool debug)
        {
            LoggingLevelSwitch lls = new LoggingLevelSwitch()
            {
                MinimumLevel = (debug) ? LogEventLevel.Debug : LogEventLevel.Information
            };

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"),
                    retainedFileCountLimit: 2,
                    rollOnFileSizeLimit: true
                    )
                .MinimumLevel.ControlledBy(lls)
                .CreateLogger();
        }
    }
}