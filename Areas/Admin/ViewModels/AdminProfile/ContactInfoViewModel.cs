using System.ComponentModel.DataAnnotations;

namespace Wellora.Areas.Admin.ViewModels.AdminProfile
{
    public class ContactInfoViewModel
    {
        [StringLength(30)]
        public string? ContactNumber { get; set; }

        [StringLength(30)]
        public string? OfficeNumber { get; set; }

        [StringLength(30)]
        public string? OfficeOfficialNumber { get; set; }

        [StringLength(150)]
        public string? EmergencyContactName { get; set; }

        [StringLength(30)]
        public string? EmergencyContactNumber { get; set; }
    }
}
