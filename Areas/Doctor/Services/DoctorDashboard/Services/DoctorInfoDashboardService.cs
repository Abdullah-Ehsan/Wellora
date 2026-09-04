using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Wellora.Areas.Doctor.Models;
using Wellora.Areas.Doctor.Services.DoctorDashboard.Contracts;
using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;
using Wellora.Data;

namespace Wellora.Areas.Doctor.Services.DoctorDashboard.Services
{
    public class DoctorInfoDashboardService: IDoctorInfoDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DoctorInfoDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DoctorHeaderViewModel> GetDoctorHeaderAsync(int doctorId)
        {
            var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == doctor.UserId);


            return new DoctorHeaderViewModel
            {
                DoctorId = doctorId,
                FullName = doctor.FullName,
                Username = user.Username,   
                Email = user.Email,
                ProfilePhoto = doctor.ProfilePhoto,
                Specialization = doctor.Specialization
            };
        }


        public async Task<DoctorPersonalInfoViewModel> GetDoctorPersonalInfoAsync(int doctorId)
        {
            var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            return new DoctorPersonalInfoViewModel
            {
                DateOfBirth = doctor.DateOfBirth,
                Gender = doctor.Gender,
                Country = doctor.Country,
                ContactNumber = doctor.ContactNumber,
                HospitalAddress = doctor.HospitalAddress,
                LanguagesSpoken = doctor.LanguagesSpoken

            };
        }



        public async Task<ClinicalPracticeViewModel> GetClinicalPracticeAsync(int doctorId)
        {
            var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            return new ClinicalPracticeViewModel
            {
                YearsExperience = doctor.YearsExperience,
                TelemedicineAvailable = doctor.TelemedicineAvailable,
                ConsultationFee = doctor.ConsultationFee,
                DoctorAvailable = doctor.DoctorAvailable,
            };
        }


        public async Task<CredentialsViewModel> GetCredentialsAsync(int doctorId)
        {

            var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);



            return new CredentialsViewModel
            {
                LicenseNumber = doctor.LicenseNumber,
                PmdcNumber = doctor.PmdcNumber,

                PrimaryMedicalDegree = doctor.PrimaryMedicalDegree,
                PostgraduateDegree = doctor.PostgraduateDegree,
                SuperSpecialty = doctor.SuperSpecialty,
                ProfessionalCertification = doctor.ProfessionalCertification,

                MedicalSchool = doctor.MedicalSchool,
                Certifications = doctor.Certifications
            };

        }



        public async Task<SpecialtiesViewModel> GetSpecialtiesAsync(int doctorId)
        {

            var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);


            return new SpecialtiesViewModel
            {
                Specialization = doctor.Specialization,
                SubSpecialties = doctor.SubSpecialties,
                ServicesOffered = doctor.ServicesOffered
            };
        }



        public async Task<PublicationViewModel> GetPublicationsAsync(int doctorId)
        {

            var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);



            return new PublicationViewModel
            {
                Biography = doctor.Biography,
                Achievements = doctor.Achievements,
                Publications = doctor.Publications,
                SocialLinks = doctor.SocialLinks,
                CreatedAt = doctor.CreatedAt,
                UpdatedAt = doctor.UpdatedAt
            };
        }
    }
}
