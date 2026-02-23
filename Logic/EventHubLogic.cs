using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CarCareTracker.Logic
{
    public interface IEventHubLogic
    {
        Task ReceiveChangeForAllVehicles();
    }
    [Authorize]
    public class EventHubLogic: Hub<IEventHubLogic>
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }
}