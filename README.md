# Net2TypeScript #

> .NET tool to generate TypeScript interfaces/enums from .NET assembly classes/interfaces/enums

## .NET tool command ##

`dotnet net2typescript`

## Avaliable arguments ##

```
-s|--settings       Path to JSON settings file that uses the settings "$schema" (see
                    schema URL below). If argument is omitted, the settings file is expected
                    to be found in the same folder as the Net2TypeScript executable with the
                    file name "settings.json".

-c|--configuration  The build configuration (Release/Debug etc).
                    Emitting this parameter, enables interpolation of $(BuildConfiguration)
                    variables in the JSON settings file's path properties.
                    This is particularily useful when you want to pick assembly files from
                    different folders dynamically for different build configurations.
                    Defaults to Debug when argument is omitted.

--*                 All global properties (of "primitive" value types) in the settings file
                    schema can be set or overriden using command line arguments as an
                    alternative to using the settings file.

-h|--help           Show this help information.

JSON schema for settings file:
https://raw.githubusercontent.com/josundt/Net2TypeScript/master/Net2TypeScript/settings.schema.json
```

## Sample settings file ##
```json
{
  "$schema": "https://raw.githubusercontent.com/josundt/Net2TypeScript/master/Net2TypeScript/settings.schema.json",
  "assemblyPaths": [
    "./bin/$(BuildConfiguration)/net6.0/My.App.Models.dll",
    "../My.App.ApiModels/bin/$(BuildConfiguration)/netstandard2.0/My.App.ApiModels.dll"
  ],
  "outputPath": "../My.App.Spa/src/models/models.ts",
  "dotNetRootNamespace": "My.App",
  "tsRootNamespace": null,
  "tsFlattenNamespaces": true,
  "classNamespaceFilter": [
    "My.App.Models*",
    "My.App.ApiModels*"
  ],
  "enumNamespaceFilter": [
    "My.App.Models*",
    "My.App.ApiModels*"
  ],
  "indent": "    ",
  "enumType": "stringIfNotFlagEnum",
  "enumFormat": "enum",
  "camelCase": true,
  "excludeClass": false,
  "excludeInterface": true,
  "nullableReferenceTypes": true,
  "namespaceOverrides": {
    "My.App.ApiModels*": {
      "nullableReferenceTypes": false
    }
  },
  "classOverrides": {
    "My.App.ApiModels.PersonLocation": {
      "extraProperties": {
        "location": "import(\"./geography-point.js\").IGeographyPoint"
      }
    }
  }
}
```
