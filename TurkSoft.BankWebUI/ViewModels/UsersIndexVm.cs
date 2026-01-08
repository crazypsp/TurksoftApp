using TurkSoft.BankWebUI.Models;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class UsersIndexVm
    {
        public List<DemoUser> Users { get; set; } = new();
    }
}
