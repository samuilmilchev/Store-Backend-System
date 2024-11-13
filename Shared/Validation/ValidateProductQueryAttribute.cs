using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.DTOs;

namespace Shared.Validation
{
    public class ValidateProductQueryAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var queryDto = context.ActionArguments["queryData"] as ProductQueryDto;
            if (queryDto == null) return;

            var validSortBy = new[] { "Rating", "Price" };
            var validSortDirection = new[] { "Asc", "Desc" };

            if (!validSortBy.Contains(queryDto.SortBy, StringComparer.OrdinalIgnoreCase) ||
                !validSortDirection.Contains(queryDto.SortDirection, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new BadRequestObjectResult("Invalid sort parameters.");
            }
        }
    }
}
