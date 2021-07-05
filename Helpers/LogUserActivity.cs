using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Threading.Tasks;
using ZawagProject.Data;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ZawagProject.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public  async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //any action user entered
            var resultContext=await next();
            var userId= int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var repo=resultContext.HttpContext.RequestServices.GetService<IZawajRepository>();
            var user=await repo.GetUser(userId,true);
            user.LastActive=DateTime.Now;
            await repo.SaveAll();
        }


    }
}