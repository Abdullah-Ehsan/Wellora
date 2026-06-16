using Wellora.Areas.Doctor.ViewModels.DoctorProfile;

namespace Wellora.Areas.Doctor.Services
{
    public interface IDoctorProfileService
    {
        // ✅ Profile updates
        void UpdateProfile(DoctorProfileViewModel model);

        // ✅ Password updates
        void UpdatePassword(ChangePasswordViewModel model);

        // ✅ Specialization updates
        void UpdateSpecialization(DoctorDetailsViewModel model);

        // ✅ Contact info updates
        void UpdateContactInfo(DoctorDetailsViewModel model);

        // ✅ Biography, achievements, publications
        void UpdateBiography(DoctorDetailsViewModel model);

        // ✅ Consultation fee & telemedicine availability
        void UpdateConsultationInfo(DoctorDetailsViewModel model);

        // ✅ Remaining doctor details (medical school, certifications, services offered, etc.)
        void UpdateDoctorDetails(DoctorDetailsViewModel model);

        // ✅ Social media links
        void UpdateSocialLinks(DoctorDetailsViewModel model);


        
    }
}
