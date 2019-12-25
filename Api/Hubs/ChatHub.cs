using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrbanSisters.Dal;
using UrbanSisters.Model;

namespace UrbanSisters.Api.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly UrbanSisterContext _context;

        public ChatHub(UrbanSisterContext context)
        {
            this._context = context;
        }

        public async Task SendMessageToRelookeuse(int receiver, string message)
        {
            Appointment appointment = await _context.Appointment.Where(a => a.Accepted && !a.Finished && a.UserId.Equals(Int32.Parse(Context.UserIdentifier)) && a.RelookeuseId == receiver).FirstOrDefaultAsync();

            if (appointment == null)
            {
                await Clients.Caller.SendAsync("Unauthorized");
            }
            else
            {
                await Clients.User(appointment.RelookeuseId.ToString()).SendAsync("ReceiveMessage", message);
            }
        }
        
        public async Task SendMessageToCustomer(int receiver, string message)
        {
            Appointment appointment = await _context.Appointment.Where(a => a.Accepted && !a.Finished && a.UserId == receiver && a.RelookeuseId.Equals(Int32.Parse(Context.UserIdentifier))).FirstOrDefaultAsync();

            if (appointment == null)
            {
                await Clients.Caller.SendAsync("Unauthorized");
            }
            else
            {
                await Clients.User(appointment.UserId.ToString()).SendAsync("ReceiveMessage", message);
            }
        }
    }
}
