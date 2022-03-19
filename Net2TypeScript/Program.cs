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

        Console.WriteLine("Converting .NET entities to TypeScript interfaces.");

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
            //#if DEBUG
            Console.WriteLine(ex.StackTrace);
            //#endif
            result = -1;
        }

        return result;
    }
}
