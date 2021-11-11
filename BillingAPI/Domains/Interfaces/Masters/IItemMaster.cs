using Entity.Common.Entities;
using Entity.Entities.Masters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domains.Interfaces.Masters
{
    public interface IItemMaster
    {
        Task<ResultEntity<ItemMasters>> Add(ItemMasters entity);
        Task<ResultEntity<ItemMasters>> Update(ItemMasters entity);
        Task<ResultEntity<bool>> Delete(Guid ItemID);
        Task<ResultList<ItemMasters>> GetAllItems();
        Task<ResultEntity<ItemMasters>> GetItemByID(Guid ItemID);
        Task<ResultList<ItemMasters>> SearchItem(GridParameters gridParameters);
        ResultEntity<Dictionary<string, object>> GetFormattedMasterDataByType(Type[] DataTypes, string Culture);
    }
}
