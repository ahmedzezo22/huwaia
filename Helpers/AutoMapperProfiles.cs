using System.Linq;
using AutoMapper;
using ZawagProject.DTO;
using ZawagProject.Models;

namespace ZawagProject.Helpers
{
    public class AutoMapperProfiles :Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User,UsersForDetailsDto>()
         .ForMember(dest=>dest.PhotoUrl,op=>{op.MapFrom(src=>src.Photos.FirstOrDefault(x=>x.IsMain).Url);})
         .ForMember(dest=>dest.Age,op=>{op.ResolveUsing(src=>src.DateofBirth.CalculateAge());});

            CreateMap<User,UsersListDto>()
            .ForMember(dest=>dest.PhotoUrl,op=>{op.MapFrom(src=>src.Photos.FirstOrDefault(x=>x.IsMain).Url);})
            .ForMember(dest=>dest.Age,op=>{op.ResolveUsing(src=>src.DateofBirth.CalculateAge());});

            CreateMap<Photo,PhotoForDetailsDto>();
            CreateMap<UserForUpdateDto,User>();
            CreateMap<Photo,PhotoReturn>();
            CreateMap<PhotoForCreateDto,Photo>();
            CreateMap<UserForRegisterDto,User>();
            CreateMap<MessageForCreationDto,Message>().ReverseMap();
            CreateMap<Message,MessagesToReturnDto>()
            .ForMember(dest=>dest.SenderPhotoUrl,op=>{op.MapFrom(src=>src.Sender.Photos.FirstOrDefault(x=>x.IsMain).Url);})
            .ForMember(dest=>dest.RecepientPhotoUrl,op=>{op.MapFrom(src=>src.Recepient.Photos.FirstOrDefault(x=>x.IsMain).Url);});

            
        }
    }
}