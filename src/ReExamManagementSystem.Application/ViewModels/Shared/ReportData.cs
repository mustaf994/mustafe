namespace ReExamManagementSystem.Application.ViewModels.Shared;

public record ReportData(string Title, IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows);
