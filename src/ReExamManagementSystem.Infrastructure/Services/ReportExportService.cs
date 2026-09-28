using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Student;

namespace ReExamManagementSystem.Infrastructure.Services;

public class ReportExportService : IReportExportService
{
    static ReportExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportToPdf(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().Text("Re-Exam Management System").FontSize(14).Bold();
                    column.Item().Text(title).FontSize(11).SemiBold();
                    column.Item().Text($"Generated on {DateTime.Now:dd MMM yyyy, HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in headers) columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var head in headers)
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(head).Bold();
                        }
                    });

                    foreach (var row in rows)
                    {
                        foreach (var cell in row)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(cell);
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportExamSlipToPdf(ExamSlipViewModel slip)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("RE-EXAM MANAGEMENT SYSTEM").FontSize(14).Bold();
                    column.Item().AlignCenter().Text("University Examination Slip").FontSize(11).SemiBold().FontColor(Colors.Grey.Darken2);
                    column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingTop(15).Column(column =>
                {
                    column.Spacing(6);

                    void Row(string label, string value)
                    {
                        column.Item().Row(row =>
                        {
                            row.ConstantItem(110).Text(label).SemiBold();
                            row.RelativeItem().Text(value);
                        });
                    }

                    Row("Student Number", slip.StudentNumber);
                    Row("Student Name", slip.StudentName);
                    Row("Department", slip.DepartmentName);
                    Row("Program", slip.ProgramName);

                    column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    column.Item().PaddingTop(8);

                    Row("Course Code", slip.CourseCode);
                    Row("Course Name", slip.CourseName);
                    Row("Exam Date", slip.ExamDate.ToString("dddd, dd MMMM yyyy"));
                    Row("Exam Time", $"{slip.StartTime:hh\\:mm} - {slip.EndTime:hh\\:mm}");
                    Row("Exam Room", slip.RoomLabel);

                    column.Item().PaddingTop(15).Background(Colors.Grey.Lighten4).Padding(8).Column(instructions =>
                    {
                        instructions.Item().Text("Examination Instructions").Bold();
                        instructions.Item().Text("- Arrive at the examination room at least 15 minutes before the start time.");
                        instructions.Item().Text("- Bring this slip and a valid student ID card. No admission without both.");
                        instructions.Item().Text("- Mobile phones and unauthorized materials are not permitted in the examination room.");
                    });

                    column.Item().PaddingTop(15).AlignCenter().Text($"Reference: {slip.ReferenceNumber}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.Span(DateTime.Now.ToString("dd MMM yyyy, HH:mm")).FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportToExcel(string sheetTitle, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        using var workbook = new XLWorkbook();
        var safeSheetName = sheetTitle.Length > 31 ? sheetTitle[..31] : sheetTitle;
        var worksheet = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(safeSheetName) ? "Report" : safeSheetName);

        for (var c = 0; c < headers.Count; c++)
        {
            var cell = worksheet.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(240, 240, 240);
        }

        for (var r = 0; r < rows.Count; r++)
        {
            var row = rows[r];
            for (var c = 0; c < row.Count; c++)
            {
                worksheet.Cell(r + 2, c + 1).Value = row[c];
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
