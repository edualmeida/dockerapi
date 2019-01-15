// namespace RabbitLib
// {
//     public class RabbitListener
//     {
//         ConnectionFactory factory { get; set; }
//         IConnection connection { get; set; }
//         IModel channel { get; set; }

//         private void StartListenner()
//         {
//             Console.WriteLine("StartListenner->Trying to connect to MQ;");

//             try
//             {
//                 this.factory = new ConnectionFactory();

//                 factory.UserName = "guest";
//                 factory.Password = "guest";
//                 factory.HostName = "rabbitmq";
//                 factory.Port = 5672;

//                 this.connection = factory.CreateConnection();
//                 this.channel = connection.CreateModel();      

//                 Console.WriteLine("StartListenner->Connection done;");
//             }
//             catch(Exception ex)
//             {
//                 Console.WriteLine(ex.ToString());
//                 System.Threading.Thread.Sleep(10000);
//                 StartListenner();
//             }
//         }

//         public void Register()
//         {            
//             StartListenner();

//             channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

//             var consumer = new EventingBasicConsumer(channel);
//             consumer.Received += (model, ea) =>
//             {
//                 var body = ea.Body;
//                 var message = Encoding.UTF8.GetString(body);
//                 int m = 0;

//                 Console.WriteLine("Message received: " + message);
//             };
//             channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

//             Console.WriteLine("RabbitListener registered.");
//         }

//         public void Deregister()
//         {
//             this.connection.Close();
//         }

//         public RabbitListener()
//         {
//             //this.factory = new ConnectionFactory() { HostName = "localhost" };



//         }
//     }
// }