using Data.Data;
using Data.Interfaces;
using Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Data.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ITradeMarketDbContext _tradeMarketDbContext;

        private ICustomerRepository _customerRepository;
        private IPersonRepository _personRepository;
        private IProductRepository _productRepository;
        private IProductCategoryRepository _productCategoryRepository;
        private IReceiptDetailRepository _receiptDetailRepository;
        private IReceiptRepository _receiptRepository;

        public UnitOfWork(ITradeMarketDbContext tradeMarketDbContext)
        {
            _tradeMarketDbContext = tradeMarketDbContext;
        }

        public ICustomerRepository CustomerRepository
        {
            get
            {
                if (_customerRepository == null)
                    _customerRepository = new CustomerRepository(_tradeMarketDbContext);
                return _customerRepository;
            }
        }

        public IPersonRepository PersonRepository
        {
            get
            {
                if (_personRepository == null)
                    _personRepository = new PersonRepository(_tradeMarketDbContext);
                return _personRepository;
            }
        }

        public IProductCategoryRepository ProductCategoryRepository
        {
            get
            {
                if (_productCategoryRepository == null)
                    _productCategoryRepository = new ProductCategoryRepository(_tradeMarketDbContext);
                return _productCategoryRepository;
            }
        }

        public IProductRepository ProductRepository
        {
            get
            {
                if (_productRepository == null)
                    _productRepository = new ProductRepository(_tradeMarketDbContext);
                return _productRepository;
            }
        }

        public IReceiptDetailRepository ReceiptDetailRepository
        {
            get
            {
                if (_receiptDetailRepository == null)
                    _receiptDetailRepository = new ReceiptDetailRepository(_tradeMarketDbContext);
                return _receiptDetailRepository;
            }
        }

        public IReceiptRepository ReceiptRepository
        {
            get
            {
                if (_receiptRepository == null)
                    _receiptRepository = new ReceiptRepository(_tradeMarketDbContext);
                return _receiptRepository;
            }
        }

        public Task SaveAsync()
        {
            _tradeMarketDbContext.SaveChanges();
            return Task.CompletedTask;
        }
    }
}
