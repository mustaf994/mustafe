using ReExamManagementSystem.Application.ViewModels.Student;

namespace ReExamManagementSystem.Application.Interfaces;

/// <summary>
/// Generic tabular report exporter, used by every report in the system so
/// each report only needs to supply its title, column headers and row data -
/// not its own PDF/Excel generation logic. Also generates the one
/// document-shaped PDF the system produces: the examination slip.
/// </summary>
public interface IReportExportService
{
    byte[] ExportToPdf(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows);
    byte[] ExportToExcel(string sheetTitle, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows);
    byte[] ExportExamSlipToPdf(ExamSlipViewModel slip);
}
