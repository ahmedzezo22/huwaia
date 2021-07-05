using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ZawagProject.Helpers
{
    public static class Extension
    {
        public static void AddApplicationError(this HttpResponse response,string message ){
            response.Headers.Add("Application-Error",message);
            response.Headers.Add("Access-Control-Expose-Headers","Application-error");
            response.Headers.Add("Access-control-Allow-origin","*");
        }
        public static int CalculateAge(this DateTime dateTime){
            var age=DateTime.Today.Year-dateTime.Year;
            if(dateTime.AddYears(age)>DateTime.Today){
                age--;
            }
            return age;
        }
        //add pagination
        public static void AddPagination(this HttpResponse response,int currnetPage,int itemsPerPage,int totalPages,int totalItems){
            var paginationHeader=new PaginationHeaders(currnetPage,itemsPerPage,totalPages,totalItems);
             var camelCaseFormatter=new JsonSerializerSettings();

            camelCaseFormatter.ContractResolver=new CamelCasePropertyNamesContractResolver();
            
            response.Headers.Add("Pagination",JsonConvert.SerializeObject(paginationHeader,camelCaseFormatter));
             response.Headers.Add("Access-Control-Expose-Headers","Pagination");
          
        }
    }
}