using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Application.ViewModels.Shared;

namespace ReExamManagementSystem.Application.ViewModels.Student;

public class StudentDashboardViewModel
{
    public int EligibleSubjects { get; set; }
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int UpcomingExamsCount { get; set; }
    public int PublishedResultsCount { get; set; }

    public List<PublishedResultViewModel> RecentResults { get; set; } = new();
    public List<ScheduledExamViewModel> UpcomingExams { get; set; } = new();
    public List<NotificationViewModel> RecentNotifications { get; set; } = new();
    public List<ChartPoint> ApplicationsByStatus { get; set; } = new();
}
