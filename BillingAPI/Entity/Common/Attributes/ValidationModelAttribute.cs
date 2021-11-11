using Entity.Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Net;
using Entity.Enums;

namespace Entity.Common.Attributes
{
    public class ValidateModelAttribute : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ModelState.IsValid)
            {
                var errors = filterContext.ModelState.Values.Where(v => v.Errors.Count > 0)
                      .SelectMany(v => v.Errors)
                      .Select(v => v.ErrorMessage).Distinct()
                      .ToList();
                string ErrorResult = string.Join(", ", errors);
                ResultEntity<object> resultEntity = new ResultEntity<object>()
                {
                    Status = (int)ResponseStatus.ValidationError,
                    MessageEnglish = ErrorResult,
                    Entity = null
                };
                filterContext.Result = new JsonResult(resultEntity)
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
        }
    }

    public class ValidateMobileModelAttribute : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ModelState.IsValid)
            {
                var errors = filterContext.ModelState.Values.Where(v => v.Errors.Count > 0)
                      .SelectMany(v => v.Errors)
                      .Select(v => v.ErrorMessage).Distinct()
                      .ToList();
                string ErrorResult = string.Join(", ", errors);
                ResultEntity<object> resultEntity = new ResultEntity<object>()
                {
                    Status = (int)ResponseStatus.ValidationError,
                    MessageEnglish = ErrorResult,
                    Entity = null
                };

                filterContext.Result = new JsonResult(new
                {
                    Result = resultEntity
                })
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
        }
    }
}
