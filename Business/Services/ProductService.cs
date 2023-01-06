using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Validation;
using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public Task AddAsync(ProductModel model)
        {
            Validation(model);

            var product = _mapper.Map<ProductModel, Product>(model);

            _unitOfWork.ProductRepository.AddAsync(product);

            _unitOfWork.SaveAsync();

            return Task.CompletedTask;
        }

        public Task AddCategoryAsync(ProductCategoryModel categoryModel)
        {
            Validation(categoryModel);

            var category = _mapper.Map<ProductCategoryModel, ProductCategory>(categoryModel);

            _unitOfWork.ProductCategoryRepository.AddAsync(category);

            _unitOfWork.SaveAsync();

            return Task.CompletedTask;
        }

        public Task DeleteAsync(int modelId)
        {
            _unitOfWork.ProductRepository.DeleteByIdAsync(modelId);

            _unitOfWork.SaveAsync();

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<ProductModel>> GetAllAsync()
        {
            var products =await _unitOfWork.ProductRepository.GetAllWithDetailsAsync();

            return _mapper.Map<IEnumerable<Product>, IEnumerable<ProductModel>>(products);
        }

        public async Task<IEnumerable<ProductCategoryModel>> GetAllProductCategoriesAsync()
        {
            var categories = await _unitOfWork.ProductCategoryRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<ProductCategory>, IEnumerable<ProductCategoryModel>>(categories);
        }

        public async Task<IEnumerable<ProductModel>> GetByFilterAsync(FilterSearchModel filterSearch)
        {
            var products =await _unitOfWork.ProductRepository.GetAllWithDetailsAsync();

            if(filterSearch.CategoryId!=null)
                products=products.Where(p=>p.ProductCategoryId==filterSearch.CategoryId);

            if(filterSearch.MinPrice!=null)
                products = products.Where(p=>p.Price>=filterSearch.MinPrice);

            if(filterSearch.MaxPrice!=null)
                products=products.Where(p=>p.Price<=filterSearch.MaxPrice);

            return _mapper.Map<IEnumerable<Product>, IEnumerable<ProductModel>>(products);
        }

        public async Task<ProductModel> GetByIdAsync(int id)
        {
            return _mapper.Map<Product, ProductModel>(await _unitOfWork.ProductRepository.GetByIdWithDetailsAsync(id));
        }

        public async Task RemoveCategoryAsync(int categoryId)
        {
            await _unitOfWork.ProductCategoryRepository.DeleteByIdAsync(categoryId);

            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(ProductModel model)
        {
            Validation(model);

            _unitOfWork.ProductRepository.Update(_mapper.Map<ProductModel, Product>(model));

            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateCategoryAsync(ProductCategoryModel categoryModel)
        {
            Validation(categoryModel);

            _unitOfWork.ProductCategoryRepository.Update(_mapper.Map<ProductCategoryModel, ProductCategory>(categoryModel));

            await _unitOfWork.SaveAsync();
        }

        private void Validation(ProductModel model)
        {
            if (model == null)
                throw new MarketException("Product is null");
            if(model.ProductName == null||model.ProductName=="")
                throw new MarketException("Product name is empty");
            if(model.Price<=0)
                throw new MarketException("Product price is less or equal 0");
        }

        private void Validation(ProductCategoryModel model)
        {
            if(model == null)
                throw new MarketException("Product category is null");
            if(model.CategoryName == null||model.CategoryName=="")
                throw new MarketException("Product category name is empty");
        }
    }
}
