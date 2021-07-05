using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZawagProject.Data;
using ZawagProject.DTO;
using ZawagProject.Helpers;
using Microsoft.Extensions.Options;
using ZawagProject.Models;
using Stripe;
using System;
using DinkToPdf;
using System.IO;
using DinkToPdf.Contracts;

namespace ZawagProject.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly IZawajRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<StripeSettings> _StripeSettings;
        private readonly IConverter _converter;

        public UsersController(IZawajRepository repo, IMapper mapper, IOptions<StripeSettings> StripeSettings, IConverter converter)
        {
            _converter = converter;
            _StripeSettings = StripeSettings;
            _repo = repo;
            _mapper = mapper;
        }

        // public async Task<IActionResult> GetUsers(){
        //     var users=await _repo.GetUsers();
        //     var UsersToReturn=_mapper.Map<IEnumerable<UsersListDto>>(users);
        //     return Ok(UsersToReturn);
        // }
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UsersParam usersParam)
        {
            var currnetUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);//filtering
            var userFromRepo = await _repo.GetUser(currnetUserId, true);
            usersParam.userId = currnetUserId;
            if (string.IsNullOrEmpty(usersParam.Gender))
            {
                usersParam.Gender = userFromRepo.Gender == "رجل" ?
                 "إمرأة" :
                 "رجل";
            }
            var users = await _repo.GetUsers(usersParam);
            var UsersToReturn = _mapper.Map<IEnumerable<UsersListDto>>(users);
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPage);
            return Ok(UsersToReturn);
        }
        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var isCurrentUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) == id;
            var user = await _repo.GetUser(id, isCurrentUser);
            var UserToReturn = _mapper.Map<UsersForDetailsDto>(user);
            return Ok(UserToReturn);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var UserFromRepo = await _repo.GetUser(id, true);
            _mapper.Map(userForUpdateDto, UserFromRepo);

            if (await _repo.SaveAll())
            {
                return NoContent();
            }
            throw new System.Exception($"حدثت مشكله في تعديل بيانات المشترك رقم {id}");
        }
        [HttpPost("{id}/like/{receipientId}")]
        public async Task<IActionResult> LikeUser(int id, int receipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var Like = await _repo.GetLike(id, receipientId);
            if (Like != null)
            {
                return BadRequest("لقد قمت بالاعجاب بهذا الشخص من قبل");
            }
            if (await _repo.GetUser(receipientId, false) == null)
            {
                return NotFound();
            }
            Like = new Models.Like
            {
                LikerId = id,
                LikeeId = receipientId
            };
            _repo.Add<Models.Like>(Like);
            if (await _repo.SaveAll()) return Ok();
            return BadRequest("فشل الاعجاب");
        }

        //payment method
        [HttpPost("{userId}/charge/{stripeToken}")]
        public async Task<IActionResult> Charge(int userId, string stripeToken)

        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var customers = new CustomerService();
            var charges = new ChargeService();

            // var options = new TokenCreateOptions
            // {
            // Card = new CreditCardOptions
            //     {
            //         // Number = "4242424242424242",
            //         // ExpYear = 2020,
            //         // ExpMonth = 3,
            //         // Cvc = "123"
            //     }
            // };

            // var service = new TokenService();
            // Token stripeToken = service.Create(options);

            var customer = customers.Create(new CustomerCreateOptions
            {
                Source = stripeToken
            });

            var charge = charges.Create(new ChargeCreateOptions
            {
                Amount = 5000,
                Description = "إشتراك مدى الحياة",
                Currency = "usd",
                Customer = customer.Id,

            });

            var payment = new Payment
            {
                PaymentDate = DateTime.Now,
                Amount = charge.Amount / 100,
                UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value),
                ReceiptUrl = charge.ReceiptUrl,
                Description = charge.Description,
                Currency = charge.Currency,
                IsPaid = charge.Paid
            };
            _repo.Add<Payment>(payment);
            if (await _repo.SaveAll())
            {
                return Ok(new { IsPaid = charge.Paid });
            }

            return BadRequest("فشل في السداد");

        }
        [HttpGet("{userId}/payment")]

        public async Task<IActionResult> GetPaymentForUser(int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var payment = await _repo.GetPaymentForUser(userId);
            return Ok(payment);
        }
        //print reports
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("UserReport/{userId}")]
        public IActionResult CreatePdfForUser(int userId)
        {
            var templateGenerator = new TemplateGenerator(_repo, _mapper);
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 15, Bottom = 20 },
                DocumentTitle = "بطاقة مشترك"

            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = templateGenerator.GetHTMLStringForUser(userId),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","assets", "style.css") },
                HeaderSettings = { FontName = "Impact", FontSize = 12, Spacing = 5, Line = false },
                FooterSettings = { FontName = "Geneva", FontSize = 15, Spacing = 7, Line = true, Center = "Huwaia By Eng Ahmed Mourad", Right = "[page]" }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);
            return File(file, "application/pdf");
        }
        //Get all users Except Admin
         [Authorize(Policy="RequireAdminRole")]
         [HttpGet("GetAllUsersExceptAdmin")]
         public async Task<IActionResult> GetAllUsersExceptAdmin(){
             var users=await _repo.GetAllUsersExceptAdmin();
             var UserToReturn=_mapper.Map<IEnumerable<UsersForDetailsDto>>(users);
             return Ok(UserToReturn);
         }
    }
}