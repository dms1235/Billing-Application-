using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domains.Interfaces.Masters;
using Entity.Common.Entities;
using Entity.Common.WebExtention;
using Entity.Entities.Auth;
using Entity.Entities.Masters;
using Entity.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BillingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IWebExtention webExtention;
        private readonly IItemMaster itemMaster;
        public ItemController(IConfiguration _config, IWebExtention _webExtention, IItemMaster _itemMaster)
        {
            config = _config;
            webExtention = _webExtention;
            itemMaster = _itemMaster;

        }
        [HttpPost("Create/ItemMaster")]
        public async Task<IActionResult> CreateItemMaster([FromBody] ItemMasters entity)
        {
            IActionResult response = Unauthorized();
            ResultEntity<ItemMasters> resEntity = await itemMaster.Add(entity);
            response = Ok(new
            {
                Result = resEntity
            });
            return response;
        }
        [HttpPost("Update/ItemMaster")]
        public async Task<IActionResult> UpdateItemMaster([FromBody] ItemMasters entity)
        {
            IActionResult response = Unauthorized();
            ResultEntity<ItemMasters> resEntity = await itemMaster.Update(entity);
            response = Ok(new
            {
                Result = resEntity
            });
            return response;
        }
        [HttpDelete("Delete/ItemMaster")]
        public async Task<IActionResult> DeleteItemMaster([FromForm] Guid ItemID)
        {
            IActionResult response = Unauthorized();
            ResultEntity<bool> resEntity = await itemMaster.Delete(ItemID);
            response = Ok(new
            {
                Result = resEntity
            });
            return response;
        }
        [HttpGet("Search/ItemMaster")]
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
            ResultList<ItemMasters> resEntity = await itemMaster.SearchItem(gridParam);
            response = Ok(new
            {
                Result = resEntity
            });
            return response; 
        }
        [AllowAnonymous]
        [HttpGet("Lookups")]
        public async Task<IActionResult> GetItemMastersData(string culture = "en")
        {
            IActionResult response = Unauthorized();
            webExtention.SetCulture(culture);
            ResultEntity<Dictionary<string, object>> resultEntity = new ResultEntity<Dictionary<string, object>>();
            Type[] DataTypes = new Type[] { typeof(ItemMasters) };
            resultEntity = await Task.Run(() => itemMaster.GetFormattedMasterDataByType(DataTypes, culture));
            response = Ok(new
            {
                Result = resultEntity
            });
            return response;
        }
    }
}
