using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Business;
using Business.Interfaces;
using Business.Services;
using Data.Data;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped <IProductCategoryRepository, ProductCategoryRepository> ();
            services.AddScoped <IProductRepository, ProductRepository> ();
            services.AddScoped <IReceiptDetailRepository, ReceiptDetailRepository> ();
            services.AddScoped <IReceiptRepository, ReceiptRepository> ();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped <ICustomerService, CustomerService> ();
            services.AddScoped <IProductService, ProductService> ();
            services.AddScoped <IReceiptService, ReceiptService> ();
            services.AddScoped <IStatisticService, StatisticService> ();

            services.AddSingleton(provider => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfile());

            }
           ).CreateMapper());

            services.AddDbContext<ITradeMarketDbContext, TradeMarketDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Market")));

            services.AddControllers();

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("../swagger/v1/swagger.json", "MyAPI V1");
                    options.RoutePrefix = string.Empty;
                });

            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
