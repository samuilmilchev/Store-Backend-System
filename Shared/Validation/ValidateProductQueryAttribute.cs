using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.DTOs;
using Shared.Enums;

namespace Shared.Validation
{
    public class ValidateProductQueryAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var queryDto = context.ActionArguments["queryData"] as ProductQueryDto;

            if (!Enum.IsDefined(typeof(SortBy), queryDto.SortBy) ||
            !Enum.IsDefined(typeof(SortDirection), queryDto.SortDirection))
            {
                context.Result = new BadRequestObjectResult("Invalid sort parameters.");
            }
        }
    }
}
