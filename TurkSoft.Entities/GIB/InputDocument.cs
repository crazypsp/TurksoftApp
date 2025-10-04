using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class InputDocument
    {
        public string DocumentUUID { get; set; }
        public string XmlContent { get; set; }
        public string SourceUrn { get; set; }
        public string DestinationUrn { get; set; }
        public string LocalId { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string DocumentId { get; set; }
        public string DocumentNoPrefix { get; set; }
        public bool? SubmitForApproval { get; set; }
        public string Note { get; set; }
    }
}
