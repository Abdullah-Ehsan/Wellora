using Microsoft.EntityFrameworkCore;
using Wellora.Areas.Patient.Models;
using Wellora.Areas.Patient.ViewModels.PatientProfile;
using Wellora.Data;
using Wellora.Models;
using PatientEntity = Wellora.Areas.Patient.Models.Patient;

namespace Wellora.Areas.Patient.Services.PatientProfile
{
    public class PatientProfileService
    {
        private readonly ApplicationDbContext _context;

        public PatientProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Get Patient entity by UserId (includes User for email/username mapping)
        public PatientEntity? GetPatientByUserId(int userId)
        {
            return _context.Patients.FirstOrDefault(p => p.UserId == userId);
        }

        public User? GetUserById(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == userId);
        }


        // 🔹 Update Medical History from ViewModel
        public void UpdateMedicalHistory(int userId, MedicalHistoryViewModel vm)
        {
            var patient = GetPatientByUserId(userId);
            if (patient == null) return;

            patient.Allergies = vm.Allergies;
            patient.MedicalConditions = vm.MedicalConditions;
            patient.Medications = vm.Medications;
            patient.BloodGroup = vm.BloodGroup;

            SaveChanges();
        }

        // 🔹 Update Emergency Contacts from ViewModel
        public void UpdateEmergencyContacts(int userId, EmergencyContactsViewModel vm)
        {
            var patient = GetPatientByUserId(userId);
            if (patient == null) return;

            patient.EmergencyContactName = vm.EmergencyContactName;
            patient.EmergencyContactPhone = vm.EmergencyContactPhone;

            SaveChanges();
        }

        // 🔹 Update Profile Info from ViewModel
        public void UpdateProfileInfo(int userId, ProfileInfoViewModel vm)
        {
            var patient = GetPatientByUserId(userId);
            var user = GetUserById(userId);

            if (patient == null || user == null) return;

            patient.FullName = vm.FullName;
            patient.DateOfBirth = vm.DateOfBirth;
            patient.Gender = vm.Gender;
            patient.Address = vm.Address;
            patient.PreferredLanguage = vm.PreferredLanguage;
            patient.ProfilePhoto = vm.ProfilePhoto;

            user.Email = vm.Email;
            user.Username = vm.Username;

            SaveChanges();
        }

        // 🔹 Change Password (with hashing logic placeholder)
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = GetUserById(userId);
            if (user == null) return false;

            // Replace with proper hashing/verification logic
            if (user.PasswordHash != oldPassword) return false;

            user.PasswordHash = newPassword; // hash before saving in real app
            SaveChanges();
            return true;
        }

        // 🔹 Save changes
        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
