using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.Document
{
    public class ExcelUploadRequest
    {
        public IFormFile Dosya { get; set; }
        public string KlasorYolu { get; set; }
    }
}
