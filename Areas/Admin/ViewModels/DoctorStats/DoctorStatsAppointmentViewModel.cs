namespace Wellora.Areas.Admin.ViewModels.DoctorStats;

public class DoctorStatsAppointmentViewModel
{
    public List<DoctorStatsAppointmentTrendViewModel> AppointmentsOverTime { get; set; } = [];

    public List<DoctorStatsMonthlyAppointmentViewModel> MonthlyAppointments { get; set; } = [];

    public List<DoctorStatsWeekdayViewModel> AppointmentsByWeekday { get; set; } = [];

    public List<DoctorStatsStatusViewModel> AppointmentStatus { get; set; } = [];

    public List<DoctorStatsAppointmentMethodViewModel> AppointmentsByMethod { get; set; } = [];

    public string BusiestMonth { get; set; } = string.Empty;

    public int BusiestMonthAppointmentCount { get; set; }

    public string BusiestWeekday { get; set; } = string.Empty;

    public int BusiestWeekdayAppointmentCount { get; set; }
}


public class DoctorStatsAppointmentTrendViewModel
{
    public DateTime Date { get; set; }

    public int Count { get; set; }
}


public class DoctorStatsMonthlyAppointmentViewModel
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public int Count { get; set; }
}


public class DoctorStatsWeekdayViewModel
{
    public string Day { get; set; } = string.Empty;

    public int Count { get; set; }
}


public class DoctorStatsStatusViewModel
{
    public string Status { get; set; } = string.Empty;

    public int Count { get; set; }
}


public class DoctorStatsAppointmentMethodViewModel
{
    public string Method { get; set; } = string.Empty;

    public int Count { get; set; }
}
