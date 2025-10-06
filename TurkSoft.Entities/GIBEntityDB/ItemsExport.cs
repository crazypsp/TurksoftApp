using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ItemsExport
    {
        public long Id { get; set; }
        public string Company { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Address { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public string GtipNo { get; set; }
        public short RowCodeNo { get; set; }
        public int KapNo { get; set; }
        public short KapPiece { get; set; }
        public string KapType { get; set; }
        public string ShipMethod { get; set; }
        public string ShipDetail { get; set; }
        public decimal GrossKg { get; set; }
        public decimal NetKg { get; set; }
        public decimal Freight { get; set; }
        public decimal Insurance { get; set; }
    }
}
