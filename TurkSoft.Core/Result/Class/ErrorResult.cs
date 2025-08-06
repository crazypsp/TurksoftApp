using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Core.Result.Class
{
    /// <summary>
    /// Hatalı işlemler için sade sonuç nesnesi
    /// </summary>
    public class ErrorResult : Result
    {
        public ErrorResult(string message)
            : base(false, message) { }
        public ErrorResult() : base(false, string.Empty) { }
    }
}
