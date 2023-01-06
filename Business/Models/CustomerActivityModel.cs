using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public class CustomerActivityModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal ReceiptSum { get; set; }
    }
}
