using Microsoft.AspNetCore.SignalR;
namespace ZawagProject.Models
{
    public class ChatHub :Hub //destripioter//موزع
    {
        public async void Refresh(){
            //all members connected to hub
            await Clients.All.SendAsync("Refresh");
        }
          public async void count(){
            //all members connected to hub
            await Clients.All.SendAsync("count");
        }
    }
}