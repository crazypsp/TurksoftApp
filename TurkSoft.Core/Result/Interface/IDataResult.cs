using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Core.Result.Interface
{
    /// <summary>
    /// Veri içeren işlem sonucu
    /// </summary>
    public interface IDataResult<T> : IResult
    {
        T Data { get; } //Geri döndürülecek nesne
    }
}
