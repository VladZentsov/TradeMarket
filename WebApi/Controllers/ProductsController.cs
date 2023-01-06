using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Filters;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TradeMarketExceptionFilterAttribute]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductModel>> GetById(int id)
        {
            return Content(JsonConvert.SerializeObject(await _productService.GetByIdAsync(id)));
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<ProductCategoryModel>>> Get()
        {
            return Content(JsonConvert.SerializeObject(await _productService.GetAllProductCategoriesAsync()));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetByFilter([FromQuery]  FilterSearchModel filterSearchModel)
        {
            return Content(JsonConvert.SerializeObject(await _productService.GetByFilterAsync(filterSearchModel)));
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] ProductModel value)
        {
            await _productService.AddAsync(value);

            return Ok(value);
        }

        [HttpPost("categories")]
        public async Task<ActionResult> AddCategory([FromBody] ProductCategoryModel value)
        {
            await _productService.AddCategoryAsync(value);

            return Ok(value);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);

            return Ok();
        }

        [HttpDelete("categories/{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            await _productService.RemoveCategoryAsync(id);

            return Ok();
        }

        // PUT: api/customers/1
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int Id, [FromBody] ProductModel value)
        {
            value.Id = Id;
            await _productService.UpdateAsync(value);

            return Ok();
        }

        [HttpPut("categories/{id}")]
        public async Task<ActionResult> UpdateCategory(int Id, [FromBody] ProductCategoryModel value)
        {
            value.Id = Id;
            await _productService.UpdateCategoryAsync(value);

            return Ok();
        }

    }
}
