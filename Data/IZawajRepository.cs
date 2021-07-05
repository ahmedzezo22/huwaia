using System.Collections.Generic;
using System.Threading.Tasks;
using ZawagProject.Helpers;
using ZawagProject.Models;

namespace ZawagProject.Data
{
    public interface IZawajRepository
    {
         void Add<T>(T entity) where T:class;
         void Delete<T>(T entity) where T:class;

         Task<bool>SaveAll();

         Task<PagesList<User>> GetUsers(UsersParam usersParam);

         Task<User>GetUser(int id,bool isCurrentUser);
         Task<Photo>GetPhoto(int id);
         Task<Photo>GetMainPhotoForUser(int userid);
         Task<Like>GetLike(int userId,int receipientId);
         Task<Message>GetMessage(int Id);

         Task<PagesList<Message>>GetMessagesForUser(MessageParams messageParams);
       Task<IEnumerable<Message>> GetConversation(int userId, int receipientId);

       Task<int> GetUnReadMessagesForUser(int userId);
       Task<Payment>GetPaymentForUser(int userId);
       Task<ICollection<User>> GetLikersOrLikees(int userId,string type);

       Task<ICollection<User>> GetAllUsersExceptAdmin();
    }
}