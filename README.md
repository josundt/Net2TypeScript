# Net2TypeScript #

> .NET tool to generate TypeScript models from POCOs in .NET assemblies

Run dotnet tool:

`dotnet tool run net2typescript`

```
Net2TypeScript arguments:

-s|--settings       Path to JSON settings file that uses the settings "$schema" (see schema URL below).
                    If argument is omitted, the settings file is expected to be found in the same
                    folder as the Net2TypeScript executable with the file name "settings.json".

-c|--configuration  The build configuration (Release/Debug etc).
                    Emitting this parameter, enables interpolation of $(BuildConfiguration) variables in
                    the JSON settings file's path properties.
                    This is particularily useful when you want to pick assembly files from different
                    folders dynamically for different build configurations.
                    Defaults to Debug when argument is omitted.

--*                 All global properties (of "primitive" value types) in the settings file schema can
                    beset or overriden using command line arguments as an alternative to using the
                    settings file.

-h|--help           Show this help information.

 JSON schema for settings file:
 https://raw.githubusercontent.com/josundt/Net2TypeScript/master/Net2TypeScript/settings.schema.json
 ```
