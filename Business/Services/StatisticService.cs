using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public StatisticService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ProductModel>> GetCustomersMostPopularProductsAsync(int productCount, int customerId)
        {
            Dictionary<Product, int> productsTop = new Dictionary<Product, int>();

            var receipts = (await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync()).Where(r=>r.CustomerId==customerId);

            //var pr = await _unitOfWork.ProductRepository.GetAllWithDetailsAsync();

            foreach (var receipt in receipts)
            {
                foreach (var receiptDetail in receipt.ReceiptDetails)
                {
                    if (productsTop.ContainsKey(receiptDetail.Product))
                        productsTop[receiptDetail.Product] += receiptDetail.Quantity;
                    else
                    {
                        var resultProduct = receiptDetail.Product;
                        ICollection<ReceiptDetail> productReceiptDetails = receipt.ReceiptDetails.Where(rd => rd.ProductId == resultProduct.Id).ToList();
                        resultProduct.ReceiptDetails = productReceiptDetails;

                        productsTop.Add(resultProduct, receiptDetail.Quantity);
                    }
                }
            }

            var products = productsTop.OrderByDescending(p => p.Value).Select(p => p.Key).Take(productCount);

            var result = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductModel>>(products);

            return _mapper.Map<IEnumerable<Product>, IEnumerable<ProductModel>>(products);

        }

        public async Task<decimal> GetIncomeOfCategoryInPeriod(int categoryId, DateTime startDate, DateTime endDate)
        {
            var receipts = (await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync())
                .Where(r => r.OperationDate >= startDate && r.OperationDate <= endDate);

            decimal income = 0;

            foreach (var receipt in receipts)
            {
                foreach (var receiptDetail in receipt.ReceiptDetails.Where(rd=>rd.Product.ProductCategoryId==categoryId))
                {
                    income += receiptDetail.DiscountUnitPrice * receiptDetail.Quantity;
                }
            }

            return income;
        }

        public async Task<IEnumerable<ProductModel>> GetMostPopularProductsAsync(int productCount)
        {
            Dictionary<Product, int> productsTop = new Dictionary<Product, int>();

            var receiptDetails = await _unitOfWork.ReceiptDetailRepository.GetAllWithDetailsAsync();


            foreach (var receiptDetail in receiptDetails)
            {
                if (productsTop.ContainsKey(receiptDetail.Product))
                    productsTop[receiptDetail.Product] += receiptDetail.Quantity;
                else
                    productsTop.Add(receiptDetail.Product, receiptDetail.Quantity);
            }

            var products = productsTop.OrderByDescending(p => p.Value).Select(p => p.Key).Take(productCount);

            return _mapper.Map<IEnumerable<Product>, IEnumerable<ProductModel>>(products);
        }

        public async Task<IEnumerable<CustomerActivityModel>> GetMostValuableCustomersAsync(int customerCount, DateTime startDate, DateTime endDate)
        {
            Dictionary<Customer, decimal> customersTop = new Dictionary<Customer, decimal>();

            var receipts = (await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync())
                .Where(r => r.OperationDate >= startDate && r.OperationDate <= endDate);

            foreach (var receipt in receipts)
            {
                decimal receiprSum = 0;

                foreach (var receiptDetail in receipt.ReceiptDetails)
                {
                    receiprSum += receiptDetail.DiscountUnitPrice * receiptDetail.Quantity;
                }

                if (customersTop.ContainsKey(receipt.Customer))
                    customersTop[receipt.Customer] += receiprSum;
                else
                    customersTop.Add(receipt.Customer, receiprSum);
            }

            var customers = customersTop.OrderByDescending(c => c.Value).Take(customerCount);

            List<CustomerActivityModel> customerActivityModels = new List<CustomerActivityModel>();

            foreach (var customer in customers)
            {
                CustomerActivityModel customerActivityModel = new CustomerActivityModel() 
                { 
                    CustomerId = customer.Key.Id, 
                    CustomerName = customer.Key.Person.Name+" "+ customer.Key.Person.Surname, 
                    ReceiptSum = customer.Value 
                };

                customerActivityModels.Add(customerActivityModel);
            }

            return customerActivityModels;


        }
    }
}
