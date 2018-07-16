using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using jasMIN.Net2TypeScript.Shared.Model;

namespace jasMIN.Net2TypeScript.Tests
{
    [TestClass]
    public class TypeScriptTypeTests
    {
        [TestMethod]
        public void TypeScriptTypeTests_NumberTypes()
        {
            var clrTypes = new List<Type> {
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(float),
                typeof(double)
            };

            var settings = new Settings {};

            var tsTypes = clrTypes.Select(nt => TypeScriptType.FromClrType(nt, settings)).ToList();

            foreach(var tsType in tsTypes) {
                tsType.AssertEquals(new TsTypeAssertion {
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
            var clrTypes = new List<Type> {
                typeof(short?),
                typeof(int?),
                typeof(long?),
                typeof(decimal?),
                typeof(float?),
                typeof(double?)
            };


            var settings = new Settings { };

            var tsTypes = clrTypes.Select(nt => TypeScriptType.FromClrType(nt, settings)).ToList();

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

            var tsType = TypeScriptType.FromClrType(typeof(string), settings);

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

            var tsType = TypeScriptType.FromClrType(typeof(bool), settings);

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

            var tsType = TypeScriptType.FromClrType(typeof(bool?), settings);

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
            var clrTypes = new List<Type> {
                typeof(DateTime),
                typeof(DateTimeOffset)
            };

            var settings = new Settings { };

            var tsTypes = clrTypes.Select(nt => TypeScriptType.FromClrType(nt, settings)).ToList();

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
            var clrTypes = new List<Type> {
                typeof(DateTime?),
                typeof(DateTimeOffset?)
            };

            var settings = new Settings { };

            var tsTypes = clrTypes.Select(nt => TypeScriptType.FromClrType(nt, settings)).ToList();

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
            var settings = new Settings {
                clrRootNamespace = "System"
            };

            var tsType = TypeScriptType.FromClrType(typeof(System.ComponentModel.BindableSupport), settings);

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
                clrRootNamespace = "System"
            };

            var tsType = TypeScriptType.FromClrType(typeof(System.ComponentModel.BindableSupport?), settings);

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
                clrRootNamespace = "System.ComponentModel"
            };

            var tsType = TypeScriptType.FromClrType(typeof(System.ComponentModel.BindableSupport?), settings);

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
        public void TypeScriptTypeTests_ListOfStrings_DefaultSettings()
        {
            var settings = new Settings {
            };

            var tsType = TypeScriptType.FromClrType(typeof(List<string>), settings);

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
                nonNullableArrays = true
            };

            var tsType = TypeScriptType.FromClrType(typeof(List<string>), settings);

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
                nonNullableArrayEntityItems = true,
                clrRootNamespace = "jasMIN.Net2TypeScript"
            };

            var tsType = TypeScriptType.FromClrType(typeof(List<TsTypeAssertion>), settings);

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
                nonNullableArrays = true,
                nonNullableArrayEntityItems = true,
                clrRootNamespace = "jasMIN.Net2TypeScript"
            };

            var tsType = TypeScriptType.FromClrType(typeof(List<TsTypeAssertion>), settings);

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

            var tsType = TypeScriptType.FromClrType(typeof(string[]), settings);

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
        public void TypeScriptTypeTests_Class()
        {
            var settings = new Settings
            {
                clrRootNamespace = "jasMIN.Net2TypeScript"
            };

            var tsType = TypeScriptType.FromClrType(typeof(TsTypeAssertion), settings);

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
                nonNullableEntities = true,
                clrRootNamespace = "jasMIN.Net2TypeScript"
            };

            var tsType = TypeScriptType.FromClrType(typeof(TsTypeAssertion), settings);

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
                clrRootNamespace = "jasMIN.Net2TypeScript.Tests"
            };

            var tsType = TypeScriptType.FromClrType(typeof(TsTypeAssertion), settings);

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

            var tsType = TypeScriptType.FromClrType(typeof(string), settings, true);

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

            var tsType = TypeScriptType.FromClrType(typeof(List<string>), settings, true);

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
                nonNullableArrays = true
            };

            var tsType = TypeScriptType.FromClrType(typeof(List<string>), settings, true);

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
                nonNullableArrayEntityItems = true,
                clrRootNamespace = "jasMIN.Net2TypeScript"
            };

            var tsType = TypeScriptType.FromClrType(typeof(List<TsTypeAssertion>), settings, true);

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
                nonNullableArrays = true,
                nonNullableArrayEntityItems = true,
                clrRootNamespace = "jasMIN.Net2TypeScript"
            };

            var tsType = TypeScriptType.FromClrType(typeof(List<TsTypeAssertion>), settings, true);

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

            var tsType = TypeScriptType.FromClrType(typeof(List<List<string>>), settings, true);

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

    }

    class TsTypeAssertion
    {
        public bool IsNullable { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsKnockoutObservable { get; set; }
        public string TypeName { get; set; }
        public string ToStringResult { get; set; }
    }

    static class ITypeScriptTypeExtensions
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

}
