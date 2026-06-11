using Wellora.Areas.Doctor.Services.DoctorDashboard.Contracts;
using Wellora.Areas.Doctor.Services.DoctorDashboard.DoctorDashboardService;
using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;
using Wellora.Services.DoctorDashboard.Contracts;

namespace Wellora.Services.DoctorDashboard
{
    public class DoctorDashboardService : IDoctorDashboardService
    {
        private readonly IDoctorInfoDashboardService _doctorInfoDashboardService;
        private readonly IAppointmentDashboardService _appointmentService;
        private readonly IPatientDashboardService _patientService;
        private readonly IRevenueDashboardService _revenueService;
        private readonly IGraphDashboardService _graphService;
        private readonly IScheduleDashboardService _scheduleService;

        public DoctorDashboardService(
            IDoctorInfoDashboardService doctorInfoDashboardService,
            IAppointmentDashboardService appointmentService,
            IPatientDashboardService patientService,
            IRevenueDashboardService revenueService,
            IGraphDashboardService graphService,
            IScheduleDashboardService scheduleService)
        {
            _doctorInfoDashboardService = doctorInfoDashboardService;
            _appointmentService = appointmentService;
            _patientService = patientService;
            _revenueService = revenueService;
            _graphService = graphService;
            _scheduleService = scheduleService;
        }

        // =========================================
        // MAIN DASHBOARD ENTRY POINT
        // =========================================
        public async Task<DoctorDashboardViewModel> GetDashboardAsync(int doctorId)
        {
            var header = await _doctorInfoDashboardService.GetDoctorHeaderAsync(doctorId);
            var persinalInfo = await _doctorInfoDashboardService.GetDoctorPersonalInfoAsync(doctorId);
            var todayAppointments = await _appointmentService.GetTodayAppointmentsAsync(doctorId);
            var patientStats = await _patientService.GetPatientStatsAsync(doctorId);
            var revenue = await _revenueService.GetRevenueAsync(doctorId);
            var graphs = await _graphService.GetGraphDataAsync(doctorId);
            var schedule = await _scheduleService.GetWeeklyScheduleAsync(doctorId);
            var ClinicalPractice = await _doctorInfoDashboardService.GetClinicalPracticeAsync(doctorId);
            var Credentials = await _doctorInfoDashboardService.GetCredentialsAsync(doctorId);
            var Specialties = await _doctorInfoDashboardService.GetSpecialtiesAsync(doctorId);
            var Publications = await _doctorInfoDashboardService.GetPublicationsAsync(doctorId);

            return new DoctorDashboardViewModel
            {
                DoctorId = doctorId,
                Header = header,
                PersonalInfo = persinalInfo,
                TodayAppointments = todayAppointments,
                PatientStats = patientStats,
                Revenue = revenue,
                Graphs = graphs,
                WeeklySchedule = schedule,
                ClinicalPractice = ClinicalPractice,
                Credentials = Credentials,
                Specialties = Specialties,
                Publications = Publications
            };
        }
    }
}