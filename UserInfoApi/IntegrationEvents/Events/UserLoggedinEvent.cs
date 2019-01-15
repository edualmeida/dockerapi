namespace UserInfoApi.IntegrationEvents.Events
{
    public class UserLoggedinEvent: IntegrationEvent
    {
        public string UserId { get; }

        public string UserName { get; }

        public Guid RequestId { get; set; }

        public UserLoggedinEvent(string userId, string userName, Guid requestId,)
        {
            UserId = userId;
            UserName = userName;
            RequestId = requestId;
        }

    }
}