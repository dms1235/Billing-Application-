using System;
using System.Collections.Generic;
using System.Text;

namespace Entity.Common.WebExtention
{
    public enum ItemOrigin
    {
        Header = 0,
        Cookies = 1,
        Session = 2,
        FormData = 3
    }

    public class HttpItem
    {
        public HttpItem() { }
        public ItemOrigin Origin { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
    }
    public class HttpItemCollection : List<HttpItem>
    {
        public HttpItemCollection(IEnumerable<HttpItem> httpItems) { }
    }
}
