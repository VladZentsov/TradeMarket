using Data.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Data.EqualityComparers
{
    public class PersonEqualityComparer : IEqualityComparer<Person>
    {
        public bool Equals([AllowNull] Person x, [AllowNull] Person y)
        {
            if(x.Id == y.Id&&x.Name==y.Name&&x.Surname==y.Surname&&x.BirthDate==y.BirthDate)
                return true;

            return false;
        }

        public int GetHashCode([DisallowNull] Person obj)
        {
            return obj.GetHashCode();
        }
    }
}
