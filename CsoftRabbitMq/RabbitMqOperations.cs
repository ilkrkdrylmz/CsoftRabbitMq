using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CsoftRabbitMq
{
    public class RabbitMqOperations
    {
        private readonly string _hostname;
        private readonly string _virtualHost;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;

        /// <summary>
        ///    RabbitMQ Kuyruklarına bağlanmak için kullanılır. Gönderilen bilgilerle istenilen kuyruğa bağlanmak için Puslisher veya Consumer metodları çağırılır.
        /// </summary>
        /// <param name="hostname">RabbitMQ Server Adı (Default Değeri: 127.0.0.1)</param>
        /// <param name="virtualHost">RabbitMQ Virtual Host Adı (Default Değeri: /)</param>
        /// <param name="port">RabbitMQ AMQP Portu (Default Değeri: 5672)</param>
        /// <param name="username">RabbitMQ Kullanıcı Adı (Default Değeri: guest)</param>
        /// <param name="password">RabbitMQ Şifresi (Default Değeri: guest)</param>
        public RabbitMqOperations(string hostname = "127.0.0.1", string virtualHost = "/", string username = "guest", string password = "guest", int port = 5672)
        {
            _hostname = hostname;
            _virtualHost = virtualHost;
            _username = username;
            _password = password;
            _port = port;
        }

        /// <summary>
        /// RabbitMQ Kuyruk bağlantısı döner.
        /// </summary>
        /// <returns>Kuyruk bağlanıtısı</returns>
        public IConnection GetConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password,
                VirtualHost = _virtualHost,
                Port = _port
            };
            return factory.CreateConnection();
        }

        /// <summary>
        /// RabbitMQ Kanalı döner. Kuyruk adını dışarıdan alır.
        /// </summary>
        /// <param name="connection">Bağlanılacak Kuyruk Bağlantısı</param>
        /// <param name="queueName">Kuyruk Adı</param>
        /// <param name="queueDurable">Kuyruğun kalıcı olması ayarlanır.</param>
        /// <param name="autoDelete">Kuyruktaki mesajların otomatik silinmesinin ayarlanır.</param>
        /// <param name="exclusive">Kuyruğun özel olduğunun belirtecidir.</param>
        /// <returns>Kuyruk Kanalı</returns>
        public IModel GetChannel(IConnection connection, string queueName, bool queueDurable = true, bool autoDelete = false, bool exclusive = false)
        {
            IModel channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName, durable: queueDurable, exclusive, autoDelete: autoDelete, null);
            return channel;
        }

        /// <summary>
        /// Kuyruğa mesaj gönderimi için kullanılır.
        /// </summary>
        /// <param name="channel">Rabbit MQ Mesaj Kanalı</param>
        /// <param name="messageContent">Mesaj içeriği</param>
        /// <param name="routingKey">Mesajın gönderileceği kuyruğun adı</param>
        /// <param name="alwaysSaveMessages">Mesajlar her zaman saklansın ayarı.</param>
        /// <returns>Genel kuyruk cevabı içerisinde başarı mesajı döner.</returns>
        public GeneralQueueResponse<string> PublishMessage(IModel channel, byte[] messageContent, string routingKey, bool alwaysSaveMessages = true)
        {
            try
            {
                if (alwaysSaveMessages)
                {
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    channel.BasicPublish("", routingKey, properties, body: messageContent);
                }
                else
                {
                    channel.BasicPublish("", routingKey, null, body: messageContent);
                }
                return new GeneralQueueResponse<string>()
                {
                    Result = "Mesaj kuyruğa iletildi."
                };
            }
            catch (Exception ex)
            {
                return new GeneralQueueResponse<string>()
                {
                    ErrorMessage = ex.Message + " " + ex.InnerException.Message ?? null
                };
            }
        }

        /// <summary>
        /// Kuyruğa dinleyici olarak bağlanır.
        /// </summary>
        /// <param name="channel">Rabbit MQ Mesaj Kanalı</param>
        /// <param name="queueName">Bağlanılacak kuyruk adı</param>
        /// <param name="queueDurable">Kuyruğun RAM yada ROM da tutulması için istenir. (Default Değer: false, RAM de tutulur.)</param>
        /// <param name="autoDelete">Kuyruğun otomatik silinmesi belirtecidir. (Default Değer: true, kuyruğun işi bitince silinir.</param>
        /// <returns>Eventing Basic Consumer objesi döner.</returns>
        public EventingBasicConsumer SetConsumer(IModel channel, string queueName, bool queueDurable = false, bool autoDelete = true)
        {
            channel.QueueDeclare(queue: queueName, durable: queueDurable, false, autoDelete: autoDelete, null);
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueName, false, consumer);
            return consumer;
        }
    }
}
