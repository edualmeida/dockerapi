using RabbitLib;
using System;

namespace UserInfoApi.IntegrationEvents.Events
{
    public class UserLoggedinEvent: IntegrationEvent
    {
        public string UserId { get; }

        public string UserName { get; }

        public string RequestId { get; set; }

        public UserLoggedinEvent(string userId, string userName, string requestId)
        {
            UserId = userId;
            UserName = userName;
            RequestId = requestId;
        }

    }
}