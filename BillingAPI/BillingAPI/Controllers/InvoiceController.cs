using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domains.Interfaces.Invoice;
using Entity.Common.Entities;
using Entity.Common.WebExtention;
using Entity.Entities.DTO;
using Entity.Entities.Invoice;
using Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BillingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IWebExtention webExtention;
        private readonly IInvoiceDomain invoiceDomain;
        public InvoiceController(IConfiguration _config, IWebExtention _webExtention, IInvoiceDomain _invoiceDomain)
        {
            config = _config;
            webExtention = _webExtention;
            invoiceDomain = _invoiceDomain;

        }
        [HttpPost("Create/Invoice")]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequestDTO invoiceRequestDTO)
        {
            IActionResult response = Unauthorized();
            ResultEntity<InvoiceDTO> resEntity = await invoiceDomain.AddInvoice(invoiceRequestDTO.invoiceMaster, invoiceRequestDTO.invoiceDetails);
            response = Ok(new
            {
                Result = resEntity
            });
            return response;
        }
        [HttpPost("UpdateMaster/Invoice")]
        public async Task<IActionResult> UpdateInvoiceMaster([FromBody] InvoiceMaster entity)
        {
            IActionResult response = Unauthorized();
            ResultEntity<InvoiceMaster> resEntity = await invoiceDomain.UpdateInvoiceMaster(entity);
            response = Ok(new
            {
                Result = resEntity
            });
            return response;
        }
        [HttpPost("UpdateDetails/Invoice")]
        public async Task<IActionResult> UpdateInvoiceDetails([FromBody] List<InvoiceDetails> entity)
        {
            IActionResult response = Unauthorized();
            ResultEntity<object> resEntity = await invoiceDomain.UpdateInvoiceDetails(entity);
            response = Ok(new
            {
                Result = resEntity
            });
            return response;
        }
        [HttpDelete("Delete/Invoice")]
        public async Task<IActionResult> DeleteItemMaster([FromForm] Guid InvoiceID)
        {
            IActionResult response = Unauthorized();
            ResultEntity<bool> resEntity = await invoiceDomain.Delete(InvoiceID);
            response = Ok(new
            {
                Result = resEntity
            });
            return response;
        }
        [HttpGet("Search/Invoice")]
        public async Task<IActionResult> SearchGrid(string SearchText = "", [FromHeader] int PageNumber = 1, [FromHeader] int PageSize = 10, [FromHeader] string SortBy = "", [FromHeader] SortType SortType = SortType.ASC, [FromHeader] string Culture = "en")
        {
            IActionResult response = Unauthorized();
            GridParameters gridParam = new GridParameters();
            gridParam.SearchText = SearchText == null ? "" : SearchText.Trim();
            gridParam.PageNumber = PageNumber;
            gridParam.PageSize = PageSize;
            gridParam.SortBy = SortBy;
            gridParam.IsAscending = SortType == SortType.ASC ? true : false;
            gridParam.Culture = Culture;
            ResultList<InvoiceMaster> resEntity = await invoiceDomain.SearchItem(gridParam);
            response = Ok(new
            {
                Result = resEntity
            });
            return response;
        }
    }
}
