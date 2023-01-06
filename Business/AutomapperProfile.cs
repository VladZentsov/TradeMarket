using AutoMapper;
using Business.Models;
using Data.Entities;
using System.Linq;

namespace Business
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<Receipt, ReceiptModel>()
                .ForMember(rm => rm.ReceiptDetailsIds, r => r.MapFrom(x => x.ReceiptDetails.Select(rd => rd.Id)))
                .ReverseMap();

            CreateMap<Product, ProductModel>()
                .ForMember(rm => rm.CategoryName, r => r.MapFrom(x => x.Category.CategoryName))
                .ForMember(p => p.ReceiptDetailIds, r => r.MapFrom(x => x.ReceiptDetails.Select(rd => rd.Id)));

            CreateMap<ProductModel, Product>();

            CreateMap<ReceiptDetail, ReceiptDetailModel>()
                .ReverseMap();

            CreateMap<Customer, CustomerModel>()
                .ForMember(c=>c.Name, cm=>cm.MapFrom(cd=>cd.Person.Name))
                .ForMember(c => c.Surname, cm => cm.MapFrom(cd => cd.Person.Surname))
                .ForMember(c => c.BirthDate, cm => cm.MapFrom(cd => cd.Person.BirthDate))
                .ForMember(c => c.ReceiptsIds, r => r.MapFrom(x => x.Receipts.Select(rd => rd.Id)))
                .ReverseMap();

            CreateMap<CustomerModel, Person>();

            CreateMap<ProductCategory, ProductCategoryModel>()
               .ForMember(rm => rm.ProductIds, r => r.MapFrom(x => x.Products.Select(rd => rd.Id)))
               .ReverseMap();
        }
    }
}