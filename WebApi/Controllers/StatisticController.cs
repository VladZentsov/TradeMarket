using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Filters;

namespace WebApi.Controllers
{
    [Route("api/statistics/")]
    [ApiController]
    [TradeMarketExceptionFilterAttribute]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpGet("popularProducts")]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetMostPopularProducts([FromQuery] int productCount )
        {
            return Content(JsonConvert.SerializeObject(await _statisticService.GetMostPopularProductsAsync(productCount)));
        }

        [HttpGet("customer/{id}/{productCount}")]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetCustomersMostPopularProducts(int id, int productCount)
        {
            return Content(JsonConvert.SerializeObject(await _statisticService.GetCustomersMostPopularProductsAsync(productCount, id)));
        }

        [HttpGet("income/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetIncomeOfCategoryInPeriod(int categoryId, [FromQuery] DateTime startDate, DateTime endDate)
        {
            return Content(JsonConvert.SerializeObject(await _statisticService.GetIncomeOfCategoryInPeriod(categoryId, startDate, endDate)));
        }

        [HttpGet("activity/{customerCount}")]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetMostValuableCustomers(int customerCount, [FromQuery] DateTime startDate, DateTime endDate)
        {
            return Content(JsonConvert.SerializeObject(await _statisticService.GetMostValuableCustomersAsync(customerCount, startDate, endDate)));
        }
    }
}
