using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public class ReceiptModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OperationDate { get; set; }
        public bool IsCheckedOut { get; set; }
        public ICollection<int> ReceiptDetailsIds { get; set; }
    }
}
