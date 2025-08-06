using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Core.Result.Interface;

namespace TurkSoft.Core.Result.Class
{
    /// <summary>
    /// IResult arayüzü temel alan soyut sınıf
    /// </summary>
    public class Result : IResult
    {
        public Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }
        public bool Success { get; }

        public string Message { get; }
    }
}
