using RabbitMQ.Client;
using System;

namespace Bb.Brokers
{

    public class RabbitInterceptor
    {

        public static RabbitInterceptor Instance { get; set; }



        /// <summary>
        /// Gets or sets interceptor for initialize connection action.
        /// </summary>
        /// <value>
        /// The initialize connection.
        /// </value>
        public Action<IBroker, IConnection> InitializeConnection { get; set; }

        /// <summary>
        /// Gets or sets interceptor for dispose connection. 
        /// </summary>
        /// <value>
        /// The dispose connection.
        /// </value>
        public Action<IBroker, IConnection> DisposeConnection { get; set; }

        /// <summary>
        /// Gets or sets interceptor for initialize session.
        /// </summary>
        /// <value>
        /// The initialize session.
        /// </value>
        public Action<IBroker, IModel> InitializeSession { get; set; }

        /// <summary>
        /// Gets or sets interceptor for dispose session.
        /// </summary>
        /// <value>
        /// The dispose session.
        /// </value>
        public Action<IBroker, IModel> DisposeSession { get; set; }

        #region internal

        internal void InitializeConnectionRun(IBroker broker, IConnection connection)
        {
            InitializeConnection?.Invoke(broker, connection);
        }

        internal void DisposeConnectionRun(IBroker broker, IConnection connection)
        {
            DisposeConnection?.Invoke(broker, connection);
        }

        internal void InitializeSessionRun(IBroker broker, IModel session)
        {
            InitializeSession?.Invoke(broker, session);
        }

        /// <summary>
        /// Disposes the session.
        /// </summary>
        /// <param name="broker">The broker.</param>
        /// <param name="session">The session.</param>
        internal void DisposeSessionRun(IBroker broker, IModel session)
        {
            DisposeSession?.Invoke(broker, session);
        }

        #endregion internal

    }

}
