using jasMIN.Net2TypeScript.Utils;
using System.Diagnostics;

namespace jasMIN.Net2TypeScript;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
public static class Application
{
    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        int result = 0;

        if (args.Length == 1 && (args[0] == "-h" || args[0] == "--help"))
        {
            WriteHelp();
            return 0;
        }

        Console.Write("net2typescript: Generating TypeScript models from .NET models...");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            var globalSettings = SettingsBuilder.BuildGlobalSettings(args);

            var generator = new Generator(globalSettings);

            generator.GenerateTypeScript(
                () => File.Create(globalSettings.OutputPath)
            );

            var duration = stopwatch.Elapsed;

            Console.WriteLine($"done in {Math.Floor(duration.TotalSeconds)}.{duration.Milliseconds} seconds!");
            Console.WriteLine($"net2typescript: Output file: {globalSettings.OutputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine();

            Console.WriteLine("net2typescript: ERROR: " + ex.Message);
#if DEBUG
            Console.WriteLine(ex.StackTrace);
#endif

            WriteHelp();

            result = -1;
        }
        finally
        {
            stopwatch.Stop();
        }
#pragma warning restore CA1031 // Do not catch general exception types

        return result;
    }

    private static void WriteHelp()
    {
        Console.WriteLine();
        Console.WriteLine(@"net2typescript usage:");
        Console.WriteLine();
        Console.WriteLine(@"-s|--settings       Path to JSON settings file that uses the settings ""$schema"" (see");
        Console.WriteLine(@"                    schema URL below). If argument is omitted, the settings file is expected");
        Console.WriteLine(@"                    to be found in the same folder as the Net2TypeScript executable with the");
        Console.WriteLine(@"                    file name ""settings.json"".");
        Console.WriteLine();
        Console.WriteLine(@"-c|--configuration  The build configuration (Release/Debug etc).");
        Console.WriteLine(@"                    Emitting this parameter, enables interpolation of $(BuildConfiguration)");
        Console.WriteLine(@"                    variables in the JSON settings file's path properties.");
        Console.WriteLine(@"                    This is particularily useful when you want to pick assembly files from");
        Console.WriteLine(@"                    different folders dynamically for different build configurations.");
        Console.WriteLine(@"                    Defaults to Debug when argument is omitted.");
        Console.WriteLine();
        Console.WriteLine(@"--*                 All global properties (of ""primitive"" value types) in the settings file");
        Console.WriteLine(@"                    schema can be set or overriden using command line arguments as an ");
        Console.WriteLine(@"                    alternative to using the settings file.");
        Console.WriteLine();
        Console.WriteLine(@"-h|--help           Show this help information.");
        Console.WriteLine();
        Console.WriteLine(@"JSON schema for settings file:");
        Console.WriteLine(@"https://raw.githubusercontent.com/josundt/Net2TypeScript/master/Net2TypeScript/settings.schema.json");
        Console.WriteLine();

    }
}
