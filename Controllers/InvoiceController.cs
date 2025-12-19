using Microsoft.AspNetCore.Mvc;
using ApiBilling.BLL;
using ApiBilling.Models;

namespace ApiBilling.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceBLL _bll;

        public InvoiceController(InvoiceBLL bll)
        {
            _bll = bll;
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] InvoiceModel model)
        {
            int id = _bll.InsertInvoice(model);
            return Ok(new { status = "success", invoiceId = id });
        }
    }
}
