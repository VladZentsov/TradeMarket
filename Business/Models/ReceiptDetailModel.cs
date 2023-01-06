using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public class ReceiptDetailModel
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public int ProductId { get; set; }
        public decimal DiscountUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
