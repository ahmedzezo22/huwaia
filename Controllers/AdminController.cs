using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ZawagProject.Data;
using ZawagProject.DTO;
using ZawagProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using ZawagProject.Helpers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ZawagProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        private readonly IOptions<CloudinarySettings> _cloudnryConfg;

        private readonly DataContext _context;
        private Cloudinary _cloudnary;

        public AdminController(DataContext context, UserManager<User> userManager, IOptions<CloudinarySettings> cloudnryConfg, RoleManager<Role> roleManager)
        {
            _cloudnryConfg = cloudnryConfg;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            Account account = new Account(
               _cloudnryConfg.Value.CloudName,
               _cloudnryConfg.Value.APIKey,
               _cloudnryConfg.Value.APISecret
           );
            _cloudnary = new Cloudinary(account);
        }
        [Authorize(Policy = "RequireAdminRole")]

        [HttpGet("role")]
        public async Task<IActionResult> GetUserWithRoles()
        {
            var userList = await (from user in _context.Users
                                  orderby user.UserName
                                  select new
                                  {
                                      Id = user.Id,
                                      UserName = user.UserName,
                                      Roles = (from userRole in user.userRole
                                               join role in _context.Roles
                                               on userRole.RoleId equals role.Id
                                               select role.Name).ToList()
                                  }

            ).ToListAsync();
            return Ok(userList);
        }
        [Authorize(Policy = "RequireAdminRole")]

        [HttpPost("editroles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName,[FromBody] RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if(await _roleManager.RoleExistsAsync("Member")||await _roleManager.RoleExistsAsync("Admin")||await _roleManager.RoleExistsAsync("Moderator")
            ||await _roleManager.RoleExistsAsync("VIP"))
            {
            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = roleEditDto.RoleNames;
            selectedRoles = selectedRoles ?? new string[] { };
            
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded)
                return BadRequest("حدث خطأ أثناء إضافة الأدوار");
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded)
                return BadRequest(result.Errors);
             }
            return Ok(await _userManager.GetRolesAsync(user));
        }
        //photo mangment

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            var photos = await _context.Photos
                .Include(u => u.User)
                .IgnoreQueryFilters()
                .Where(p => p.IsApproved == false)
                .Select(u => new
                {
                    Id = u.Id,
                    UserName = u.User.UserName,
                    KnownAs = u.User.KnownAs,
                    Url = u.Url,
                    IsApproved = u.IsApproved
                }).ToListAsync();

            return Ok(photos);
        }
        //approve photo 

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approvePhoto/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _context.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == photoId);

            photo.IsApproved = true;

            await _context.SaveChangesAsync();

            return Ok();
        }
        //reject photo
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            var photo = await _context.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo.IsMain)
                return BadRequest("لا يمكنك رفض الصورة الأساسية");

            if (photo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photo.PublicId);

                var result = _cloudnary.Destroy(deleteParams);

                if (result.Result == "ok")
                {
                    _context.Photos.Remove(photo);
                }
            }

            if (photo.PublicId == null)
            {
                _context.Photos.Remove(photo);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }

}
