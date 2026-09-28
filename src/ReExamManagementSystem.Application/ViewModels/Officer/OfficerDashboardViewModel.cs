using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.ViewModels.Officer;

public class OfficerDashboardViewModel
{
    public int EligibleStudents { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int UpcomingExams { get; set; }
    public int CompletedExams { get; set; }
    public int PendingResults { get; set; }
    public int PublishedResults { get; set; }

    public List<ApplicationListItemViewModel> RecentApplications { get; set; } = new();
    public List<ChartPoint> ApplicationsByStatus { get; set; } = new();
}
