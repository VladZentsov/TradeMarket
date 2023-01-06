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
    [Route("api/[controller]")]
    [ApiController]
    [TradeMarketExceptionFilterAttribute]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptService _receiptService;

        public ReceiptsController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] ReceiptModel value)
        {
            await _receiptService.AddAsync(value);

            return Ok(value);
        }

        [HttpPut("{id}/products/add/{productId}/{quantity}")]
        public async Task<ActionResult> AddProduct(int id, int productId, int quantity, [FromBody] ReceiptDetailModel receiptDetail)
        {
            await _receiptService.AddProductAsync(productId, id, quantity);

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReceiptModel>>> Get()
        {
            return Content(JsonConvert.SerializeObject(await _receiptService.GetAllAsync()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReceiptModel>> GetById(int id)
        {
            return Content(JsonConvert.SerializeObject(await _receiptService.GetByIdAsync(id)));
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<ReceiptModel>> GetReceiptDetails(int id)
        {
            return Content(JsonConvert.SerializeObject(await _receiptService.GetReceiptDetailsAsync(id)));
        }
        [HttpGet("{id}/sum")]
        public async Task<ActionResult<ReceiptModel>> GetSum(int id)
        {
            return Content(JsonConvert.SerializeObject(await _receiptService.ToPayAsync(id)));
        }

        [HttpGet("period")]
        public async Task<ActionResult<ReceiptModel>> GetReceiptsByPeriod([FromQuery] DateTime startDate, DateTime endDate)
        {
            return Content(JsonConvert.SerializeObject(await _receiptService.GetReceiptsByPeriodAsync(startDate, endDate)));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int Id, [FromBody] ReceiptModel receipt)
        {
            receipt.Id = Id;
            await _receiptService.UpdateAsync(receipt);

            return Ok();
        }

        [HttpPut("{id}/products/remove/{productId}/{quantity}")]
        public async Task<ActionResult> RemoveProduct(int id, int productId, int quantity, [FromBody] ReceiptDetailModel receiptDetail)
        {
            await _receiptService.RemoveProductAsync(productId, id, quantity);

            return Ok();
        }

        [HttpPut("{id}/checkout")]
        public async Task<ActionResult> CheckOut(int id)
        {
            await _receiptService.CheckOutAsync(id);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _receiptService.DeleteAsync(id);

            return Ok();
        }

    }
}
