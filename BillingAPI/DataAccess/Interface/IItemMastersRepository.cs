using Entity.Common.Entities;
using Entity.Entities.Masters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IItemMastersRepository
    {
        Task<ResultEntity<ItemMasters>> InsertItemMasters(ItemMasters entity);
        Task<ResultEntity<ItemMasters>> UpdateItemMasters(ItemMasters entity);
        Task<ResultEntity<bool>> DeleteItemMasters(Guid ItemID);
        Task<ResultList<ItemMasters>> GetAllItemMasters();
        Task<ResultEntity<ItemMasters>> GetItemByID(Guid ItemID);
        Task<ResultList<ItemMasters>> SearchItemMaster(GridParameters gridParams, Expression<Func<ItemMasters, bool>> expression = null);
    }
}
