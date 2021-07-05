using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZawagProject.Data;
using ZawagProject.DTO;
using ZawagProject.Helpers;
using ZawagProject.Models;
using System;

namespace ZawagProject.Controllers
{


    [ServiceFilter(typeof(LogUserActivity))]
    // [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {

        private readonly IZawajRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IZawajRepository repo, IMapper mapper)
        {

            _repo = repo;
            _mapper = mapper;
        }
        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var MessageFromRepo = await _repo.GetMessage(id);
            if (MessageFromRepo == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(MessageFromRepo);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender=await _repo.GetUser(userId,true);
            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            messageForCreationDto.SenderId = userId;
            var recepient = await _repo.GetUser(messageForCreationDto.RecepientId,false);
            if (recepient == null) { return BadRequest("لم يتم الوصول للمرسل اليه"); }
            var message = _mapper.Map<Message>(messageForCreationDto); //destination(source)

            _repo.Add(message);
           
            if (await _repo.SaveAll()){ var MessageToRerurn = _mapper.Map<MessagesToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { userId, id = message.Id }, MessageToRerurn);}
            // return Ok(MessageToRerurn);
            throw new System.Exception("حدث خطـأ ما");
        }
        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery] MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            messageParams.userId = userId;
            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessagesToReturnDto>>(messagesFromRepo);
            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalCount
            , messagesFromRepo.TotalPage);
            return Ok(messages);
        }

        [HttpGet("chat/{recepientId}")]
        public async Task<IActionResult> GetConversation(int userId, int recepientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var conFromRepo=await _repo.GetConversation(userId,recepientId);
            var MessageToRerurn=_mapper.Map<IEnumerable<MessagesToReturnDto>>(conFromRepo);
            return Ok(MessageToRerurn);
        }
        // get count of unread messages
        [HttpGet("count")]
        public async Task<IActionResult> GetUnReadMessagesForUser(int userId){
         if(userId !=int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
         return Unauthorized();
         var count=await _repo.GetUnReadMessagesForUser(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
          return Ok(count);
        }
       [HttpPost("read/{id}")]
        public async Task<IActionResult> MarkMessageAsRead(int userId,int id){
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
             return Unauthorized();
             var message = await _repo.GetMessage(id);
             if(message.RecepientId != userId)
                 return Unauthorized();
            message.IsRead = true;
            message.DateRead=DateTime.Now;
            await _repo.SaveAll();
            return NoContent();
       }				   
       //delete message
         [HttpPost("{id}")]
         public async Task<IActionResult> DeleteMessage(int id,int userId){
               if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
             return Unauthorized();
             var message=await _repo.GetMessage(id);
             if(message.SenderId==userId)
             message.SenderDeleted=true;
             if(message.RecepientId==userId)
             message.RecepientDeleted=true;
             if(message.SenderDeleted&&message.RecepientDeleted==true)
                 _repo.Delete(message);
                 if(await _repo.SaveAll()){
                     return NoContent();
                 }
                 throw new Exception("حدث خطأ ما");
             }
    
         }


    }

