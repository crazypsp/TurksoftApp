namespace TurkSoft.Entities.Entities
{
    public class BankAccount
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public string? IBAN { get; set; }
        public string? SubeNo { get; set; }
        public string? MusteriNo { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Bank Bank { get; set; }
    }
}