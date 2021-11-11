using Entity.Base;
using Entity.Entities.Masters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Entity.Entities.DTO
{
    public class MasterItemDTO
    {
        public static MasterItemDTO FromLookup<T>(T entity, string Culture = "en") where T : BaseLookupEntity
        {
            MasterItemDTO obj = new MasterItemDTO();
            obj.ID = entity.ID;
            obj.IsEnabled = entity.IsEnabled;
            obj.Code = entity.Code;
            obj.Name = entity.Name;
            return obj;
        }

        public Guid ID { set; get; }
        public string Code { set; get; }
        public string Name { set; get; }
        public bool IsEnabled { set; get; }
    }
    public class MasterDTODictionary : Dictionary<string, object>
    {
        public static MasterDTODictionary FromLookup<T>(T entity, string Culture = "en") where T : BaseLookupEntity
        {
            MasterDTODictionary obj = new MasterDTODictionary();
            obj.Add("ID", entity.ID);
            obj.Add("IsEnabled", entity.IsEnabled);
            obj.Add("Code", entity.Code);
            obj.Add("Name", entity.Name);
            try
            {
                foreach (var Property in typeof(T).GetProperties().Where(x => x.GetCustomAttribute<ForeignKeyAttribute>() != null))
                {
                    obj.Add(Property.Name, Property.GetValue(entity));
                }

                //if (typeof(T) == typeof(ItemMasters))
                //{
                //    var Property = typeof(T).GetProperties().FirstOrDefault(x => x.Name == "");
                //    obj.Add(Property.Name, Property.GetValue(entity));
                //}
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
            }
            return obj;
        }

        public static MasterDTODictionary FromEntity<T>(T entity, string Culture = "en") where T : BaseEntity
        {
            MasterDTODictionary obj = new MasterDTODictionary();
            obj.Add("ID", entity.ID);
            try
            {
                foreach (var Property in typeof(T).GetProperties().Where(x => x.GetCustomAttribute<ForeignKeyAttribute>() != null))
                {
                    obj.Add(Property.Name, Property.GetValue(entity));
                }
            }
            catch
            {
            }
            return obj;
        }
    }
}
