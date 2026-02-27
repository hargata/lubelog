using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CarCareTracker.Logic
{
    public interface IEventHubLogic
    {
        Task ReceiveChangeForAllVehicles(WebHookPayload webHookPayload);
    }
    [Authorize]
    public class EventHubLogic: Hub<IEventHubLogic>
    {
        private readonly IUserLogic _userLogic;
        private readonly ILogger<EventHubLogic> _logger;
        public EventHubLogic(IUserLogic userLogic, ILogger<EventHubLogic> logger)
        {
            _userLogic = userLogic;
            _logger = logger;
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        public async Task JoinGroup(string groupName)
        {
            try
            {
                string userId = Context.UserIdentifier ?? string.Empty;
                if (groupName.ToLower().StartsWith("kiosk"))
                {
                    //append user id
                    groupName = $"kiosk_{userId}";
                }
                else if (groupName.ToLower().StartsWith("vehicleid_"))
                {
                    int vehicleId = int.Parse(groupName.Split("_")[1]);
                    if (!_userLogic.UserCanEditVehicle(int.Parse(userId), vehicleId, HouseholdPermission.View))
                    {
                        _logger.LogWarning($"User Id {userId} attempted to subscribe to webhook events for inaccessible group {groupName}");
                        return; //user does not have access to this vehicle
                    }
                }
                else
                {
                    _logger.LogWarning($"User Id {userId} attempted to subscribe to webhook events for invalid group {groupName}");
                    return; //invalid group name
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}