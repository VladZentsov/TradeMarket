using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Data
{
    public interface ITradeMarketDbContext
    {
        DbSet<T> Set<T>() where T : class;
        int SaveChanges();
    }
}
