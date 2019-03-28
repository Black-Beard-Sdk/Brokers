using Bb.Configurations;
using Bb.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Text;

namespace Bb.Brokers
{
    public class RabbitBrokerContext : IBrokerContext, IRabbitMessage
    {

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="parameters"></param>
        public RabbitBrokerContext()
        {

        }

        public object TransactionId => _message.DeliveryTag;

        /// <summary>
        /// Return the message from utf8. 
        /// </summary>
        public string Utf8Data => Encoding.UTF8.GetString(_message.Body);

        /// <summary>
        /// The exchange the message was originally published to
        /// </summary>
        public string Exchange => _message.Exchange;

        /// <summary>
        /// The routing key used when the message was originally published.
        /// </summary>
        public string RoutingKey => _message.RoutingKey;

        /// <summary>
        /// A message may have headers. (can be null or empty).
        /// </summary>
        public IDictionary<string, object> Headers
        {
            get => _message.BasicProperties.Headers;
            set => _message.BasicProperties.Headers = value;
        }

        /// <summary>
        /// Latest message read is marked as correctly read and should never be presented again (may actually happen).
        /// </summary>
        public void Commit()
        {
            _session.BasicAck(_message.DeliveryTag, false);
        }

        /// <summary>
        /// Discard a message, never present it again.
        /// </summary>
        public void Reject()
        {
            _session.BasicReject(_message.DeliveryTag, false);
        }

        /// <summary>
        /// Discard a message, represent it later.
        /// </summary>
        public void Rollback()
        {
            _session.BasicNack(_message.DeliveryTag, false, true);
        }

        /// <summary>
        /// Will put the message back in the queue, at the start of the queue.
        /// </summary>
        public void RequeueLast()
        {
            IncrementReplay();
            _session.BasicPublish(_message.Exchange, _message.RoutingKey, _message.BasicProperties, _message.Body);
            Commit();

        }

        ///// <summary>
        ///// How many times the message may be requeued before being labeled a poison message. -1 to disable.
        ///// </summary>
        //public int MaxReplayCount => _parameters.MaxReplayCount;

        ///// <summary>
        ///// The header used for the poison message mechanism. By default the value is "REPLAY"
        ///// </summary>
        //public string ReplayHeaderKey => _parameters.ReplayHeaderKey;

        public bool CanBeRequeued()
        {

            int count = 0;
            if (_message.BasicProperties.Headers.TryGetValue(_parameters.ReplayHeaderKey, out object o))
                if (!int.TryParse(o.ToString(), out count))
                    count = 1;

            return count < _parameters.MaxReplayCount;

        }

        public int ReplayCount
        {
            get
            {
                int count = 0;
                if (_message.BasicProperties.Headers.TryGetValue(_parameters.ReplayHeaderKey, out object o))
                    if (!int.TryParse(o.ToString(), out count))
                        count = 1;

                return count;
            }
        }

        private void IncrementReplay()
        {

            var count = ReplayCount;
            count++;

            if (count > _parameters.MaxReplayCount)
                throw new MaxReplayException(_parameters.MaxReplayCount, this);

            if (_message.BasicProperties.Headers.ContainsKey(_parameters.ReplayHeaderKey))
                _message.BasicProperties.Headers[_parameters.ReplayHeaderKey] = count;
            else
                _message.BasicProperties.Headers.Add(_parameters.ReplayHeaderKey, count);
        }






        BrokerSubscriptionParameter IRabbitMessage.Parameters { get => _parameters; set => _parameters = value; }

        BasicDeliverEventArgs IRabbitMessage.Message { get => _message; set => _message = value; }

        IModel IRabbitMessage.Session { get => _session; set => _session = value; }

        IBroker IRabbitMessage.Broker { get => _broker; set => _broker= value; }

        public IBroker Broker => _broker; 

        private IBroker _broker;
        private BrokerSubscriptionParameter _parameters;
        private IModel _session;
        private BasicDeliverEventArgs _message;

    }


}
