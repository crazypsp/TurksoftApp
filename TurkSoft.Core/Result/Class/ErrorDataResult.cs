using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Core.Result.Class
{
    /// <summary>
    /// Hatalı işlemler için veri taşıyan sonuç sınıfı
    /// </summary>
    public class ErrorDataResult<T> : DataResult<T>
    {
        public ErrorDataResult(T data, string message)
            : base(data, false, message)
        {
        }
        public ErrorDataResult(string message)
            : base(default, false, string.Empty)
        {
        }
        public ErrorDataResult()
            : base(default, false, string.Empty)
        {
        }
    }
}
