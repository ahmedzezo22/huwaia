
namespace ZawagProject.Models
{
    public class Like
    {
       public int LikeeId { get; set; }
       public User Likee { get; set; }
       public int LikerId { get; set; }
       public User Liker { get; set; }
       
       
    }
}