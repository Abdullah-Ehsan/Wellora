namespace Wellora.ViewModels
{
    public class ContactViewModel
    {
        public string Phone { get; set; } = "+92 300 1234567";
        public string Email { get; set; } = "info@example.com";
        public string Address { get; set; } = "Govt Jinnah Islamia College, Sialkot";

        public ContactFormModel Form { get; set; } = new ContactFormModel();
    }

    public class ContactFormModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
