using System;
using System.Collections.Generic;
using System.Text;

namespace Entity.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ParentTableMapping : Attribute
    {
        public ParentTableMapping(string tableName, Type _SourceType)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Empty collection name not allowed", "value");
            TableName = tableName;
            SourceType = _SourceType;
        }
        public virtual string TableName { get; private set; }
        public virtual Type SourceType { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RelatedPropertyAttribute : Attribute
    {
        public string RelatedProperty { get; private set; }
        public RelatedPropertyAttribute(string PropertyName)
        {
            RelatedProperty = PropertyName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class GlobleSearch : Attribute
    {
        public bool IsParent { get; private set; }
        public GlobleSearch(bool isParent = false)
        {
            IsParent = isParent;
        }

    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class PropertySearch : Attribute
    {
        public PropertySearch()
        {
        }

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GeneratedControllerAttribute : Attribute
    {
        public GeneratedControllerAttribute(string route, string tagName = "Lookups")
        {
            Route = route;
            TagName = tagName;
        }
        public string Route { get; set; }
        public string ICRoute { get; set; }
        public string TagName { get; set; }
        public bool AddICAPIs { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EnglishNameAttribute : Attribute
    {
        public string RelatedProperty { get; private set; }
        public EnglishNameAttribute(string PropertyName)
        {
            RelatedProperty = PropertyName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ArabicNameAttribute : Attribute
    {
        public string RelatedProperty { get; private set; }
        public ArabicNameAttribute(string PropertyName)
        {
            RelatedProperty = PropertyName;
        }
    }
}
