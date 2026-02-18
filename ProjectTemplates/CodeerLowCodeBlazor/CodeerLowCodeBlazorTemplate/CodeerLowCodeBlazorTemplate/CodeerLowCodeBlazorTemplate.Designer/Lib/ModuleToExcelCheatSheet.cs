using System.IO;
using ClosedXML.Excel;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;

namespace CodeerLowCodeBlazorTemplate.Designer.Lib
{
    internal class ModuleToExcelCheatSheet
    {
        private List<string> scalarFieldNames = new();
        private List<(string ListFieldName, string RowVar, IReadOnlyList<string> DetailFieldNames)> listFieldInfos = new();

        public MemoryStream CreatePrintExcelCheatSheet(DesignData designData, ModuleDesign module)
        {
            var book = new XLWorkbook();
            var sheet = book.Worksheets.Add("CheatSheet");
            var row = 1;

            GetCheatSheetData(designData, module);

            var url = "https://github.com/Codeer-Software/Excel.Report.PDF";
            var cell = sheet.Cell(row, 1);
            cell.SetValue(url);
            cell.SetHyperlink(new XLHyperlink(url));
            row++;
            row++;

            //単一フィールド: $FieldName.Value ---
            //見出し
            cell = sheet.Cell(row, 1);
            cell.SetValue($"フィールドシンボル");
            row++;

            foreach (var fieldName in scalarFieldNames)
            {
                cell = sheet.Cell(row, 1);
                cell.SetValue($"${fieldName}.Value");
                row++;
            }

            //List
            foreach (var info in listFieldInfos)
            {
                row++;
                cell = sheet.Cell(row, 1);
                cell.SetValue($"リスト {info.ListFieldName}");
                row++;

                var c = 1;

                sheet.Cell(row, c).SetValue(
                    $"#LoopRow(${info.ListFieldName}.Rows, {info.RowVar}, 1)");
                row++;
                sheet.Cell(row, c).SetValue(
                    $"#LoopRowData(${info.ListFieldName}.Rows, {info.RowVar}, 1)");
                row++;
                sheet.Cell(row, c).SetValue(
                    $"#PagedLoopRows(First, 10, ${info.ListFieldName}.Rows, {info.RowVar}, 1)");
                row++;
                sheet.Cell(row, c).SetValue(
                    $"#PagedLoopRows(Body, 20, ${info.ListFieldName}.Rows, {info.RowVar}, 1)");
                row++;
                sheet.Cell(row, c).SetValue(
                    $"#PagedLoopRows(Last, 15, ${info.ListFieldName}.Rows, {info.RowVar}, 1)");
                row++;

                //各明細フィールド
                foreach (var detailFieldName in info.DetailFieldNames)
                {
                    sheet.Cell(row, c).SetValue(
                        $"${info.RowVar}.{detailFieldName}.Value");
                    c++;
                }

                row++;
            }
            //特殊描画シンボル
            row++;
            cell = sheet.Cell(row, 1);
            cell.SetValue($"特殊描画シンボル");
            row++;
            cell = sheet.Cell(row, 1);
            cell.SetValue($"#Empty");
            row++;
            cell = sheet.Cell(row, 1);
            cell.SetValue($"#FitColumn");
            row++;

            //ページ関連
            row++;
            cell = sheet.Cell(row, 1);
            cell.SetValue($"ページ数関連シンボル");
            row++;
            cell = sheet.Cell(row, 1);
            cell.SetValue($"#Page");
            cell = sheet.Cell(row, 2);
            cell.SetValue($"#PageCount");
            cell = sheet.Cell(row, 3);
            cell.SetValue($@"#PageOf(""/"")");
            //ストリームに保存して返す
            var stream = new MemoryStream();
            book.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private void GetCheatSheetData(DesignData designData, ModuleDesign module)
        {
            scalarFieldNames.Clear();
            listFieldInfos.Clear();

            foreach (var field in module.Fields)
            {
                switch (field)
                {
                    //ListFieldDesign → 詳細モジュールのフィールドから $x.***.Value を生成
                    case ListFieldDesign listField:
                        {
                            var sourceModuleName = listField.SearchCondition?.ModuleName;
                            if (string.IsNullOrEmpty(sourceModuleName))
                            {
                                break;
                            }

                            var detailModule = designData.Modules.Find(sourceModuleName);
                            if (detailModule == null)
                            {
                                break;
                            }

                            //Listの中はValueタイプだけ拾う
                            var detailFieldNames = detailModule.Fields
                                .Where(f => f is ValueFieldDesignBase)
                                .Select(f => f.Name)
                                .ToList();

                            if (detailFieldNames.Count == 0)
                            {
                                break;
                            }

                            // 行変数名は例と同じ "x" で固定
                            listFieldInfos.Add((listField.Name, "x", detailFieldNames));
                            break;
                        }

                    //それ以外、Valueタイプに限定
                    default:
                        if (field is not ValueFieldDesignBase) break;
                        scalarFieldNames.Add(field.Name);
                        break;
                }
            }

            //見やすさのためソート
            scalarFieldNames.Sort(StringComparer.Ordinal);
            listFieldInfos.Sort((a, b) =>
                string.CompareOrdinal(a.ListFieldName, b.ListFieldName));
        }
    }
}
