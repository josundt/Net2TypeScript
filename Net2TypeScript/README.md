Net2TypeScript
==============

*- A .Net CLR type to TypeScript generator*
------------------------------------------------

**Reads all public class types (and enums) from an .NET CLR assembly
from the configured root namespace, and generates
corresponding TypeScript interface declarations into a single file.**

Options:
----------------

**Options will be read from:**

**1) The 'settings.json' file**

*PS! The schema file for settings.json will provide further documentation, intellisense and validation 
for the TypeScript generation options if editing in an editor that supports JSON schemas (f.ex. Visual Studio).*

**2) Command line aruments**

Can be used to override options from settings.json for the simple settings types, typically useful for path specific options. 
Use from command line by using double dash arguments where arument names correspond with settings.json properties:
```bash
Net2TypeScript --assemblyPath "c:\MyAssembly.dll" --declarationsOutputPath "c:\out\model.d.ts" --modelModuleOutputPath "c:\out\model.ts"
```

