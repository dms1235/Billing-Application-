using Entity.Base;
using Entity.Common.Attributes;
using Entity.Common.Logs;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Entity.Common.Helper
{
    public static class ExtentionMethods
    {

        public static byte[] ToByteArray(this IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                formFile.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        public static void CopyProperties<TSource, TDestination>(this TSource source, TDestination destination)
        {
            if (destination == null)
            {
                destination = (TDestination)Activator.CreateInstance(typeof(TDestination));
            }
            PropertyInfo[] destinationProperties = destination.GetType().GetProperties();
            foreach (PropertyInfo destinationPi in destinationProperties)
            {
                try
                {
                    if (source != null)
                    {
                        PropertyInfo sourcePi = source.GetType().GetProperty(destinationPi.Name);
                        if (sourcePi != null)
                        {
                            destinationPi.SetValue(destination, sourcePi.GetValue(source, null), null);
                        }
                    }

                }
                catch
                {
                }
            }
        }

        public static byte[] ToBytes(this Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static string ToBase64(this Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
            }
        }

        public static string ToMessageString(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
                sb.AppendLine($"Method-> {trace.GetFrame(0).GetMethod().ReflectedType.FullName}");
                sb.AppendLine($"LineNumber-> {trace.GetFrame(0).GetFileLineNumber().ToString()}");
                sb.AppendLine($"Column-> {trace.GetFrame(0).GetFileColumnNumber().ToString()}");
            }
            catch
            {
            }

            sb.AppendLine($"Message-> {ex.Message}");
            if (ex.InnerException != null)
            {
                sb.AppendLine($"InnerMessage-> {ex.InnerException.Message}");
            }
            sb.AppendLine($"StackTrace-> {ex.StackTrace}");
            return sb.ToString();
        }

        public static void ParentLookupdataMap<T>(T entity, object DomainInstance, string culture)
        {
            try
            {
                foreach (var Property in typeof(T).GetProperties().Where(x => x.GetCustomAttribute<ParentTableMapping>() != null))
                {
                    Type ParentType = Property.GetCustomAttribute<ParentTableMapping>().SourceType;
                    if (Property.GetCustomAttribute<RelatedPropertyAttribute>() != null)
                    {
                        string RelatedProperty = Property.GetCustomAttribute<RelatedPropertyAttribute>().RelatedProperty;
                        var Id = entity.GetType().GetProperty(RelatedProperty).GetValue(entity);
                        if (Id != null)
                        {
                            Type ex = DomainInstance.GetType();
                            MethodInfo mi = ex.GetMethod("GetLookupByID");
                            MethodInfo miConstructed = mi.MakeGenericMethod(ParentType);
                            object[] args = { new Guid(Id.ToString()) };
                            var resObj = miConstructed.Invoke(DomainInstance, args);
                            if (resObj != null)
                            {
                                var ParentObj = resObj as BaseLookupEntity;
                                if (ParentObj != null)
                                {
                                    entity.GetType().GetProperty(Property.Name).SetValue(entity, ParentObj.Name);
                                    var ParentPropety = typeof(T).GetProperties().FirstOrDefault(x => x.PropertyType == ParentObj.GetType());
                                    if (ParentPropety != null)
                                    {
                                        entity.GetType().GetProperty(ParentPropety.Name).SetValue(entity, ParentObj);
                                    }
                                }
                            }
                        }
                    }


                }
            }
            catch
            {

            }
        }
        public static LoggerConfiguration WithHttpRequestId(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<HttpRequestIdEnricher>();
        }
        public static string GetFormattedString(this IHeaderDictionary dictionary)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var header in dictionary.Where(p => p.Key != "Authorization"))
            {
                sb.AppendLine($"{header.Key}:{header.Value}");
            }
            return sb.ToString();
        }

        public static Expression<Func<TEntity, bool>> Dynamicsearchfilterforentity<TEntity>(string value, string culture = "en")
        {
            var p = Expression.Parameter(typeof(TEntity), "p");
            Expression<Func<TEntity, bool>> ex = null;
            Expression expression = null;
            Expression body = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                foreach (var Property in typeof(TEntity).GetProperties().Where(x => x.GetCustomAttribute<GlobleSearch>() != null))
                {
                    try
                    {
                        var isParentPropert = Property.GetCustomAttribute<GlobleSearch>().IsParent;
                        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
                        var valueToFind = Expression.Constant(value);
                        ParentTableMapping tableMapping = Property.GetCustomAttributes<ParentTableMapping>().FirstOrDefault();
                        if (tableMapping != null)
                        {
                            PropertyInfo ParentPro = typeof(TEntity).GetProperties().Where(p => p.PropertyType == tableMapping.SourceType).FirstOrDefault();
                            if (ParentPro != null)
                            {
                                string name = "Name";
                                string ProName = $"{ParentPro.Name}.{name}";
                                var propertyToSearch = ProName.Split('.').Aggregate<string, Expression>(p, Expression.PropertyOrField);
                                expression = Expression.Call(propertyToSearch, containsMethod, valueToFind);
                                body = body == null ? expression : Expression.Or(body, expression);
                            }
                        }
                        else
                        {
                            var propertyToSearch = isParentPropert ?
                          GetLastPropertyExpression<TEntity>(p, Property.Name, "Name")
                           : Expression.Property(p, Property.Name);
                            if (propertyToSearch.Type == typeof(string))
                            {
                                containsMethod = Property.PropertyType.GetMethod(nameof(string.Contains), new[] { typeof(string) });
                                expression = Expression.Call(propertyToSearch, containsMethod, valueToFind);
                                body = body == null ? expression : Expression.Or(body, expression);
                            }
                            else
                            {
                                expression = GetExpression(propertyToSearch, Property, value);
                                if (expression != null)
                                {
                                    body = body == null ? expression : Expression.Or(body, expression);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                ex = Expression.Lambda<Func<TEntity, bool>>(body, p);
            }
            return ex;
        }
        private static Expression GetLastPropertyExpression<TSource>(ParameterExpression pe, params string[] properties)
        {
            Expression lastMember = pe;
            for (int i = 0; i < properties.Length; i++)
            {
                MemberExpression member = Expression.Property(lastMember, properties[i]);
                lastMember = member;
            }
            return lastMember;
        }
        public static Expression GetExpression(Expression propertyExpression, PropertyInfo Property, string ValuetoSearch)
        {
            ConstantExpression searchExpression = null;
            MethodInfo containsMethod = null;
            Expression body = null;
            switch (Type.GetTypeCode(Property.PropertyType))
            {
                case TypeCode.Object:
                    if (Property.PropertyType == typeof(Guid))
                    {
                        searchExpression = Expression.Constant(Guid.Parse(ValuetoSearch));
                        containsMethod = typeof(Guid).GetMethod("Equals", new[] { typeof(Guid) });
                    }
                    else if (Property.PropertyType == typeof(Guid?))
                    {
                        searchExpression = Expression.Constant(Guid.Parse(ValuetoSearch));
                        propertyExpression = Expression.Property(propertyExpression, "Value");
                        containsMethod = typeof(Guid).GetMethod("Equals", new[] { typeof(Guid) });
                    }
                    else
                    {
                        searchExpression = Expression.Constant(ValuetoSearch.ToString());
                        containsMethod = typeof(object).GetMethod("Equals", new[] { typeof(object) });
                    }

                    body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    break;
                case TypeCode.String:
                    searchExpression = Expression.Constant(ValuetoSearch.ToString());
                    containsMethod = typeof(string).GetMethod("Equals", new[] { typeof(string) });
                    body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    break;
                case TypeCode.Byte:
                    byte _byte;
                    if (byte.TryParse(ValuetoSearch, out _byte))
                    {
                        searchExpression = Expression.Constant(_byte);
                        containsMethod = typeof(byte).GetMethod("Equals", new[] { typeof(byte) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Int16:
                    short _int16;
                    if (short.TryParse(ValuetoSearch, out _int16))
                    {
                        searchExpression = Expression.Constant(_int16);
                        containsMethod = typeof(short).GetMethod("Equals", new[] { typeof(short) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Int32:
                    int _int;
                    if (int.TryParse(ValuetoSearch, out _int))
                    {
                        searchExpression = Expression.Constant(_int);
                        containsMethod = typeof(int).GetMethod("Equals", new[] { typeof(int) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Int64:
                    long _int64;
                    if (long.TryParse(ValuetoSearch, out _int64))
                    {
                        searchExpression = Expression.Constant(_int64);
                        containsMethod = typeof(long).GetMethod("Equals", new[] { typeof(long) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Single:
                    float _Single;
                    if (float.TryParse(ValuetoSearch, out _Single))
                    {
                        searchExpression = Expression.Constant(_Single);
                        containsMethod = typeof(float).GetMethod("Equals", new[] { typeof(float) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Double:
                    double _Double;
                    if (double.TryParse(ValuetoSearch, out _Double))
                    {
                        searchExpression = Expression.Constant(_Double);
                        containsMethod = typeof(double).GetMethod("Equals", new[] { typeof(double) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Boolean:
                    bool _Boolean;
                    if (bool.TryParse(ValuetoSearch, out _Boolean))
                    {
                        searchExpression = Expression.Constant(_Boolean);
                        containsMethod = typeof(bool).GetMethod("Equals", new[] { typeof(bool) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                default:
                    body = null;
                    break;
            }
            return body;
        }

        public static bool ValidateAppPasswordPolicy(this string Password)
        {
            var input = Password;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            var passwordRegEx = new Regex("^(?=.*?[A-Za-z])(?=.*?[0-9])(?=.*?[#?!@$%^&<>*~:`-]).{8,16}$");
            if (!passwordRegEx.IsMatch(input))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
