namespace TurkSoft.BankWebUI.ViewModels
{
    public class BankTransactionVm
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string BankName { get; set; }
        public string AccountType { get; set; }
        public string AccountNumber { get; set; }
        public string ReferenceNo { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public decimal Net => Credit - Debit;
    }
}
