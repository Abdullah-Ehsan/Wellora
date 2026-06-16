using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Wellora.Areas.Doctor.Models;
using Wellora.Areas.Doctor.ViewModels.DoctorProfile;
using Wellora.Data;

namespace Wellora.Areas.Doctor.Services.DoctorProfile
{
    public class DoctorProfileService : IDoctorProfileService
    {
        private readonly ApplicationDbContext _context;

        public DoctorProfileService(ApplicationDbContext context)
        {
            _context = context;
        }


        //for the main default page
        //this is to change name,pic,email and user and gender
        public void UpdateProfile(DoctorProfileViewModel model)
        {
            // Load user
            var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null) throw new Exception("User not found");

            // Load doctor
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == model.DoctorId);
            if (doctor == null) throw new Exception("Doctor not found");

            // ✅ Capitalize names
            string Capitalize(string? name) =>
                string.IsNullOrWhiteSpace(name) ? string.Empty :
                char.ToUpper(name[0]) + name.Substring(1).ToLower();

            user.FirstName = Capitalize(model.FirstName);
            user.LastName = Capitalize(model.LastName);
            user.Email = model.Email;
            user.Username = model.Username;

            // ✅ Update doctor full name
            doctor.FullName = $"{user.FirstName} {user.LastName}".Trim();

            // ✅ Update gender
            doctor.Gender = model.Gender;
            doctor.DateOfBirth = model.DateOfBirth;

            // ✅ Handle profile picture upload
            if (model.ProfilePicture != null)
            {
                var extension = Path.GetExtension(model.ProfilePicture.FileName);
                var fileName = $"{model.Gender?.ToLower()}_{model.UserId}{model.DoctorId}{extension}";
                var savePath = Path.Combine("wwwroot", "User", "Doctor", "Profile_Picture", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    model.ProfilePicture.CopyTo(stream);
                }

                doctor.ProfilePhoto = $"/User/Doctor/Profile_Picture/{fileName}";
            }

            _context.SaveChanges();
        }




        //this is for to change or add schdule
        public void UpdateScheduleAndBreaks(DoctorScheduleUpdateViewModel model)
        {
            // Load existing schedules & breaks
            var existingSchedules = _context.DoctorSchedules
                .Where(s => s.DoctorId == model.DoctorId)
                .ToList();

            var existingBreaks = _context.DoctorBreaks
                .Where(b => b.DoctorId == model.DoctorId)
                .ToList();

            // Serialize old/new for history
            var oldScheduleJson = JsonSerializer.Serialize(new { Schedules = existingSchedules, Breaks = existingBreaks });
            var newScheduleJson = JsonSerializer.Serialize(new { Schedules = model.ScheduleRows, Breaks = model.Breaks });

            // Replace schedules
            _context.DoctorSchedules.RemoveRange(existingSchedules);
            foreach (var row in model.ScheduleRows)
            {
                var entity = new DoctorSchedule
                {
                    DoctorId = model.DoctorId,
                    DayOfWeek = row.DayOfWeek,
                    StartTime = row.StartTime,
                    EndTime = row.EndTime,
                    MaxPatientsPerDay = row.MaxPatientsPerDay,
                    AppointmentDurationMin = row.AppointmentDurationMin,
                    IsActive = row.IsActive
                };
                _context.DoctorSchedules.Add(entity);
            }

            // Replace breaks
            _context.DoctorBreaks.RemoveRange(existingBreaks);
            foreach (var br in model.Breaks)
            {
                var entity = new DoctorBreak
                {
                    DoctorId = model.DoctorId,
                    DayOfWeek = br.DayOfWeek,
                    BreakStart = br.BreakStart,
                    BreakEnd = br.BreakEnd
                };
                _context.DoctorBreaks.Add(entity);
            }

            _context.SaveChanges();

            // Save history
            var lastHistory = _context.DoctorScheduleHistorys
                .Where(h => h.DoctorId == model.DoctorId)
                .OrderByDescending(h => h.ChangedAt)
                .FirstOrDefault();

            var history = new DoctorScheduleHistory
            {
                DoctorId = model.DoctorId,
                OldSchedule = lastHistory?.NewSchedule ?? oldScheduleJson,
                NewSchedule = newScheduleJson,
                ActionType = lastHistory == null ? "INSERT" : "UPDATE",
                ChangedAt = DateTime.UtcNow
            };

            _context.DoctorScheduleHistorys.Add(history);
            _context.SaveChanges();
        }



        public void UpdatePassword(ChangePasswordViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null) throw new Exception("User not found");

            // ✅ Verify old password
            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
            {
                throw new Exception("Old password is incorrect");
            }

            // ✅ Check new vs confirm
            if (model.NewPassword != model.ConfirmPassword)
            {
                throw new Exception("New password and confirm password do not match");
            }

            // ✅ Hash new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // ✅ Update timestamp
            user.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }




        public void UpdateSpecialization(DoctorDetailsViewModel model)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == model.DoctorId);
            if (doctor == null) throw new Exception("Doctor not found");

            // ✅ Capitalize specialization and subspecialties
            string Capitalize(string? text) =>
                string.IsNullOrWhiteSpace(text) ? string.Empty :
                char.ToUpper(text[0]) + text.Substring(1).ToLower();

            doctor.Specialization = Capitalize(model.Specialization);
            doctor.SubSpecialties = Capitalize(model.SubSpecialties);
            doctor.Qualifications = model.Qualifications; // keep raw text (degrees, etc.)
            doctor.Certifications = model.Certifications; // keep raw text
            doctor.YearsExperience = model.YearsExperience;

            doctor.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }

        public void UpdateContactInfo(DoctorDetailsViewModel model)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == model.DoctorId);
            if (doctor == null) throw new Exception("Doctor not found");

            // ✅ Update contact details
            doctor.ContactNumber = model.ContactNumber;
            doctor.HospitalAddress = model.HospitalAddress;
            doctor.Country = model.Country;

            // ✅ Update timestamp
            doctor.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }


        public void UpdateBiography(DoctorDetailsViewModel model)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == model.DoctorId);
            if (doctor == null) throw new Exception("Doctor not found");

            // ✅ Update biography-related fields
            doctor.Biography = model.Biography;
            doctor.Achievements = model.Achievements;
            doctor.Publications = model.Publications;

            // ✅ Update timestamp
            doctor.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }


        public void UpdateConsultationInfo(DoctorDetailsViewModel model)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == model.DoctorId);
            if (doctor == null) throw new Exception("Doctor not found");

            // ✅ Update consultation fee and telemedicine flag
            doctor.ConsultationFee = model.ConsultationFee;
            doctor.TelemedicineAvailable = model.TelemedicineAvailable;

            // ✅ Update timestamp
            doctor.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }

        public void UpdateDoctorDetails(DoctorDetailsViewModel model)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == model.DoctorId);
            if (doctor == null) throw new Exception("Doctor not found");

            // ✅ Medical school, certifications, qualifications
            doctor.MedicalSchool = model.MedicalSchool;
            doctor.Certifications = model.Certifications;
            doctor.Qualifications = model.Qualifications;

            

            // ✅ License & PMDC number
            doctor.LicenseNumber = model.LicenseNumber;
            doctor.PmdcNumber = model.PmdcNumber;

            // ✅ Services offered & languages spoken
            doctor.ServicesOffered = model.ServicesOffered;
            doctor.LanguagesSpoken = model.LanguagesSpoken;

            // ✅ Achievements, publications, biography (if not updated separately)
            doctor.Achievements = model.Achievements;
            doctor.Publications = model.Publications;
            doctor.Biography = model.Biography;

            // ✅ Update timestamp
            doctor.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }


        public void UpdateSocialLinks(DoctorDetailsViewModel model)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == model.DoctorId);
            if (doctor == null) throw new Exception("Doctor not found");

            // ✅ Update social links (JSON, comma-separated, or raw string depending on your design)
            doctor.SocialLinks = model.SocialLinks;

            doctor.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }


    }
}
