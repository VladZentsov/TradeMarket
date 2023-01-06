using AutoMapper;
using Business.Helper;
using Business.Interfaces;
using Business.Models;
using Business.Validation;
using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Business.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReceiptService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddAsync(ReceiptModel model)
        {
            var receipt = _mapper.Map<ReceiptModel, Receipt>(model);

            await _unitOfWork.ReceiptRepository.AddAsync(receipt);

            await _unitOfWork.SaveAsync();
        }

        public async Task AddProductAsync(int productId, int receiptId, int quantity)
        {
            decimal productPrice = 0;

            if (_unitOfWork.ProductRepository!=null)
            {
                var product =await _unitOfWork.ProductRepository.GetByIdAsync(productId);
                if (product == null)
                    throw new MarketException("Product with this productId doesn`t exists");

                productPrice = product.Price;
            }

            if (_unitOfWork.ReceiptRepository != null)
            {
                var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(receiptId);

                if (receipt != null&&receipt.ReceiptDetails!=null)
                {
                    var receiptDetails = receipt.ReceiptDetails.FirstOrDefault(rd => rd.ProductId == productId);

                    if (receiptDetails != null)
                    {
                        receiptDetails.Quantity += quantity;
                        await _unitOfWork.SaveAsync();
                        return;
                    }
                }
     
            }

            decimal discount = (await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(receiptId)).Customer.DiscountValue;

            ReceiptDetail receiptDetail = new ReceiptDetail() { ProductId = productId, Quantity = quantity, 
                ReceiptId = receiptId, DiscountUnitPrice = productPrice*((100-discount)/100), UnitPrice = productPrice};

            await IdHelper.MakeId(_unitOfWork.ReceiptDetailRepository, receiptDetail);

            await _unitOfWork.ReceiptDetailRepository.AddAsync(receiptDetail);

            await _unitOfWork.SaveAsync();

        }

        public async Task CheckOutAsync(int receiptId)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdAsync(receiptId);
            receipt.IsCheckedOut=true;

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int modelId)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(modelId);

            foreach (var receiptDetail in receipt.ReceiptDetails)
            {
                _unitOfWork.ReceiptDetailRepository.Delete(receiptDetail);
            }

            await _unitOfWork.ReceiptRepository.DeleteByIdAsync(modelId);

            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<ReceiptModel>> GetAllAsync()
        {
            var receipts = await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync();

            return _mapper.Map<IEnumerable<Receipt>, IEnumerable<ReceiptModel>>(receipts);
        }

        public async Task<ReceiptModel> GetByIdAsync(int id)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(id);

            return _mapper.Map<Receipt, ReceiptModel>(receipt);
        }

        public async Task<IEnumerable<ReceiptDetailModel>> GetReceiptDetailsAsync(int receiptId)
        {
            var receiptDetails = (await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(receiptId)).ReceiptDetails;

            return _mapper.Map<IEnumerable<ReceiptDetail>, IEnumerable<ReceiptDetailModel>>(receiptDetails);
        }

        public async Task<IEnumerable<ReceiptModel>> GetReceiptsByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var receipts = (await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync()).Where(r=>r.OperationDate>=startDate&&r.OperationDate<=endDate);

            return _mapper.Map<IEnumerable<Receipt>, IEnumerable<ReceiptModel>>(receipts);
        }

        public async Task RemoveProductAsync(int productId, int receiptId, int quantity)
        {
            var receiptDetails = (await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(receiptId))
                .ReceiptDetails.Where(rd => rd.ProductId == productId);

            foreach (var receiptDetail in receiptDetails)
            {
                if (receiptDetail.Quantity <= quantity)
                {
                    _unitOfWork.ReceiptDetailRepository.Delete(receiptDetail);
                }
                else
                    receiptDetail.Quantity -= quantity;
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task<decimal> ToPayAsync(int receiptId)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(receiptId);

            decimal sum = 0;

            foreach (var receiptDetail in receipt.ReceiptDetails)
            {
                sum += receiptDetail.DiscountUnitPrice * receiptDetail.Quantity;
            }

            return sum;
        }

        public async Task UpdateAsync(ReceiptModel model)
        {
            _unitOfWork.ReceiptRepository.Update(_mapper.Map<ReceiptModel, Receipt>(model));

            await _unitOfWork.SaveAsync();
        }
    }
}
