using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using jasMIN.Net2TypeScript.Model;

namespace jasMIN.Net2TypeScript
{
    internal class Program
    {
        static int Main(string[] args)
        {
            int result = 0;

            Console.WriteLine("Converting .NET entities to TypeScript interfaces.");

            try
            {
                var settings = TypeScriptGenerator.GetSettingsFromJson();

                MergeCmdArgsWithSettings(args, settings);
                var generatorResult = TypeScriptGenerator.GenerateTypeScript(settings);

                File.WriteAllText(settings.declarationsOutputPath, generatorResult.Declarations, Encoding.UTF8);
                File.WriteAllText(settings.modelModuleOutputPath, generatorResult.EnumModel, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                result = -1;
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

            return result;
        }

        static void MergeCmdArgsWithSettings(string[] args, Settings settings)
        {
            if (args.Length % 2 != 0)
            {
                throw new ArgumentException("Wrong command line arguments.");
            }

            for (int i = 0; i < args.Length; i = i + 2)
            {
                if (!args[i].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException(string.Format("Unknown command line argument: '{0}'", args[i]));
                }

                string settingsProp = args[i].Substring(2);

                PropertyInfo propInfo = typeof(Settings).GetProperties().SingleOrDefault(pi => pi.Name == settingsProp);

                if (propInfo == null)
                {
                    throw new ArgumentException(string.Format("Unknown command line argument: '{0}'", args[i]));
                }
                else
                {
                    if (propInfo.PropertyType == typeof(bool?) || propInfo.PropertyType == typeof(bool))
                    {
                        bool boolValue = false;
                        if (!bool.TryParse(args[i + 1], out boolValue))
                        {
                            throw new ArgumentException(string.Format("Not a boolean value: '{0}'", args[i + 1]));
                        }
                        else
                        {
                            propInfo.SetValue(settings, boolValue);
                        }
                    }
                    else if (propInfo.PropertyType == typeof(string))
                    {
                        propInfo.SetValue(settings, args[i + 1]);
                    }
                }

            }
        }
    }
}
