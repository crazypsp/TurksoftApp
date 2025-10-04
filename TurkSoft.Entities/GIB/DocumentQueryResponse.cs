using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class DocumentQueryResponse
    {
        public int QueryState { get; set; }
        public string StateExplanation { get; set; }
        public int DocumentsCount { get; set; }
        public string MaxRecordIdInList { get; set; }
        public List<ResponseDocument> Documents { get; set; } = new();
    }
}
