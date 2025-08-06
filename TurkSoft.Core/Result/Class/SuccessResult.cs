using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Core.Result.Class
{
    /// <summary>
    /// Başarılı işlemler sade sonuç nesnesi
    /// </summary>
    public class SuccessResult : Result
    {
        public SuccessResult(string message)
            : base(true, message)
        {
        }
        public SuccessResult() : base(true, string.Empty) { }
    }
}
