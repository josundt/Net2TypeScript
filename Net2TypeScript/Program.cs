using jasMIN.Net2TypeScript.Utils;

namespace jasMIN.Net2TypeScript;

public static class Application
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public static int Main(string[] args)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        int result = 0;

        if (args.Length == 1 && (args[0] == "-h" || args[0] == "--help"))
        {
            WriteHelp();
            return 0;
        }

        Console.WriteLine("Generating TypeScript models from .NET models...");

        try
        {
            var globalSettings = SettingsBuilder.BuildGlobalSettings(args);

            var generator = new Generator(globalSettings);

            generator.GenerateTypeScript(
                () => File.Create(globalSettings.OutputPath)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
#if DEBUG
            Console.WriteLine(ex.StackTrace);
#endif

            WriteHelp();

            result = -1;
        }

        return result;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    private static void WriteHelp()
    {
        Console.WriteLine();
        Console.WriteLine(@"Net2TypeScript usage:");
        Console.WriteLine();                                                                                                     //                                                                                                    
        Console.WriteLine(@"-s|--settings       Path to JSON settings file that uses the settings ""$schema"" (see schema URL");
        Console.WriteLine(@"                    below). If argument is omitted, the settings file is expected to be found in the");
        Console.WriteLine(@"                    same folder as the Net2TypeScript executable with the file name ""settings.json"".");
        Console.WriteLine();
        Console.WriteLine(@"-c|--configuration  The build configuration (Release/Debug etc).");
        Console.WriteLine(@"                    Emitting this parameter, enables interpolation of $(BuildConfiguration) variables");
        Console.WriteLine(@"                    in the JSON settings file's path properties.");
        Console.WriteLine(@"                    This is particularily useful when you want to pick assembly files from different");
        Console.WriteLine(@"                    folders dynamically for different build configurations.");
        Console.WriteLine(@"                    Defaults to Debug when argument is omitted.");
        Console.WriteLine();
        Console.WriteLine(@"--*                 All global properties (of ""primitive"" value types) in the settings file schema");
        Console.WriteLine(@"                    can be set or overriden using command line arguments as an alternative to using");
        Console.WriteLine(@"                    the settings file.");
        Console.WriteLine();
        Console.WriteLine(@"-h|--help           Show this help information.");
        Console.WriteLine();
        Console.WriteLine(@"JSON schema for settings file:");
        Console.WriteLine(@"https://raw.githubusercontent.com/josundt/Net2TypeScript/master/Net2TypeScript/settings.schema.json");
        Console.WriteLine();

    }
}
