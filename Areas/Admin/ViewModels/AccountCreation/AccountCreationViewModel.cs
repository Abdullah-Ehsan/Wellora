namespace Wellora.Areas.Admin.ViewModels.AccountCreation
{
    public class AccountCreationViewModel
    {
        public CreateUserAccountViewModel UserAccount { get; set; }
            = new CreateUserAccountViewModel();

        public CreateAdminAccountViewModel AdminAccount { get; set; }
            = new CreateAdminAccountViewModel();
    }
}
