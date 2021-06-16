using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DeDup
{
    class Program
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { Uri = new Uri("amqp://admin:iamadmin@localhost:5672") };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                    type: "direct");
                var queueName = channel.QueueDeclare().QueueName;


                channel.QueueBind(queue: queueName,
                    exchange: "direct_logs",
                    routingKey: "authenticate_pipe");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                        routingKey, message);
                    var m = message.Replace(" Order", "");
                    string dedupedOrder = DeDupOrder(m);
                    SendDeDupedOrder(dedupedOrder);
                };
                channel.BasicConsume(queue: queueName,
                    autoAck: true,
                    consumer: consumer);



                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
            static string DeDupOrder(string msg)
            {
                msg += ", De-Duped Order";
                return msg;
            }

            static void SendDeDupedOrder(string dedupMessage)
            {
                var factory2 = new ConnectionFactory() { Uri = new Uri("amqp://admin:iamadmin@localhost:5672") };
                using (var connection = factory2.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "direct_logs",
                        type: "direct");

                    var severity = "dedup_pipe";
                    var message = dedupMessage;
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
}
