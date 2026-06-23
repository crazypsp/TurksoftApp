using System;
using System.Collections.Generic;

namespace TurkSoft.Service.Implementations
{
    /// <summary>
    /// Servis katmanı genel sonuç sarmalayıcısı.
    /// (Önceden LogoTigerIntegrationService.cs içinde tanımlıydı; Logo/COM bağımlılığından
    /// bağımsız derlenebilmesi için ayrı dosyaya taşındı.)
    /// </summary>
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime ResponseTime { get; set; } = DateTime.UtcNow;

        public static ServiceResult<T> SuccessResult(T data, string message = null)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "İşlem başarıyla tamamlandı",
                Errors = new List<string>()
            };
        }

        public static ServiceResult<T> ErrorResult(string message, List<string> errors = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Data = default,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
