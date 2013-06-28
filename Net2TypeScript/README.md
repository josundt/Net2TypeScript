Net2TypeScript
==============

*- A .Net type to TypeScript interface generator*
------------------------------------------------

**Reads all public class types from an .NET CLR assembly**
**from the configured root namespace, and generates**
**corresponding TypeScript interface declarations into a single file.**

*Options:*
----------
 * **assemblyPath** *(string)*

The full path to the .NET assembly file on the disk

 * **rootNamespace** *(string)*

The namespace to scan - automatically includes all "child" namespaces.

 * **tab** *(string)*

Character(s) to use for tab indentation.

 * **enumType** *(string)*

The enum type representation in TypeScript.
Possible values: "*string*" or "*number*"

(*Remark: When a .NET type instance is passed to JavaScript/typescript,*
*it often has been through a transport format like JSON. Enums are often represented*
*as either a number or a string in the JSON format, and therefore also limits the usage*
*in TypeScript to using the same type*)

 * **camelCase** *(boolean)*

Whether the property names of the CLR type should have a camelCase name conversion.

(*Remark: JSON serializers and client side libraries often offer this to align with the two*
*different naming conventions in the two different languages.*)

* **definitelyTypedRelPath** *(string)*

When options *"useKnockout"* and/or *"useBreeze"* are used , the generated file needs to add
a reference to the DefinitelyTyped files for these libraries. Please provide the relative path
to the DefinitelyTyped folder from the output file.

* **useKnockout** *(boolean)*

When set to true, all properties of types will be written as *KnockoutObservable&lt;T&gt;* instead of *T*,
and all collection properties will be converted to *KnockoutObservableArray&lt;&gt;*

* **useBreeze** *(boolean)*

Should only be used when *useKnockout* is also set to true.
Adds two additional meta properties used by breeze to all entities (*'entityAspect'* and *'entityType'*) 

* **outputPath** *(string)*

The full physical disk path to the TypeScript interface output file.

* **globalExtensions** *(JSON object literal)*

Defines fields that will extend the .NET entity in the resulting TypeScript interface.


Options options:
----------------

**Options will be read from:**

1. The **'settings.json'** file
2. Command line arguments

The command line arguments take precedence over the settings in settings.json.

**'settings.json' format** *(JSON)*

-see *'settings.sample.json'* file

**command line arguments**

-same settings as in settings.json can be used as command line arguments, except options use two preceding dashes ('--')

Example: 
		
	Net2TypeScript --assemblyPath "C:\MyFolder\MyAssembly.dll" --outputPath "C:\MyOutputFolder\model.d.ts"