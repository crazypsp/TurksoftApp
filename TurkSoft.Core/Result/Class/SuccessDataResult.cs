using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Core.Result.Class
{
    /// <summary>
    /// Başarılı işlemler için veri taşıyan sonuç sınıfı
    /// </summary>
    public class SuccessDataResult<T> : DataResult<T>
    {
        public SuccessDataResult(T data, string message)
            : base(data, true, message)
        {
        }
        public SuccessDataResult(T data)
            : base(data, true, string.Empty)
        {
        }
    }
}
