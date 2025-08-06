using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.Luca
{
    public class LucaJobQueueItem
    {
        public LucaLoginRequest Login {  get; set; }
        public List<LucaFisRow> FisRows { get; set; }
    }
}
