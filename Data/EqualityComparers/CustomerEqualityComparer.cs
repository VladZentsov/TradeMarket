using Data.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Data.EqualityComparers
{
    public class CustomerEqualityComparer : IEqualityComparer<Customer>
    {
        public bool Equals([AllowNull] Customer x, [AllowNull] Customer y)
        {
            if(x.Id==y.Id&&x.PersonId==y.PersonId&&x.DiscountValue==y.DiscountValue)
                return true;

            return false;
        }

        public int GetHashCode([DisallowNull] Customer obj)
        {
            return obj.GetHashCode();
        }
    }
}
