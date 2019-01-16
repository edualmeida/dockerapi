namespace LoggerApi.IntegrationEvents.EventHandling
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using RabbitLib;
    using LoggerApi.IntegrationEvents.Events;
    using System;
    
    public class UserLoggedinEventHandler : 
        IIntegrationEventHandler<UserLoggedinEvent>
    {
        public UserLoggedinEventHandler()
        {
        }

        public async Task Handle(UserLoggedinEvent command)
        {
            Console.WriteLine("UserLoggedinEventHandler: " + command.UserName);
        }
    }
}