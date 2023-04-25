using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.TypeScriptModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace jasMIN.Net2TypeScript.Tests;

[TestClass]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
public class TypeScriptTypeTests
{
    [TestMethod]
    public void TypeScriptTypeTests_NumberTypes()
    {
        var dotNetTypes = new List<Type> {
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(float),
                typeof(double)
            };

        var settings = new Settings { };

        var tsTypes = dotNetTypes.Select(nt => TypeScriptType.FromDotNetType(nt, null, null, settings)).ToList();

        foreach (var tsType in tsTypes)
        {
            tsType.AssertEquals(new TsTypeAssertion
            {
                IsNullable = false,
                IsGeneric = false,
                IsKnockoutObservable = false,
                TypeName = "number",
                ToStringResult = "number"
            });
        }
    }

    [TestMethod]
    public void TypeScriptTypeTests_NullableNumberTypes()
    {
        var dotNetTypes = new List<Type> {
                typeof(short?),
                typeof(int?),
                typeof(long?),
                typeof(decimal?),
                typeof(float?),
                typeof(double?)
            };


        var settings = new Settings { };

        var tsTypes = dotNetTypes.Select(nt => TypeScriptType.FromDotNetType(nt, null, null, settings)).ToList();

        foreach (var tsType in tsTypes)
        {
            tsType.AssertEquals(new TsTypeAssertion
            {
                IsNullable = true,
                IsGeneric = false,
                IsKnockoutObservable = false,
                TypeName = "number",
                ToStringResult = "number | null"
            });
        }
    }

    [TestMethod]
    public void TypeScriptTypeTests_String()
    {
        var settings = new Settings { };

        var tsType = TypeScriptType.FromDotNetType(typeof(string), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_Boolean()
    {
        var settings = new Settings { };

        var tsType = TypeScriptType.FromDotNetType(typeof(bool), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "boolean",
            ToStringResult = "boolean"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_NullableBoolean()
    {
        var settings = new Settings { };

        var tsType = TypeScriptType.FromDotNetType(typeof(bool?), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "boolean",
            ToStringResult = "boolean | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_DateTypes()
    {
        var dotNetTypes = new List<Type> {
                typeof(DateTime),
                typeof(DateTimeOffset)
            };

        var settings = new Settings { };

        var tsTypes = dotNetTypes.Select(nt => TypeScriptType.FromDotNetType(nt, null, null, settings)).ToList();

        foreach (var tsType in tsTypes)
        {
            tsType.AssertEquals(new TsTypeAssertion
            {
                IsNullable = false,
                IsGeneric = false,
                IsKnockoutObservable = false,
                TypeName = "Date",
                ToStringResult = "Date"
            });
        }
    }

    [TestMethod]
    public void TypeScriptTypeTests_NullableDateTypes()
    {
        var dotNetTypes = new List<Type> {
                typeof(DateTime?),
                typeof(DateTimeOffset?)
            };

        var settings = new Settings { };

        var tsTypes = dotNetTypes.Select(nt => TypeScriptType.FromDotNetType(nt, null, null, settings)).ToList();

        foreach (var tsType in tsTypes)
        {
            tsType.AssertEquals(new TsTypeAssertion
            {
                IsNullable = true,
                IsGeneric = false,
                IsKnockoutObservable = false,
                TypeName = "Date",
                ToStringResult = "Date | null"
            });
        }
    }

    [TestMethod]
    public void TypeScriptTypeTests_Enum()
    {
        var settings = new Settings
        {
            DotNetRootNamespace = "System"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(System.ComponentModel.BindableSupport), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "ComponentModel.BindableSupport",
            ToStringResult = "ComponentModel.BindableSupport"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_NullableEnum()
    {
        var settings = new Settings
        {
            DotNetRootNamespace = "System"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(System.ComponentModel.BindableSupport?), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "ComponentModel.BindableSupport",
            ToStringResult = "ComponentModel.BindableSupport | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_Enum_ExternalNamespace()
    {
        var settings = new Settings
        {
            DotNetRootNamespace = "System.ComponentModel"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(System.ComponentModel.BindableSupport?), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "object",
            ToStringResult = "object | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_Timespan()
    {
        var settings = new Settings
        {
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(TimeSpan), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_NullableTimespan()
    {
        var settings = new Settings
        {
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(TimeSpan?), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_ListOfStrings_DefaultSettings()
    {
        var settings = new Settings
        {
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(List<string>), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Array",
            ToStringResult = "Array<string | null> | null"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_ListOfStrings_NonNullableArraySetting()
    {
        var settings = new Settings
        {
            NonNullableArrays = true
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(List<string>), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Array",
            ToStringResult = "Array<string | null>"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_ListOfClasses_NonNullableArrayEntityItemsSetting()
    {
        var settings = new Settings
        {
            NonNullableArrayEntityItems = true,
            DotNetRootNamespace = "jasMIN.Net2TypeScript"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(List<TsTypeAssertion>), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Array",
            ToStringResult = "Array<Tests.TsTypeAssertion> | null"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "Tests.TsTypeAssertion",
            ToStringResult = "Tests.TsTypeAssertion"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_ListOfClasses_NonNullableArraySettingsAndNonNullableArrayEntityItemsSettings()
    {
        var settings = new Settings
        {
            NonNullableArrays = true,
            NonNullableArrayEntityItems = true,
            DotNetRootNamespace = "jasMIN.Net2TypeScript"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(List<TsTypeAssertion>), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Array",
            ToStringResult = "Array<Tests.TsTypeAssertion>"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "Tests.TsTypeAssertion",
            ToStringResult = "Tests.TsTypeAssertion"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_StringArray()
    {
        var settings = new Settings
        {
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(string[]), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Array",
            ToStringResult = "Array<string | null> | null"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_DictionaryOfStringString_DefaultSettings()
    {
        var settings = new Settings
        {
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(Dictionary<string, string>), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Record",
            ToStringResult = "Record<string, string | null> | null"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string"
        });

        var genericTsArg2 = tsType.GenericTypeArguments[1];

        genericTsArg2.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_IDictionaryOfStringString_NonNullableDictionariesSetting()
    {
        var settings = new Settings
        {
            NonNullableDictionaries = true
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(IDictionary<string, string>), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Record",
            ToStringResult = "Record<string, string | null>"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string"
        });

        var genericTsArg2 = tsType.GenericTypeArguments[1];

        genericTsArg2.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_DictionaryOfStringClass_NonNullableDictionaryEntityValuesSetting()
    {
        var settings = new Settings
        {
            NonNullableDictionaryEntityValues = true,
            DotNetRootNamespace = "jasMIN.Net2TypeScript"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(Dictionary<string, TsTypeAssertion>), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Record",
            ToStringResult = "Record<string, Tests.TsTypeAssertion> | null"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string"
        });

        var genericTsArg2 = tsType.GenericTypeArguments[1];

        genericTsArg2.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "Tests.TsTypeAssertion",
            ToStringResult = "Tests.TsTypeAssertion"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_IReadOnlyDictionaryOfStringClass_NonNullableDictionariesAndNonNullableDictionaryEntityValuesSetting()
    {
        var settings = new Settings
        {
            NonNullableDictionaries = true,
            NonNullableDictionaryEntityValues = true,
            DotNetRootNamespace = "jasMIN.Net2TypeScript"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(IReadOnlyDictionary<string, TsTypeAssertion>), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Record",
            ToStringResult = "Record<string, Tests.TsTypeAssertion>"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string"
        });

        var genericTsArg2 = tsType.GenericTypeArguments[1];

        genericTsArg2.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "Tests.TsTypeAssertion",
            ToStringResult = "Tests.TsTypeAssertion"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_Class()
    {
        var settings = new Settings
        {
            DotNetRootNamespace = "jasMIN.Net2TypeScript"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(TsTypeAssertion), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "Tests.TsTypeAssertion",
            ToStringResult = "Tests.TsTypeAssertion | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_Class_NonNullableEntitiesSetting()
    {
        var settings = new Settings
        {
            NonNullableEntities = true,
            DotNetRootNamespace = "jasMIN.Net2TypeScript"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(TsTypeAssertion), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "Tests.TsTypeAssertion",
            ToStringResult = "Tests.TsTypeAssertion"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_Class_ExternalNamespace()
    {
        var settings = new Settings
        {
            DotNetRootNamespace = "jasMIN.Net2TypeScript.Tests"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(TsTypeAssertion), null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "object",
            ToStringResult = "object | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_KnockoutObservableString()
    {
        var settings = new Settings
        {
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(string), null, null, settings, true);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = true,
            IsKnockoutObservable = true,
            TypeName = "KnockoutObservable",
            ToStringResult = "KnockoutObservable<string | null>"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });

    }

    [TestMethod]
    public void TypeScriptTypeTests_KnockoutObservableListOfStrings()
    {
        var settings = new Settings
        {
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(List<string>), null, null, settings, true);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = true,
            IsKnockoutObservable = true,
            TypeName = "KnockoutObservableArray",
            ToStringResult = "KnockoutObservableArray<string | null> | null"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_KnockoutObservableListOfStrings_NonNullableArraySetting()
    {
        var settings = new Settings
        {
            NonNullableArrays = true
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(List<string>), null, null, settings, true);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = true,
            IsKnockoutObservable = true,
            TypeName = "KnockoutObservableArray",
            ToStringResult = "KnockoutObservableArray<string | null>"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_KnockoutObservableListOfClasses_NonNullableArrayEntityItemsSetting()
    {
        var settings = new Settings
        {
            NonNullableArrayEntityItems = true,
            DotNetRootNamespace = "jasMIN.Net2TypeScript"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(List<TsTypeAssertion>), null, null, settings, true);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = true,
            IsKnockoutObservable = true,
            TypeName = "KnockoutObservableArray",
            ToStringResult = "KnockoutObservableArray<Tests.TsTypeAssertion> | null"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "Tests.TsTypeAssertion",
            ToStringResult = "Tests.TsTypeAssertion"
        });
    }

    [TestMethod]
    public void TypeScriptTypeTests_KnockoutObservableListOfClasses_NonNullableArraySettingsAndNonNullableArrayEntityItemsSettings()
    {
        var settings = new Settings
        {
            NonNullableArrays = true,
            NonNullableArrayEntityItems = true,
            DotNetRootNamespace = "jasMIN.Net2TypeScript"
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(List<TsTypeAssertion>), null, null, settings, true);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = true,
            IsKnockoutObservable = true,
            TypeName = "KnockoutObservableArray",
            ToStringResult = "KnockoutObservableArray<Tests.TsTypeAssertion>"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = false,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "Tests.TsTypeAssertion",
            ToStringResult = "Tests.TsTypeAssertion"
        });
    }


    [TestMethod]
    public void TypeScriptTypeTests_KnockoutObservableListOfListOfStrings()
    {
        var settings = new Settings
        {
        };

        var tsType = TypeScriptType.FromDotNetType(typeof(List<List<string>>), null, null, settings, true);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = true,
            IsKnockoutObservable = true,
            TypeName = "KnockoutObservableArray",
            ToStringResult = "KnockoutObservableArray<Array<string | null> | null> | null"
        });

        var genericTsArg1 = tsType.GenericTypeArguments[0];

        genericTsArg1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = true,
            IsKnockoutObservable = false,
            TypeName = "Array",
            ToStringResult = "Array<string | null> | null"
        });

        var genericTsArg1_1 = genericTsArg1.GenericTypeArguments[0];

        genericTsArg1_1.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = "string",
            ToStringResult = "string | null"
        });
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "<Pending>")]
    internal sealed class GenericClass<T>
    {
        public T? Item { get; set; }
    }

    [TestMethod]
    public void TypeScriptTypeTests_GenericTypeDefinitionParameterProperty()
    {
        var settings = new Settings
        {
        };

        Type type = typeof(GenericClass<>).GetProperty("Item")!.PropertyType;
        var genericParamName = typeof(GenericClass<>).GetGenericArguments().First().Name;
        var tsType = TypeScriptType.FromDotNetType(type, null, null, settings);

        tsType.AssertEquals(new TsTypeAssertion
        {
            IsNullable = true,
            IsGeneric = false,
            IsKnockoutObservable = false,
            TypeName = genericParamName,
            ToStringResult = $"{genericParamName} | null"
        });

    }

}

internal sealed class TsTypeAssertion
{
    public bool IsNullable { get; set; }
    public bool IsGeneric { get; set; }
    public bool IsKnockoutObservable { get; set; }
    public string? TypeName { get; set; }
    public string? ToStringResult { get; set; }
}

internal static class ITypeScriptTypeExtensions
{
    public static void AssertEquals(this ITypeScriptType tsType, TsTypeAssertion assertion)
    {
        Assert.AreEqual(assertion.IsNullable, tsType.IsNullable, $"Expected IsNullable to be {assertion.IsNullable}");
        Assert.AreEqual(assertion.IsGeneric, tsType.IsGeneric, $"Expected IsGeneric to be {assertion.IsGeneric}");
        Assert.AreEqual(assertion.IsKnockoutObservable, tsType.IsKnockoutObservable, $"Expected IsKnockoutObservable to be {assertion.IsKnockoutObservable}");
        Assert.AreEqual(assertion.TypeName, tsType.TypeName, $"Expected TypeName to be {assertion.TypeName}");
        Assert.AreEqual(assertion.ToStringResult, tsType.ToString(), $"Expected ToStringResult to be {assertion.ToStringResult}");
    }
}
