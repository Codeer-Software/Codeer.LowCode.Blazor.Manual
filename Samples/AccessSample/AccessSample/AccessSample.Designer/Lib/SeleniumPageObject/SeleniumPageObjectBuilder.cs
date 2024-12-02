using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AccessSample.Designer.Lib.SeleniumPageObject
{
    internal class SeleniumPageObjectBuilder
    {
        internal string TargetPath { get; set; } = string.Empty;
        internal string Namespace { get; set; } = string.Empty;

        internal void Build(DesignData designData)
        {
            if (string.IsNullOrEmpty(TargetPath))
            {
                Debug.WriteLine("Target path not found.");
                return;
            }

            Directory.CreateDirectory(TargetPath);

            foreach (var pageFrame in designData.PageFrames.ToList())
            {
                GeneratePageFramePageObject(pageFrame);
            }

            foreach (var module in designData.Modules.ToList())
            {
                GeneratePageObject(module, ModuleLayoutType.Detail);
                GeneratePageObject(module, ModuleLayoutType.List);
                GeneratePageObject(module, ModuleLayoutType.Search);
            }
        }

        #region Module PageObject generators

        private void GeneratePageObject(ModuleDesign design, ModuleLayoutType type)
        {
            var source = new SourceGenerator
            {
                UsingNamespaces =
                {
                    "Codeer.LowCode.Blazor.SeleniumDrivers",
                    "OpenQA.Selenium",
                    "Selenium.StandardControls",
                    "Selenium.StandardControls.PageObjectUtility",
                    "Selenium.StandardControls.TestAssistant.GeneratorToolKit",
                },
                Namespace = Namespace,
            };

            switch (type)
            {
                case ModuleLayoutType.Detail:
                    source.Classes.AddRange(GenerateDetailLayoutPageObject(design));
                    break;
                case ModuleLayoutType.List:
                    source.Classes.AddRange(GenerateListLayoutPageObject(design));
                    break;
                case ModuleLayoutType.Search:
                    source.Classes.AddRange(GenerateSearchLayoutPageObject(design));
                    break;
            }

            var path = System.IO.Path.Combine(TargetPath, $"{design.Name}{type}Layout.cs");
            System.IO.File.WriteAllText(path, source.Generate());
        }

        private ClassDecl[] GenerateDetailLayoutPageObject(ModuleDesign design)
        {
            var classes = new List<ClassDecl>();
            foreach (var layout in design.DetailLayouts)
            {
                classes.Add(GenerateDetailPageClassDecl(design, layout.Key, layout.Value.Layout));
            }

            if (design.DetailLayouts.Count == 0)
            {
                classes.Add(GenerateDetailPageClassDecl(design, "", new GridLayoutDesign()));
            }

            classes.Add(new ClassDecl
            {
                Name = $"{design.Name}DetailPage",
                BaseClass = $"DetailPage<{design.Name}DetailLayout>",
                Constructors =
                {
                    new ConstructorDecl
                    {
                        AccessModifier = "public",
                        Parameters = { new ParameterDecl { Type = "IWebDriver", Name = "driver" } },
                        BaseParameters = { "driver" }
                    }
                }
            });

            var identify = HasListPage(design)
                ? $@"UrlCompareType.Contains, ""/{design.Name}/"""
                : $@"UrlCompareType.IgnoreQueryEndsWith, ""/{design.Name}""";

            classes.Add(new ClassDecl
            {
                AccessModifier = "public static",
                Name = $"{design.Name}DetailPageExtensions",
                Methods =
                {
                    new MethodDecl
                    {
                        AccessModifier = "public static",
                        Name = $"Attach{design.Name}DetailPage",
                        ReturnType = $"{design.Name}DetailPage",
                        Attributes = { $@"PageObjectIdentify({identify})" },
                        Parameters = { new ParameterDecl { Type = "this IWebDriver", Name = "driver" } },
                        Statements =
                        {
                            new Statement
                                { Expression = $@"driver.WaitForUrl({identify});" },
                            new Statement { Expression = $"return new {design.Name}DetailPage(driver);" },
                        }
                    },
                    new ExpressionMethodDecl
                    {
                        AccessModifier = "public static",
                        Name = $"Attach{design.Name}Dialog",
                        ReturnType = $"ModuleDialogDriver<{design.Name}DetailLayout>",
                        Attributes = { $@"ComponentObjectIdentify" },
                        Parameters = { new ParameterDecl { Type = "this IWebDriver", Name = "driver" } },
                        Expression =
                            $"new MappingBase(driver).ByCssSelector(\"[data-system='module-dialog'][data-module-design='{design.Name}']\").Wait();"
                    }
                }
            });

            return classes.ToArray();
        }

        private ClassDecl[] GenerateListLayoutPageObject(ModuleDesign design)
        {
            var classes = new List<ClassDecl>();
            foreach (var layout in design.ListLayouts)
            {
                classes.Add(GenerateListPageClassDecl(design, layout.Key, layout.Value));
            }

            if (design.ListLayouts.Count == 0)
            {
                classes.Add(GenerateListPageClassDecl(design, "", new ListLayoutDesign()));
            }

            if (!HasListPage(design)) return classes.ToArray();

            classes.Add(new ClassDecl
            {
                Name = $"{design.Name}ListPage",
                BaseClass = $"ListPage<{design.Name}ListLayout, {design.Name}SearchLayout>",
                Constructors =
                {
                    new ConstructorDecl
                    {
                        AccessModifier = "public",
                        Parameters = { new ParameterDecl { Type = "IWebDriver", Name = "driver" } },
                        BaseParameters = { "driver" }
                    }
                }
            });

            classes.Add(new ClassDecl
            {
                AccessModifier = "public static",
                Name = $"{design.Name}ListPageExtensions",
                Methods =
                {
                    new MethodDecl
                    {
                        AccessModifier = "public static",
                        Name = $"Attach{design.Name}ListPage",
                        ReturnType = $"{design.Name}ListPage",
                        Attributes = { $@"PageObjectIdentify(UrlCompareType.IgnoreQueryEndsWith, ""/{design.Name}"")" },
                        Parameters = { new ParameterDecl { Type = "this IWebDriver", Name = "driver" } },
                        Statements =
                        {
                            new Statement
                            {
                                Expression =
                                    $@"driver.WaitForUrl(UrlCompareType.IgnoreQueryEndsWith, ""/{design.Name}"");"
                            },
                            new Statement { Expression = $"return new {design.Name}ListPage(driver);" },
                        }
                    }
                }
            });

            return classes.ToArray();
        }

        private ClassDecl[] GenerateSearchLayoutPageObject(ModuleDesign design)
        {
            var classes = new List<ClassDecl>();
            foreach (var layout in design.SearchLayouts)
            {
                classes.Add(GenerateSearchLayoutClassDecl(design, layout.Key, layout.Value.Layout));
            }

            if (design.SearchLayouts.Count == 0)
            {
                classes.Add(GenerateSearchLayoutClassDecl(design, "", new GridLayoutDesign()));
            }

            return classes.ToArray();
        }

        private ClassDecl GenerateDetailPageClassDecl(ModuleDesign design, string layoutName, LayoutDesignBase layout)
        {
            var classDecl = new ClassDecl
            {
                Name = $"{design.Name}DetailLayout",
                BaseClass = "ComponentBase",
            };

            if (!string.IsNullOrEmpty(layoutName))
            {
                classDecl.Name += $"_{SafeSignature(layoutName)}";
            }

            foreach (var gridLayout in layout.GetDescendantLayouts<GridLayoutDesign>())
            {
                if (string.IsNullOrEmpty(gridLayout.Name)) continue;
                classDecl.Properties.Add(new ExpressionPropertyDecl
                {
                    Type = "GridDriver",
                    Name = SafeSignature(gridLayout.Name) + "Grid",
                    Expression = $@"ByCssSelector(""div[data-name='{gridLayout.Name}']"").Wait()",
                });
            }

            foreach (var tabLayout in layout.GetDescendantLayouts<TabLayoutDesign>())
            {
                if (string.IsNullOrEmpty(tabLayout.Name)) continue;
                classDecl.Properties.Add(new ExpressionPropertyDecl
                {
                    Type = "TabDriver",
                    Name = SafeSignature(tabLayout.Name) + "Tab",
                    Expression = $@"ByCssSelector(""div[data-name='{tabLayout.Name}']"").Wait()",
                });
            }

            foreach (var field in layout.GetDescendantFields(design))
            {
                if (!HasDriverField(field)) continue;
                var propertyDecl = new ExpressionPropertyDecl
                {
                    Type = AsDriverName(design, field),
                    Name = SafeSignature(field.Name),
                    Expression = $@"ByCssSelector(""div[data-name='{field.Name}']"").Wait()",
                };
                classDecl.Properties.Add(propertyDecl);
            }

            classDecl.Constructors.Add(new ConstructorDecl
            {
                AccessModifier = "public",
                Parameters = { new ParameterDecl { Type = "IWebElement", Name = "element" } },
                BaseParameters = { "element" }
            });

            classDecl.Operators.Add(new ImplicitOperatorDecl
            {
                TargetType = classDecl.Name,
                Parameter = new ParameterDecl { Type = "ElementFinder", Name = "finder" },
                Expression = $"finder.Find<{classDecl.Name}>();",
            });

            return classDecl;
        }

        private ClassDecl GenerateListPageClassDecl(ModuleDesign design, string layoutName, ListLayoutDesign layout)
        {
            var classDecl = new ClassDecl
            {
                Name = $"{design.Name}ListLayout",
                BaseClass = "ListLayoutBase",
            };

            if (!string.IsNullOrEmpty(layoutName))
            {
                classDecl.Name += $"_{SafeSignature(layoutName)}";
            }

            foreach (var field in layout.Elements.SelectMany(row => row)
                         .Where(element => !string.IsNullOrEmpty(element.FieldName))
                         .Select(element => design.Fields.First(field => field.Name == element.FieldName)))
            {
                if (!HasDriverField(field)) continue;
                classDecl.Properties.Add(new ExpressionPropertyDecl
                {
                    Type = AsDriverName(design, field),
                    Name = SafeSignature(field.Name),
                    Expression = $@"ByCssSelector(""td[data-name='{field.Name}']"").Wait()",
                });
            }

            classDecl.Constructors.Add(new ConstructorDecl
            {
                AccessModifier = "public",
                Parameters = { new ParameterDecl { Type = "IWebElement", Name = "element" } },
                BaseParameters = { "element" }
            });

            classDecl.Operators.Add(new ImplicitOperatorDecl
            {
                TargetType = classDecl.Name,
                Parameter = new ParameterDecl { Type = "ElementFinder", Name = "finder" },
                Expression = $"finder.Find<{classDecl.Name}>();",
            });

            return classDecl;
        }

        private ClassDecl GenerateSearchLayoutClassDecl(ModuleDesign design, string layoutName, LayoutDesignBase layout)
        {
            var classDecl = new ClassDecl
            {
                Name = $"{design.Name}SearchLayout",
                BaseClass = "ComponentBase",
            };

            if (!string.IsNullOrEmpty(layoutName))
            {
                classDecl.Name += $"_{SafeSignature(layoutName)}";
            }

            foreach (var gridLayout in layout.GetDescendantLayouts<GridLayoutDesign>())
            {
                if (string.IsNullOrEmpty(gridLayout.Name)) continue;
                classDecl.Properties.Add(new ExpressionPropertyDecl
                {
                    Type = "SearchGridDriver",
                    Name = SafeSignature(gridLayout.Name) + "Grid",
                    Expression = $@"ByCssSelector(""div[data-name='{gridLayout.Name}']"").Wait()",
                });
            }

            foreach (var field in layout.GetDescendantFields(design))
            {
                if (!HasDriverField(field)) continue;
                var propertyDecl = new ExpressionPropertyDecl
                {
                    Type = AsSearchDriverName(design, field),
                    Name = SafeSignature(field.Name),
                    Expression = $@"ByCssSelector(""div[data-name='{field.Name}']"").Wait()",
                };

                classDecl.Properties.Add(propertyDecl);
            }

            classDecl.Constructors.Add(new ConstructorDecl
            {
                AccessModifier = "public",
                Parameters = { new ParameterDecl { Type = "IWebElement", Name = "element" } },
                BaseParameters = { "element" }
            });

            classDecl.Operators.Add(new ImplicitOperatorDecl
            {
                TargetType = classDecl.Name,
                Parameter = new ParameterDecl { Type = "ElementFinder", Name = "finder" },
                Expression = $"finder.Find<{classDecl.Name}>();",
            });

            return classDecl;
        }

        #endregion

        #region PageFrame PageObject generators

        private void GeneratePageFramePageObject(PageFrameDesign design)
        {
            var source = new SourceGenerator
            {
                UsingNamespaces =
                {
                    "Codeer.LowCode.Blazor.SeleniumDrivers",
                    "OpenQA.Selenium",
                    "Selenium.StandardControls",
                    "Selenium.StandardControls.PageObjectUtility",
                    "Selenium.StandardControls.TestAssistant.GeneratorToolKit",
                },
                Namespace = Namespace,
            };


            if (design.Left.IsVisible)
            {
                source.Classes.AddRange(GenerateSidebarDesignClassDecl(design, "Left"));
            }

            if (design.Right.IsVisible)
            {
                source.Classes.AddRange(GenerateSidebarDesignClassDecl(design, "Right"));
            }

            if (design.Header.IsVisible)
            {
                source.Classes.AddRange(GenerateHeaderDesignClassDecl(design));
            }

            var path = System.IO.Path.Combine(TargetPath, $"{design.Name}PageFrame.cs");
            System.IO.File.WriteAllText(path, source.Generate());
        }

        private ClassDecl[] GenerateSidebarDesignClassDecl(PageFrameDesign design, string side)
        {
            var classes = new List<ClassDecl>
            {
                GeneratePageFrameClassDecl(design.Name, side, side switch
                {
                    "Left" => design.Left.Links,
                    "Right" => design.Right.Links,
                    _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
                }),
                new()
                {
                    AccessModifier = "public static",
                    Name = $"{design.Name}{side}Extensions",
                    Methods =
                    {
                        new ExpressionMethodDecl
                        {
                            Attributes = { "ComponentObjectIdentify" },
                            AccessModifier = "public static",
                            Name = $"Attach{design.Name}{side}",
                            ReturnType = $"{design.Name}{side}",
                            Parameters = { new ParameterDecl { Type = "this IWebDriver", Name = "driver" } },
                            Expression =
                                $"new MappingBase(driver).ByCssSelector(\"[data-system='sidebar'][data-system-placement='{side.ToLower()}']\").Wait();",
                        }
                    }
                }
            };

            return classes.ToArray();
        }

        private ClassDecl[] GenerateHeaderDesignClassDecl(PageFrameDesign design)
        {
            var classes = new List<ClassDecl>
            {
                GeneratePageFrameClassDecl(design.Name, "Header", design.Header.Links),
                new()
                {
                    AccessModifier = "public static",
                    Name = $"{design.Name}HeaderExtensions",
                    Methods =
                    {
                        new ExpressionMethodDecl
                        {
                            Attributes = { "ComponentObjectIdentify" },
                            AccessModifier = "public static",
                            Name = $"Attach{design.Name}Header",
                            ReturnType = $"{design.Name}Header",
                            Parameters = { new ParameterDecl { Type = "this IWebDriver", Name = "driver" } },
                            Expression =
                                $"new MappingBase(driver).ByCssSelector(\"[data-system='topbar']\").Wait();",
                        }
                    }
                }
            };

            return classes.ToArray();
        }

        private ClassDecl GeneratePageFrameClassDecl(string designName, string placement, List<PageLink> items)
        {
            var mainClass = new ClassDecl
            {
                AccessModifier = "public",
                Name = $"{designName}{placement}",
                BaseClass = "ComponentBase",
                Constructors =
                {
                    new ConstructorDecl
                    {
                        Parameters = { new ParameterDecl { Type = "IWebElement", Name = "element" } },
                        BaseParameters = { "element" }
                    }
                },
                Operators =
                {
                    new ImplicitOperatorDecl
                    {
                        AccessModifier = "public",
                        TargetType = $"{designName}{placement}",
                        Parameter = new ParameterDecl { Type = "ElementFinder", Name = "finder" },
                        Expression = $"finder.Find<{designName}{placement}>();"
                    }
                }
            };

            mainClass.Properties.Add(new ExpressionPropertyDecl
            {
                Type = "AnchorDriver",
                Name = "Home",
                Expression = @"ByCssSelector("".navbar-brand"").Wait()",
            });

            var itemsSet = new HashSet<string>();
            foreach (var item in items)
            {
                var segments = item.Title.Split('/').ToList();
                for (var i = 1; i <= segments.Count; ++i)
                {
                    itemsSet.Add(string.Join("/", segments.Take(i)));
                }
            }

            foreach (var item in itemsSet)
            {
                mainClass.Properties.Add(new ExpressionPropertyDecl
                {
                    Type = "AnchorDriver",
                    Name = SafeSignature(item),
                    Expression =
                        $@"ByCssSelector(""[data-title='{item}']"").Wait()",
                });
            }

            return mainClass;
        }

        #endregion

        private static bool HasListPage(ModuleDesign design)
        {
            return design.ListLayouts.TryGetValue(string.Empty, out var listLayout) &&
                   listLayout.Elements.SelectMany(e => e).Select(e => e.FieldName).Any(e => !string.IsNullOrEmpty(e));
        }

        private bool HasDriverField(FieldDesignBase field)
        {
            if (field is MarkupStringFieldDesign) return false;
            if (field is ProCodeFieldDesign) return false;
            return true;
        }

        #region Naming utilities

        private string SafeSignature(string name)
        {
            const string validFirstLetterChars = "\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}";
            const string validChars = validFirstLetterChars + "\\p{Mn}\\p{Mc}\\p{Pc}\\p{Nd}\\p{Cf}";

            var invalidChars = new Regex($"[^{validChars}]");
            var safeSignature = invalidChars.Replace(name, "_");

            return safeSignature;
        }

        private string AsDriverName(ModuleDesign design, FieldDesignBase field)
        {
            return field switch
            {
                ListFieldDesign listFieldDesign => AsDriverName(field.GetType().Name,
                    $"<{AsListLayoutName(listFieldDesign.SearchCondition.ModuleName, listFieldDesign.LayoutName)}>"),
                DetailListFieldDesign detailListFieldDesign => AsDriverName(field.GetType().Name,
                    $"<{AsDetailLayoutName(detailListFieldDesign.SearchCondition.ModuleName, detailListFieldDesign.LayoutName)}>"),
                TileListFieldDesign tileListFieldDesign => AsDriverName(field.GetType().Name,
                    $"<{AsDetailLayoutName(tileListFieldDesign.SearchCondition.ModuleName, tileListFieldDesign.LayoutName)}>"),
                SearchFieldDesign searchFieldDesign => AsDriverName(field.GetType().Name,
                    $"<{AsSearchLayoutName(
                        (design.Fields.First(f => f.Name == searchFieldDesign.ResultsViewFieldName) as ListFieldDesignBase)!.SearchCondition.ModuleName,
                        searchFieldDesign.LayoutName)}>"),
                ModuleFieldDesign moduleFieldDesign => AsDriverName(field.GetType().Name,
                    $"<{AsDetailLayoutName(moduleFieldDesign.ModuleName, moduleFieldDesign.LayoutName)}>"),
                LinkFieldDesign linkFieldDesign => AsDriverName(field.GetType().Name,
                    $"<{AsListLayoutName(linkFieldDesign.SearchCondition.ModuleName, linkFieldDesign.ListLayoutName)}, {AsSearchLayoutName(linkFieldDesign.SearchCondition.ModuleName, linkFieldDesign.SearchLayoutName)}>"),
                _ => CreateSimpleName(field.GetType().Name)
            };

            static string CreateSimpleName(string name)
            {
                var index = name.LastIndexOf("Design");
                var simpleName = $"{name[..index]}Driver";
                return simpleName;
            }
        }

        private string AsDriverName(string name, string typeArg)
        {
            var index = name.LastIndexOf("Design");
            return $"{name[..index]}Driver{typeArg}";
        }

        private string AsSearchDriverName(ModuleDesign design, FieldDesignBase field)
        {
            return field switch
            {
                LinkFieldDesign linkFieldDesign => AsSearchDriverName(field.GetType().Name,
                    $"<{AsListLayoutName(linkFieldDesign.SearchCondition.ModuleName, linkFieldDesign.ListLayoutName)}, {AsSearchLayoutName(linkFieldDesign.SearchCondition.ModuleName, linkFieldDesign.SearchLayoutName)}>"),
                _ => CreateSimpleName(field.GetType().Name)
            };

            static string CreateSimpleName(string name)
            {
                var index = name.LastIndexOf("Design");
                return $"{name[..index]}SearchDriver";
            }
        }

        private string AsSearchDriverName(string name, string typeArg)
        {
            var index = name.LastIndexOf("Design");
            return $"{name[..index]}SearchDriver{typeArg}";
        }

        private string AsDetailLayoutName(string moduleName, string layoutName)
        {
            var name = $"{moduleName}DetailLayout";
            if (!string.IsNullOrEmpty(layoutName))
            {
                name += $"_{SafeSignature(layoutName)}";
            }

            return name;
        }

        private string AsListLayoutName(string moduleName, string layoutName)
        {
            var name = $"{moduleName}ListLayout";
            if (!string.IsNullOrEmpty(layoutName))
            {
                name += $"_{SafeSignature(layoutName)}";
            }

            return name;
        }

        private string AsSearchLayoutName(string moduleName, string layoutName)
        {
            var name = $"{moduleName}SearchLayout";
            if (!string.IsNullOrEmpty(layoutName))
            {
                name += $"_{SafeSignature(layoutName)}";
            }

            return name;
        }

        #endregion
    }
}
