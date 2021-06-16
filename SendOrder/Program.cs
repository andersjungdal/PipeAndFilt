using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace SendOrder
{
    class Order
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { Uri = new Uri("amqp://admin:iamadmin@localhost:5672") };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                    type: "direct");

                var severity = "order_pipe";
                var message = "Incoming order";
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "direct_logs",
                    routingKey: severity,
                    basicProperties: null,
                    body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
