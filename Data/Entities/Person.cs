using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities
{
    public class Person: BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime BirthDate { get; set; }



    }
}
