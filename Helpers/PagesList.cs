using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace ZawagProject.Helpers
{
    public class PagesList<T> :List<T>
    {
        public int CurrentPage { get; set; }

        public int TotalPage { get; set; }

        public int PageSize { get; set; }
        
        public int TotalCount { get; set; }
        public PagesList(List<T> items,int count,int pageNumber,int pageSize)
        {
            TotalCount=count;
            CurrentPage=pageNumber;
            PageSize=pageSize;
            TotalPage=(int)Math.Ceiling(count/(double)pageSize);
            this.AddRange(items);

        }
          public static async Task<PagesList<T>> createAsync(IQueryable<T> source,int pageNumber,int pageSize){
             //list of users or messages in db
           var count=await source.CountAsync();
           var items=await source.Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync();//delegare Query
           return new PagesList<T>(items,count,pageNumber,pageSize);
             
          }
    }
}