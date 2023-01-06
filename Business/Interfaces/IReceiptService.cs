using Business.Models;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Interfaces
{
    public interface IReceiptService : ICrud<ReceiptModel>
    {
        Task AddProductAsync(int productId, int receiptId, int quantity);

        Task RemoveProductAsync(int productId, int receiptId, int quantity);

        Task<IEnumerable<ReceiptDetailModel>> GetReceiptDetailsAsync(int receiptId);

        Task<decimal> ToPayAsync(int receiptId);

        Task CheckOutAsync(int receiptId);

        Task<IEnumerable<ReceiptModel>> GetReceiptsByPeriodAsync(DateTime startDate, DateTime endDate);
    }
}
