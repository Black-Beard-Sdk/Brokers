using Bb.Brokers;
using Bb.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bb.LogAppender
{

    /// <summary>
    /// Custom log appender for RabbitMQ. 
    /// </summary>
    public class RabbitMqAppender : TraceListener
    {

        #region ctors

        public static RabbitMqAppender Initialize(IFactoryBroker self, string name, string rabbitPublisherName, bool reformat = true, int appendLogsIntervalSeconds = 2, params string[] restrictedTracks)
        {

            if (string.IsNullOrEmpty(rabbitPublisherName))
                throw new Bb.Exceptions.InvalidConfigurationException(nameof(rabbitPublisherName));

            var ex = self.CheckPublisher(rabbitPublisherName);
            if (ex != null)
                throw ex;

            var logger = new RabbitMqAppender(self, appendLogsIntervalSeconds)
            {
                Name = name,
                PublisherName = rabbitPublisherName,
                Reformat = reformat
                //TraceOutputOptions = TraceOptions.
            };

            if (restrictedTracks.Length > 0)
                logger.RestrictedTracks = new HashSet<string>(restrictedTracks);

            System.Diagnostics.Trace.Listeners.Add(logger);
            return logger;
        }

        private RabbitMqAppender(IFactoryBroker factory, int appendLogsIntervalSeconds)
        {
            ass1 = GetType().Assembly;
            ass2 = typeof(TraceListener).Assembly;

            AppendLogsIntervalSeconds = appendLogsIntervalSeconds;
            _factoryBrocker = factory;
            _bag = new List<Task>();
            _timer = new Timer(AppendAsync, null, AppendLogsIntervalSeconds * 1000, AppendLogsIntervalSeconds * 1000);
        }

        #endregion ctors

        /// <summary>
        /// Interval in seconds between appending/sending the logs (default 2 sec.)
        /// used by a timer
        /// </summary>
        public int AppendLogsIntervalSeconds { get; }

        public string PublisherName { get; internal set; }

        public bool Reformat { get; private set; }

        public HashSet<string> RestrictedTracks { get; internal set; }

        #region Dispose

        /// <summary>
        /// When appender is closed, dispose Broker and Timer
        /// </summary>
        protected void OnClose()
        {
            _publisher?.Dispose();
            _timer?.Dispose();
        }

        public override void Flush()
        {

            if (_bag.Count > 0)
                lock (_lock2)
                    if (_bag.Count > 0)
                    {
                        var ar = _bag.ToArray();
                        Task.WaitAll(ar);
                    }

            base.Flush();

            try
            {
                AppendAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("could not flush appender " + e.Message + " - log messages may have been lost.\n" + e.StackTrace); // do not use logger here, as we are inside the logger...
            }
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
            _publisher?.Dispose();
            _timer?.Dispose();
        }

        #endregion

        #region override TraceListener

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(string message)
        {
            Log(message, TraceLevel.Info.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteLine(string message)
        {
            Log(message, TraceLevel.Info.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(object o)
        {
            Log(o, TraceLevel.Info.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Fail(string message)
        {
            Log(message, TraceLevel.Error.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Fail(string message, string detailMessage)
        {
            //Log(message, detailMessage);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(object o, string category)
        {
            Log(o, category);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteLine(object o)
        {
            Log(o, TraceLevel.Error.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(string message, string category)
        {
            Log(message, category);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteLine(object o, string category)
        {
            Log(o, category);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteLine(string message, string category)
        {
            Log(message, category);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Log(object message, string category)
        {

            if (RestrictedTracks == null || RestrictedTracks.Contains(category))
            {

                if (!Reformat && message is string txt)
                    _bufferQueue.Enqueue($"{{ Message : {message}, Level : {category} }}");

                else
                {

                    Dictionary<string, object> properties = DataCollect(message, category);

                    var t = Task.Run(() =>
                    {

                        var sb = new System.Text.StringBuilder(1000);
                        properties.SerializeObjectToJson(sb, false);

                        while (_inPaused)
                            Thread.Yield();

                        _bufferQueue.Enqueue(sb.ToString());

                    });

                    lock (_lock2)
                        _bag.Add(t);

                    t.ContinueWith(task =>
                    {
                        lock (_lock2)
                            _bag.Remove(task);

                    });

                }
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Dictionary<string, object> DataCollect(object message, string category)
        {

            Dictionary<string, object> properties;

            if (message is string)
            {
                properties = new Dictionary<string, object>();
                properties.Add("Message", message);
            }
            else
                properties = TranslateObjectToDictionnarySerializerExtension.GetDictionnaryProperties(message, true);

            if ((TraceOutputOptions & TraceOptions.ProcessId) == TraceOptions.ProcessId)
                properties.Add("ProcessId", Process.GetCurrentProcess().Id);

            if ((TraceOutputOptions & TraceOptions.ThreadId) == TraceOptions.ThreadId)
                properties.Add("ManagedThreadId", Thread.CurrentThread.ManagedThreadId);

            if ((TraceOutputOptions & TraceOptions.DateTime) == TraceOptions.DateTime)
                properties.Add("DateTime", RabbitClock.GetNow().ToString("u", Thread.CurrentThread.CurrentCulture));

            else if ((TraceOutputOptions & TraceOptions.Timestamp) == TraceOptions.Timestamp)
                properties.Add("Timestamp", RabbitClock.GetNow().ToString("u", Thread.CurrentThread.CurrentCulture));

            if ((TraceOutputOptions & TraceOptions.LogicalOperationStack) == TraceOptions.LogicalOperationStack
             || (TraceOutputOptions & TraceOptions.Callstack) == TraceOptions.Callstack)
            {

                var stack = GetStack();

            }

            properties.Add("TraceLevel", category);
            return properties;
        }

        #endregion override

        #region Push in broker

        /// <summary>
        /// AsyncMethod to append/send logs to rabbitMQ. Adds a new Task for each message to be published
        /// and commits and waits for all to finish. The maximum amount of publish per queue is set to 1000.
        /// </summary>
        /// <returns>Empty Task</returns>
        private void AppendAsync(object state = null)
        {

            while (!inTreatment && _bufferQueue.Count > 0)
                try
                {
                    inTreatment = true;
                    PushOutAsynch();
                }
                finally
                {
                    inTreatment = false;
                }

        }

        private void PushOutAsynch()
        {

            lock (_lock1) // general lock - only one thread reads the queue and sends messages at a time.
            {

                if (_publisher == null)
                    _publisher = _factoryBrocker.CreatePublisher(PublisherName);

                var logList = new List<string>(_bufferQueue.Count + 10);

                if (_bufferQueue.Count > 0)
                    try
                    {

                        FillUp(logList);

                        if (logList.Count > 0)
                            _publisher.Publish(message: string.Format("[{0}]", string.Join(",", logList)));

                    }
                    catch (Exception e1)
                    {
                        _inPaused = true;
                        Rescue(logList, e1);
                    }
                    finally
                    {
                        _inPaused = false;
                    }

            }

        }

        private void FillUp(List<string> logList)
        {
            while (_bufferQueue.TryDequeue(out var log))    // dequeue all logs from queue
            {
                logList.Add(log);
                if (logList.Count > MAX_BROKER_LINES)
                    break;
            }
        }

        private void Rescue(List<string> logList, Exception e1)
        {

            var queue2 = new ConcurrentQueue<string>();
            foreach (var item in logList)
                queue2.Enqueue(item);

            while (_bufferQueue.TryDequeue(out var log))
                queue2.Enqueue(log);

            _bufferQueue = queue2;


            // In case of error, just give up the backlog and restart logger. Print on stdout (do not use the logger!)
            Console.WriteLine(e1.ToString());

            try
            {
                _publisher?.Rollback();
            }
            catch (Exception)
            {
                // Do nothing.
            }

        }

        #endregion Push in broker

        #region GetStack

        private StackTrace GetStack()
        {
            var skip = _skipFrame ?? StoreSkip();
            var stack = new StackTrace(skip);
            var ass = stack.GetFrame(0).GetMethod().DeclaringType.Assembly;
            if (ass == ass1 || ass == ass2)
                stack = new StackTrace(GetSkip());
            return stack;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private int StoreSkip()
        {
            lock (_lock1)
                if (_skipFrame == null)
                    lock (_lock1)
                        _skipFrame = GetSkip();

            return _skipFrame.Value;

        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private int GetSkip()
        {

            int skipFrame = 0;
            var stack = new StackTrace();
            for (int i = 0; i < stack.FrameCount; i++)
            {
                var frame = stack.GetFrame(i);
                var type = frame.GetMethod().DeclaringType;
                if (type.Assembly != ass1 && type.Assembly != ass2)
                {
                    skipFrame = i - 2;
                    break;
                }
            }

            return skipFrame;

        }

        #endregion GetStack

        private static readonly int MAX_BROKER_LINES = 1000;
        //private readonly BrokerPublishParameters _cnxBroker;

        /// <summary>
        /// (single threaded) persistent publisher, holding the broker session.
        /// </summary>
        private IBrokerPublisher _publisher;

        /// <summary>
        /// Buffer queue containing the string (of loggingEvent) to be sent/appended to rabbitMQ
        /// </summary>
        private ConcurrentQueue<string> _bufferQueue = new ConcurrentQueue<string>();

        /// <summary>
        /// Timer to execute the async appending of logs
        /// </summary>
        private readonly Timer _timer;
        private readonly object _lock1 = new object();
        private readonly object _lock2 = new object();
        private readonly IFactoryBroker _factoryBrocker;
        private readonly List<Task> _bag;
        private readonly Assembly ass1;
        private readonly Assembly ass2;
        private bool inTreatment = false;
        private int? _skipFrame;
        private bool _inPaused;
    }
}
