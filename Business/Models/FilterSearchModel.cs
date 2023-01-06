using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public class FilterSearchModel
    {
        public int? CategoryId { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
    }
}
