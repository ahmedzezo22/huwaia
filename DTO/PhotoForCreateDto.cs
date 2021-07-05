using System;
using Microsoft.AspNetCore.Http;

namespace ZawagProject.DTO
{
    public class PhotoForCreateDto
    {
        public PhotoForCreateDto()
        {
            DateAdded=DateTime.Now;
            
        }
        public string Url { get; set; }

        public IFormFile File { get; set; }

        public string Description { get; set; }

        public DateTime DateAdded { get; set; }

        public string PublicId { get; set; }
    }
}