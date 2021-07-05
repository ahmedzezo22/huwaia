using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ZawagProject.Data;
using ZawagProject.DTO;
using ZawagProject.Helpers;
using ZawagProject.Models;

namespace ZawagProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users/{userId}/photo")]
     
    public class PhotoController : ControllerBase
    {
        private readonly IZawajRepository _repo;
        private readonly IOptions<CloudinarySettings> _cloudnryConfg;
        private readonly IMapper _mapper;
        private Cloudinary _cloudnary;

        public PhotoController(IZawajRepository repo, IOptions<CloudinarySettings> cloudnryConfg, IMapper mapper)
        {
            _repo = repo;
            _cloudnryConfg = cloudnryConfg;
            _mapper = mapper;
            Account account = new Account(
                _cloudnryConfg.Value.CloudName,
                _cloudnryConfg.Value.APIKey,
                _cloudnryConfg.Value.APISecret
            );
            _cloudnary = new Cloudinary(account);
        }


         [HttpGet("{id:int}",Name=nameof(GetPhoto))]
      public async Task<IActionResult> GetPhoto(int id){
           var photoFromRepo=await _repo.GetPhoto(id);
           var photoToReturn=_mapper.Map<PhotoReturn>(photoFromRepo);
           return Ok(photoToReturn);
        }
        

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreateDto photoForCreateDto)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var UserFromRepo = await _repo.GetUser(userId,true);
            var file = photoForCreateDto.File;
            var uploadResult = new ImageUploadResult();
            if (file != null && file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudnary.Upload(uploadParams);
                }
            }
            photoForCreateDto.Url = uploadResult.Uri.ToString();
            photoForCreateDto.PublicId = uploadResult.PublicId;
            var photo = _mapper.Map<Photo>(photoForCreateDto);
            if (!UserFromRepo.Photos.Any(x => x.IsMain))
            {
                photo.IsMain = true;
            }
            UserFromRepo.Photos.Add(photo);
            if (await _repo.SaveAll())
            {
                var photoToReturn=_mapper.Map<PhotoReturn>(photo);
                return  CreatedAtRoute("GetPhoto",new { userId,id = photo.Id},photoToReturn);
                 //return Ok(photoToReturn);
             
            }
            return BadRequest("خطأ فى رفع الصوره");

        }
        [HttpPost("{id}/setMain")]
    public async Task<IActionResult> setMainPhoto(int userId,int id){
        if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var UserFromRepo = await _repo.GetUser(userId,true);
            if(!UserFromRepo.Photos.Any(p=>p.Id==id)){
                return Unauthorized();
            }
            var DesiredMainPhoto=await _repo.GetPhoto(id);
            if(DesiredMainPhoto.IsMain){
                return BadRequest("هذه الصوره الرْيسيه بالفعل");
            }
            var currentMainPhoto=await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain=false;
            DesiredMainPhoto.IsMain=true;
            if(await _repo.SaveAll()){
                return NoContent();
            }
            return BadRequest("لايمكن تعديل الصوره الرْيسيه");
            
    }
    [HttpDelete("{id}")]
    

    public async Task<IActionResult> DeletePhoto(int userId,int id){
           if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var UserFromRepo = await _repo.GetUser(userId,true);
            if(!UserFromRepo.Photos.Any(p=>p.Id==id)){
                return Unauthorized();
            }
            var photo=await _repo.GetPhoto(id);
            if(photo.IsMain){
                return BadRequest(" لايمكن حذف الصوره الرءيسيه");
            }
            if(photo.PublicId !=null){
                var DeletionParam=new DeletionParams(photo.PublicId);
                var result=this._cloudnary.Destroy(DeletionParam);
                if(result.Result=="ok"){
                    _repo.Delete(photo);
                }
            }
            if(photo.PublicId==null){ _repo.Delete(photo);}
            if(await _repo.SaveAll()) return Ok();
            return BadRequest("فشل حذف الصوره");
            
    }
    }
}