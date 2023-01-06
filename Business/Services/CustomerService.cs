using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Validation;
using Data.Data;
using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork=unitOfWork;
            _mapper = mapper;
        }
        public Task AddAsync(CustomerModel model)
        {
            Validation(model);

            Customer customer = _mapper.Map<CustomerModel, Customer>(model);
            _unitOfWork.CustomerRepository.AddAsync(customer);

            _unitOfWork.SaveAsync();

            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int modelId)
        {
            await _unitOfWork.CustomerRepository.DeleteByIdAsync(modelId);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<CustomerModel>> GetAllAsync()
        {
            var customers = await _unitOfWork.CustomerRepository.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<Customer>, IEnumerable<CustomerModel>>(customers);
        }

        public async Task<CustomerModel> GetByIdAsync(int id)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByIdWithDetailsAsync(id);

            if (customer == null)
                throw new MarketNotFoundException("No Customer with such id");

            return _mapper.Map<Customer, CustomerModel>(await _unitOfWork.CustomerRepository.GetByIdWithDetailsAsync(id));
        }

        public async Task<IEnumerable<CustomerModel>> GetCustomersByProductIdAsync(int productId)
        {
            var customers = (await _unitOfWork.CustomerRepository.GetAllWithDetailsAsync())
                .Where(c => c.Receipts
                .Any(r => r.ReceiptDetails
                .Any(rd => rd.ProductId == productId)));
            
            return _mapper.Map<IEnumerable<Customer>, IEnumerable<CustomerModel>>(customers);
        }

        public Task UpdateAsync(CustomerModel model)
        {
            Validation(model);

            var customer = _mapper.Map<CustomerModel, Customer>(model);

            _unitOfWork.CustomerRepository.Update(customer);
            _unitOfWork.SaveAsync();
            return Task.CompletedTask;
        }
        private void Validation(CustomerModel model)
        {
            if (model == null)
                throw new MarketException("Model is null");
            if (model.Name == null || model.Name == "")
                throw new MarketException("Name is empty");
            if (model.Surname == null || model.Surname == "")
                throw new MarketException("Surname is empty");
            if (model.BirthDate >= DateTime.Now || model.BirthDate < new DateTime(1900, 1, 1))
                throw new MarketException("Invalid BirthDate");
            if (model.DiscountValue < 0 || model.DiscountValue > 100)
                throw new MarketException("Invalid Discount");
        }
    }
}
