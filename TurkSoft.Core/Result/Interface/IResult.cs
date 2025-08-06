using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Core.Result.Interface
{
    /// <summary>
    /// İşlem sonucu bilgisi : Başarılı mı ve mesaj nedir?
    /// </summary>
    public interface IResult
    {
        bool Success { get; } //İşlem başarılı mı?
        string Message { get; } //Kullanıcıya gösterilecek mesaj
    }
}
