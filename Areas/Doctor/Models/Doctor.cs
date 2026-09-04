using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wellora.Areas.Doctor.Models
{
    [Table("doctors")]
    public class Doctor
    {
        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("date_of_birth")]
        public DateOnly DateOfBirth { get; set; }

        [Column("gender")]
        public string? Gender { get; set; }

        [Column("profile_photo")]
        public string? ProfilePhoto { get; set; }

        [Column("contact_number")]
        public string? ContactNumber { get; set; }

        [Column("hospital_address")]
        public string? HospitalAddress { get; set; }

        [Column("license_number")]
        public string? LicenseNumber { get; set; }

        [Column("specialization")]
        public string? Specialization { get; set; }

        [Column("sub_specialties")]
        public string? SubSpecialties { get; set; }

        [Column("years_experience")]
        public int? YearsExperience { get; set; }

        [Column("qualifications")]   //needs to be removed but not yet
        public string? Qualifications { get; set; }

        [Column("primary_medical_degree")]
        public string? PrimaryMedicalDegree { get; set; }

        [Column("postgraduate_degree")]
        public string? PostgraduateDegree { get; set; }

        [Column("super_specialty")]
        public string? SuperSpecialty { get; set; }

        [Column("professional_certification")]
        public string? ProfessionalCertification { get; set; }

        [Column("additional_degree")]
        public string? AdditionalDegree { get; set; }



        [Column("telemedicine_available")]
        public bool? TelemedicineAvailable { get; set; }


        [Column("consultation_fee")]
        public decimal ConsultationFee { get; set; }

        [Column("services_offered")]
        public string? ServicesOffered { get; set; }

        [Column("languages_spoken")]
        public string? LanguagesSpoken { get; set; }

        [Column("biography")]
        public string? Biography { get; set; }

        [Column("achievements")]
        public string? Achievements { get; set; }

        [Column("publications")]
        public string? Publications { get; set; }

        [Column("social_links")]
        public string? SocialLinks { get; set; }

        [Column("pmdc_number")]
        public string? PmdcNumber { get; set; }

        [Column("country")]
        public string? Country { get; set; }

        [Column("medical_school")]
        public string? MedicalSchool { get; set; }

        [Column("certifications")]
        public string? Certifications { get; set; }

        [Column("doctor_available")]
        public bool? DoctorAvailable { get; set; }


        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
