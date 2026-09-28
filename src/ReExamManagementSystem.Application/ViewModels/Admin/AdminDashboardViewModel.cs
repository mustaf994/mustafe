namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalStudents { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalPrograms { get; set; }
    public int TotalCourses { get; set; }
    public int EligibleStudents { get; set; }
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int UpcomingExams { get; set; }
    public int PublishedResults { get; set; }

    public List<ChartPoint> ApplicationsByMonth { get; set; } = new();
    public List<ChartPoint> StudentsByDepartment { get; set; } = new();
    public List<RecentApplicationViewModel> RecentApplications { get; set; } = new();
}

public class ChartPoint
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class RecentApplicationViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
}
