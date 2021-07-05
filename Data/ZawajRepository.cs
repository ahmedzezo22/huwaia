using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZawagProject.Models;
using System.Linq;
using ZawagProject.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;

namespace ZawagProject.Data
{
    
    public class ZawajRepository : IZawajRepository
    {
        private readonly DataContext _context;

        public ZawajRepository(DataContext context)
     {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
           _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public  async Task<Photo> GetPhoto(int id)
        {
           var photo =await _context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(p=>p.Id==id);
           return photo;
        }

       public  async Task<Photo>  GetMainPhotoForUser(int userId){
         return await _context.Photos.Where(u=>u.UserId==userId).FirstOrDefaultAsync(p=>p.IsMain);
       }
        public  async Task<User> GetUser(int id,bool isCurrentUser)
        {
           var query= _context.Users.Include(u=>u.Photos).AsQueryable();
           if(isCurrentUser){
              query=query.IgnoreQueryFilters();
           }
            // var user=await _context.Users.Include(u=>u.Photos).FirstOrDefaultAsync(x=>x.Id==id);
             var user=await query.FirstOrDefaultAsync(x=>x.Id==id);
            return user;
        }

        // public async Task<IEnumerable<User>> GetUsers()
        // {
        //    var users=await _context.Users.Include(u=>u.Photos).ToListAsync();
        //    return users;
        // }
         public async Task<PagesList<User>> GetUsers(UsersParam usersParam)
        {
           var users= _context.Users.Include(u=>u.Photos).OrderByDescending(u=>u.LastActive) .AsQueryable();
           users=users.Where(u=>u.Id!=usersParam.userId);
           users=users.Where(u=>u.Gender==usersParam.Gender);
        //    Age filtering
           if(usersParam.MinAge !=18 || usersParam.MaxAge!=99){
               var minDob=DateTime.Today.AddYears(-usersParam.MaxAge-1);
               var maxDob=DateTime.Today.AddYears(-usersParam.MinAge);
                 users=users.Where(u=>u.DateofBirth >=minDob && u.DateofBirth<=maxDob);
           }
             if(usersParam.Likers){
          var userLikers=await GetUserLikes(usersParam.userId,usersParam.Likers);
          users=users.Where(u=>userLikers.Contains(u.Id));
              }
               if(usersParam.Likees==true){
                    var userLikees=await GetUserLikes(usersParam.userId,usersParam.Likers);
                   users=users.Where(u=>userLikees.Contains(u.Id));
               }
           if(!string.IsNullOrEmpty(usersParam.OrderBy)){
              switch (usersParam.OrderBy)
              {
                 case "created":
                 users=users.OrderByDescending(u=>u.CreatedAt);
                    break;
                 default:users=users.OrderByDescending(u=>u.LastActive);
                    break;
              }
            
           }

         
           return await PagesList<User>.createAsync(users,usersParam.PageNumber,usersParam.PageSize);
        }

        public async Task<bool> SaveAll()
        {
           return await _context.SaveChangesAsync()>0;
        }

        public async Task<Like> GetLike(int userId, int receipientId)
        {
             return await _context.Likes.FirstOrDefaultAsync(l=>l.LikerId==userId && l.LikeeId==receipientId);
        }
        private async Task<IEnumerable<int>>GetUserLikes(int id,bool Likers){
           var user=await _context.Users.Include(u=>u.Likers).Include(u=>u.Likees).FirstOrDefaultAsync(u=>u.Id==id);
          if(Likers){
             return user.Likers.Where(u=>u.LikeeId==id).Select(l=>l.LikerId);
          }else{
        return user.Likees.Where(u=>u.LikerId==id).Select(l=>l.LikeeId);

          }
        }

        public async Task<Message> GetMessage(int Id)
        {
           return await _context.Messages.FirstOrDefaultAsync(m=>m.Id==Id);
        }

        public async Task<PagesList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
           var messages=  _context.Messages.Include(m=>m.Sender).ThenInclude(u=>u.Photos)
           .Include(m=>m.Recepient).ThenInclude(u=>u.Photos).AsQueryable();
           switch (messageParams.MessageType)
           {
              case "Inbox":
              messages=messages.Where(m=>m.RecepientId==messageParams.userId && m.RecepientDeleted==false);
                 break;
                 case "OutBox":
                 messages=messages.Where(m=>m.SenderId==messageParams.userId && m.SenderDeleted==false);
                 break;
              default:
               messages=messages.Where(m=>m.RecepientId==messageParams.userId && m.IsRead==false &&m.RecepientDeleted==false);
                 break;
           }
           messages=messages.OrderByDescending(m=>m.MessageSent);
           return await PagesList<Message>.createAsync(messages,messageParams.PageNumber,messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetConversation(int userId, int receipientId)
        {
            var messages= await _context.Messages.Include(m=>m.Sender).ThenInclude(u=>u.Photos)
           .Include(m=>m.Recepient).ThenInclude(u=>u.Photos).Where(m=>m.RecepientId==userId &&m.RecepientDeleted==false && m.SenderId==receipientId||m.RecepientId==receipientId &&m.SenderDeleted==false&& m.SenderId==userId)
           .OrderBy(m=>m.MessageSent).ToListAsync();
           return messages;
        }

        public  async Task<int> GetUnReadMessagesForUser(int userId)
        {
           var messages= await _context.Messages.Where(m=>m.IsRead==false && m.RecepientId==userId).ToListAsync();
           var count=messages.Count();
           return count;
        }

        public async Task<Payment> GetPaymentForUser(int userId)
        {
            return await _context.Payments.FirstOrDefaultAsync(m=>m.UserId==userId);
        }

        public async Task<ICollection<User>> GetLikersOrLikees(int userId, string type)
        {
            var users = _context.Users.Include(u=>u.Photos).OrderBy(u=>u.UserName).AsQueryable();
            if(type=="likers")
           {
               var userLikers = await GetUserLikes(userId,true);
               users =  users.Where(u=>userLikers.Contains(u.Id));
           }
           else if(type=="likees")
           {
               var userLikees = await GetUserLikes(userId,false);
               users =  users.Where(u=>userLikees.Contains(u.Id));
           }
           else{
               throw new Exception("لا توجد بيانات متاحة");
           }

           return users.ToList();
            
        }
       
      public async Task<ICollection<User>> GetAllUsersExceptAdmin(){
            return await _context.Users.OrderBy(u=>u.NormalizedUserName).Where(u=>u.NormalizedUserName !="ADMIN").ToListAsync();
      }
    }
}