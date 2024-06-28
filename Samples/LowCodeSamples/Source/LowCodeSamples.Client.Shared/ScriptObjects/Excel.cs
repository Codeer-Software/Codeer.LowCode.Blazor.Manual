using ClosedXML.Excel;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.Utils;
using Excel.Report.PDF;
using LowCodeSamples.Client.Shared.Services;

namespace LowCodeSamples.Client.Shared.ScriptObjects
{
    public class ExcelCellIndex
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public ExcelCellIndex GetNext(int rowOffset, int columnOffset)
            => new ExcelCellIndex { RowIndex = RowIndex + rowOffset, ColumnIndex = ColumnIndex + columnOffset };
    }

    public class Excel : IDisposable
    {
        private class DataGetter : IExcelSymbolConverter
        {
            Module _module;
            internal DataGetter(Module module)
                => _module = module;

            public async Task<ExcelOverWriteCell?> GetData(string text)
            {
                var value = new ObjectWrapper<object>();
                return await _module.TryGetValueByPropertyTextAsync(text, value) ? new ExcelOverWriteCell { Value = value.Value } : null;
            }

            public async Task<ExcelOverWriteCell?> GetData(object? x, string elementName, string text)
            {
                var value = new ObjectWrapper<object>();
                return await _module.TryGetValueByPropertyTextAsync(x, text, elementName, value) ? new ExcelOverWriteCell { Value = value.Value } : null;
            }
        }

        XLWorkbook _book;
        string _fileName;

        [ScriptInject]
        public Codeer.LowCode.Blazor.RequestInterfaces.Services? Services { get; set; }

        [ScriptInject]
        public HttpService? Http { get; set; }

        [ScriptHide]
        public static Func<MemoryStream, MemoryStream>? ConvertPdf { get; set; }

        public Excel(MemoryStream? stream, string fileName)
        {
            _fileName = fileName;
            _book = new XLWorkbook(stream);
        }

        public void Dispose() => _book.Dispose();

        public async Task OverWrite(Module data)
            => await _book.Worksheets.First().OverWrite(new DataGetter(data));

        public ExcelCellIndex? FindCellByText(string text)
        {
            var texts = _book.Worksheet(1).ReadAllTexts();
            for (int i = 0; i < texts.Count; i++)
            {
                for (int j = 0; j < texts[i].Count; j++)
                {
                    if (texts[i][j].Trim() == text)
                    {
                        return new ExcelCellIndex { RowIndex = i + 1, ColumnIndex = j + 1 };
                    }
                }
            }
            return null;
        }

        public void SetCellValue(ExcelCellIndex cell, object value)
        {
            var sheet = _book.Worksheets.First();
            sheet.Cell(cell.RowIndex, cell.ColumnIndex).SetValue(XLCellValue.FromObject(value));
        }

        public void CopyCells(ExcelCellIndex source, ExcelCellIndex destination, int rowCount, int colCount)
        {
            var sheet = _book.Worksheets.First();
            var rangeToCopy = sheet.Range(source.RowIndex, source.ColumnIndex, source.RowIndex + rowCount, source.ColumnIndex + colCount);
            rangeToCopy.CopyTo(sheet.Cell(destination.RowIndex, destination.ColumnIndex));
        }

        public void AddImage(ExcelCellIndex cellIndex, Stream stream)
        {
            var sheet = _book.Worksheets.First();
            var image = sheet.AddPicture(stream);
            image.MoveTo(sheet.Cell(cellIndex.RowIndex, cellIndex.ColumnIndex), 2, 2);
        }

        public async Task<bool> Download()
        {
            var stream = GetStream();
            if (stream == null) return false;
            return await Services!.UIService.DownloadFile(stream, Path.GetFileNameWithoutExtension(_fileName) + ".xlsx");
        }

        public async Task<bool> DownloadPdf()
        {
            var stream = GetStream();
            if (stream == null) return false;

            MemoryStream pdfStream = default!;
            if (ConvertPdf == null)
            {
                if (Http == null) return false;
                var result = await Http.PostContent("api/excel/pdf", new StreamContent(stream));
                if (result == null) return false;
                pdfStream = (MemoryStream)await result.Content.ReadAsStreamAsync();
            }
            else
            {
                pdfStream = ConvertPdf(stream);
            }
            return await Services!.UIService.DownloadFile(pdfStream, Path.GetFileNameWithoutExtension(_fileName) + ".pdf");
        }

        MemoryStream? GetStream()
        {
            var newStream = new MemoryStream();
            _book.SaveAs(newStream);
            newStream.Seek(0, SeekOrigin.Begin);
            return newStream;
        }
    }
}
