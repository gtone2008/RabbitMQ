using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ_receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            RabbitMQReceiver();
            Console.ReadLine();
        }
        static void RabbitMQReceiver()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = "amqp://test:test@WUXENGSER04:5672";
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    //channel.QueueDeclare("MyTest", false, false, false, null);
                    QueueingBasicConsumer consumer = new QueueingBasicConsumer(channel);
                    //noAck = true，不需要回复，接收到消息后，queue上的消息就会清除  
                    //noAck = false，需要回复，接收到消息后，queue上的消息不会被清除，  
                    //直到调用channel.basicAck(deliveryTag, false);   
                    //queue上的消息才会被清除 而且，在当前连接断开以前，其它客户端将不能收到此queue上的消息  
                    channel.BasicConsume("MyTest", true, consumer);//true 
                    while (true)
                    {
                        //阻塞函数，获取队列中的消息
                        BasicDeliverEventArgs ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                        byte[] bytes = ea.Body;
                        string str = Encoding.UTF8.GetString(bytes);
                        Console.WriteLine("队列消息:" + str.ToString());
                    }

                }
            }
        }

    }
}
