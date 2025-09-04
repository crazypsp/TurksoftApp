// Controllers/Custom/Dtos/AssignDtos.cs
namespace TurkSoft.ErpApi.Controllers.Custom.Dtos
{
    /// <summary>Kullanıcıyı Bayi/Firma/MM'ye bağlamak için</summary>
    public sealed class AssignUserLinkDto
    {
        public Guid TargetId { get; set; }            // BayiId / FirmaId / MaliMusavirId
        public bool IsPrimary { get; set; } = false;
        public string? AtananRol { get; set; }        // BayiAdmin, MMUser, FirmaAdmin ...
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
    }

    /// <summary>MM ↔ KeyAccount/Luca için basit Id DTO</summary>
    public sealed class IdDto
    {
        public Guid Id { get; set; }                  // KeyAccountId veya LucaId
    }
}
