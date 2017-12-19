using Swisscard.MS.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;



namespace Syliance.Tracing
{
    public class TraceHelper : IDisposable
    {
        #region Constants, please change
        // ID for ETL Tracing
        string ETLTracerId = Constants.Trace_ETLTracerId;
        // NT Event Source name
        string eventSourceName = Constants.Trace_SourceName;
#if DEBUG
        string traceFileName =  Constants.Trace_SourceName + ".log";
#endif
        #endregion

#if DEBUG
        static DelimitedListWithTimeTraceListener fileListener = null;
#endif
        static EventLogTraceListener eventListener = null;
        static ETWTraceListener etwListener = null;

        static TraceSource source = new TraceSource(Constants.Trace_SourceName, SourceLevels.All);

        string currentMethod = "";
        bool writeFuncStartEnd;

        static TraceHelper()
        {
        }

        public TraceHelper() : this(true)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writeFuncStartEnd">Should trace function start and end method</param>
        public TraceHelper(bool writeFuncStartEnd)
        {
            this.writeFuncStartEnd = writeFuncStartEnd;

#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                if (fileListener == null)
                {
                    fileListener = new DelimitedListWithTimeTraceListener(traceFileName);
                    if (!source.Listeners.Contains(fileListener))
                        source.Listeners.Add(fileListener);
                }
            }
#endif
            if (eventListener == null)
            {
                try
                {
                    string eventSourceNameOverride = "Service Manager Console";
                    if (!EventLog.SourceExists(eventSourceNameOverride))
                        EventLog.CreateEventSource(new EventSourceCreationData(eventSourceNameOverride, "Operations Manager"));
                    EventLog eventLog = new EventLog("Operations Manager");
                    eventLog.Source = eventSourceNameOverride;
                    TraceHelper.eventListener = new EventLogTraceListener(eventLog);
                    eventListener.Filter = new SylianceEventTypeFilter(SourceLevels.Information);
                }
                catch
                {

                }
            }

            if (TraceHelper.etwListener == null)
            {
                TraceHelper.etwListener = new ETWTraceListener(ETLTracerId);
            }

            if (!source.Listeners.Contains(eventListener))
                source.Listeners.Add(eventListener);

            if (!source.Listeners.Contains(etwListener))
                source.Listeners.Add(etwListener);

            StackTrace stackTrace = new StackTrace(true);
            this.currentMethod = stackTrace.GetFrame(1).GetMethod().Name;

            if (this.ShouldTrace(TraceEventLevel.Verbose))
            {
                if (this.writeFuncStartEnd)
                {
                    this.WriteVerbose("Starting method...");
                }
            }
        }

        public void Dispose()
        {
            if (this.writeFuncStartEnd)
            {
                this.WriteVerbose("End method");
            }
        }

        public void WriteTrace(TraceEventLevel level, string message, params object[] args)
        {
            string curDateTime = DateTime.Now.ToString("s");

            switch (level)
            {
                case TraceEventLevel.Always:
                case TraceEventLevel.Verbose:
                    message = this.currentMethod + ": " + message;
                    if (args.Length == 0)
                        source.TraceEvent(TraceEventType.Verbose, 0, message);
                    else
                        source.TraceEvent(TraceEventType.Verbose, 0, message, args);
                    break;
                case TraceEventLevel.Critical:
                    message = this.currentMethod + ": " + message;
                    if (args.Length == 0)
                        source.TraceEvent(TraceEventType.Critical, 0, message);
                    else
                        source.TraceEvent(TraceEventType.Critical, 0, message, args);
                    break;
                case TraceEventLevel.Error:
                    message = this.currentMethod + ": " + message;
                    if (args.Length == 0)
                        source.TraceEvent(TraceEventType.Error, 0, message);
                    else
                        source.TraceEvent(TraceEventType.Error, 0, message, args);
                    break;
                case TraceEventLevel.Warning:
                    if (args.Length == 0)
                        source.TraceEvent(TraceEventType.Warning, 0, message);
                    else
                        source.TraceEvent(TraceEventType.Warning, 0, message, args);
                    break;
                case TraceEventLevel.Informational:
                    if (args.Length == 0)
                        source.TraceEvent(TraceEventType.Information, 0, message);
                    else
                        source.TraceEvent(TraceEventType.Information, 0, message, args);
                    break;
            }

            source.Flush();
        }

        public void WriteVerbose(string message, params object[] args)
        {
            this.WriteTrace(TraceEventLevel.Verbose, message, args);
        }

        public void WriteInfo(string message, params object[] args)
        {
            this.WriteTrace(TraceEventLevel.Informational, message, args);
        }

        public void WriteWarning(string message, params object[] args)
        {
            this.WriteTrace(TraceEventLevel.Warning, message, args);
        }

        public void WriteError(Exception er)
        {
            WriteError(er, "");
        }

        public void WriteError(Exception er, string message, params object[] args)
        {
            string resultMessage = "";
            if (!string.IsNullOrEmpty(message))
            {
                resultMessage += string.Format(message, args) + Environment.NewLine;
            }
            while (er != null)
            {
                StackTrace stackTrace = new StackTrace(er, true);
                resultMessage += string.Format("{0}\r\n\r\n{1}", er.Message, stackTrace.ToString());
                er = er.InnerException;
            }

            this.WriteTrace(TraceEventLevel.Error, resultMessage);
        }

        public void WriteError(string message, params object[] args)
        {
            this.WriteTrace(TraceEventLevel.Error, message, args);
        }



        public void WriteCritical(string message, params object[] args)
        {
            this.WriteTrace(TraceEventLevel.Critical, message, args);
        }

        public bool ShouldTrace(TraceEventLevel level)
        {
            return etwListener.ShouldTrace(level) || System.Diagnostics.Debugger.IsAttached;
        }

        /// <summary>
        /// Filter for max level
        /// </summary>
        public class SylianceEventTypeFilter : TraceFilter
        {
            private SourceLevels level;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="level">Max level to trace</param>
            public SylianceEventTypeFilter(SourceLevels level)
            {
                this.level = level;
            }

            public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
            {
                return (int)eventType <= (int)this.level;
            }

            public SourceLevels EventType
            {
                get
                {
                    return this.level;
                }
                set
                {
                    this.level = value;
                }
            }
        }


    }

    /// <summary>
    /// /// According http://msdn.microsoft.com/en-us/library/windows/desktop/aa363711(v=vs.85).aspx possible values are:
    /// 0 - filtering off
    /// 1 TRACE_LEVEL_CRITICAL Abnormal exit or termination events
    /// 2 TRACE_LEVEL_ERROR Severe error events
    /// 3 TRACE_LEVEL_WARNING Warning events such as allocation failures
    /// 4 TRACE_LEVEL_INFORMATION Non-error events such as entry or exit events
    /// 5 TRACE_LEVEL_VERBOSE Detailed trace events
    /// </summary>
    public enum TraceEventLevel
    {
        Always,
        Critical,
        Error,
        Warning,
        Informational,
        Verbose
    }

    public static class EventProviderExtension
    {
        public static byte GetLevel(this EventProvider provider)
        {
            FieldInfo levelInfo = typeof(EventProvider).GetField("m_level", System.Reflection.BindingFlags.Instance | BindingFlags.NonPublic);

            if (levelInfo != null)
            {
                return (byte)levelInfo.GetValue(provider);
            }


            return (byte)0;
        }
    }

    /// <summary>
    /// This is copy of the standard EventProviderTraceListener class
    /// The only fixed thing - mapping between TraceEventType and Trace Level
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public class ETWTraceListener : TraceListener
    {
        private string m_delimiter;
        private int m_initializedDelim;
        private object m_Lock;
        private EventProvider m_provider;
        private const string s_activityIdString = "activityId=";
        private const string s_callStackString = " : CallStack:";
        private const int s_defaultPayloadSize = 0x200;
        private const uint s_keyWordMask = 0xffffff00;
        private const string s_nullCStringValue = ": null";
        private const string s_nullStringComaValue = "null,";
        private const string s_nullStringValue = "null";
        private const string s_optionDelimiter = "delimiter";
        private const string s_relatedActivityIdString = "relatedActivityId=";

        public bool ShouldTrace(TraceEventLevel level)
        {
            return this.m_provider.IsEnabled((byte)level, 0);
        }

        public ETWTraceListener(string providerId)
        {
            this.m_delimiter = ";";
            this.m_Lock = new object();
            this.InitProvider(providerId);
        }

        public ETWTraceListener(string providerId, string name)
            : base(name)
        {
            this.m_delimiter = ";";
            this.m_Lock = new object();
            this.InitProvider(providerId);
        }

        public ETWTraceListener(string providerId, string name, string delimiter)
            : base(name)
        {
            this.m_delimiter = ";";
            this.m_Lock = new object();
            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }
            if (delimiter.Length == 0)
            {
                throw new ArgumentException("Please provide non-empty Delimiter");
            }
            this.m_delimiter = delimiter;
            this.m_initializedDelim = 1;
            this.InitProvider(providerId);
        }

        public override void Close()
        {
            this.m_provider.Close();
        }

        public override void Fail(string message, string detailMessage)
        {
            StringBuilder builder = new StringBuilder(message);
            if (detailMessage != null)
            {
                builder.Append(" ");
                builder.Append(detailMessage);
            }
            this.TraceEvent(null, null, TraceEventType.Error, 0, builder.ToString());
        }

        public sealed override void Flush()
        {
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "delimiter" };
        }

        private void InitProvider(string providerId)
        {
            Guid providerGuid = new Guid(providerId);
            this.m_provider = new EventProvider(providerGuid);
        }

        public sealed override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                StringBuilder builder = new StringBuilder(0x200);
                if (data != null)
                {
                    builder.Append(data.ToString());
                }
                else
                {
                    builder.Append(": null");
                }
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    builder.Append(" : CallStack:");
                    builder.Append(eventCache.Callstack);
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
            }
        }

        public sealed override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                StringBuilder builder = new StringBuilder(0x200);
                if ((data != null) && (data.Length > 0))
                {
                    int index = 0;
                    while (index < (data.Length - 1))
                    {
                        if (data[index] != null)
                        {
                            builder.Append(data[index].ToString());
                            builder.Append(this.Delimiter);
                        }
                        else
                        {
                            builder.Append("null,");
                        }
                        index++;
                    }
                    if (data[index] != null)
                    {
                        builder.Append(data[index].ToString());
                    }
                    else
                    {
                        builder.Append("null");
                    }
                }
                else
                {
                    builder.Append("null");
                }
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    builder.Append(" : CallStack:");
                    builder.Append(eventCache.Callstack);
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
            }
        }

        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    this.m_provider.WriteMessageEvent(" : CallStack:" + eventCache.Callstack, (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(string.Empty, (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
            }
        }

        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                StringBuilder builder = new StringBuilder(0x200);
                builder.Append(message);
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    builder.Append(" : CallStack:");
                    builder.Append(eventCache.Callstack);
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
            }
        }

        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                if (args == null)
                {
                    if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                    {
                        this.m_provider.WriteMessageEvent(format + " : CallStack:" + eventCache.Callstack, (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                    }
                    else
                    {
                        this.m_provider.WriteMessageEvent(format, (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                    }
                }
                else if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    this.m_provider.WriteMessageEvent(string.Format(CultureInfo.InvariantCulture, format, args) + " : CallStack:" + eventCache.Callstack, (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(string.Format(CultureInfo.InvariantCulture, format, args), (byte)TraceEventTypeToTraceLevel(eventType), ((long)eventType) & ((long)0xffffff00L));
                }
            }
        }

        [SecurityCritical]
        public sealed override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            if (this.m_provider.IsEnabled())
            {
                StringBuilder builder = new StringBuilder(0x200);
                object activityId = Trace.CorrelationManager.ActivityId;
                if (activityId != null)
                {
                    Guid guid = (Guid)activityId;
                    builder.Append("activityId=");
                    builder.Append(guid.ToString());
                    builder.Append(this.Delimiter);
                }
                builder.Append("relatedActivityId=");
                builder.Append(relatedActivityId.ToString());
                builder.Append(this.Delimiter + message);
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    builder.Append(" : CallStack:");
                    builder.Append(eventCache.Callstack);
                    this.m_provider.WriteMessageEvent(builder.ToString(), 0, 0x1000L);
                }
                else
                {
                    this.m_provider.WriteMessageEvent(builder.ToString(), 0, 0x1000L);
                }
            }
        }

        public sealed override void Write(string message)
        {
            if (this.m_provider.IsEnabled())
            {
                this.m_provider.WriteMessageEvent(message, (byte)TraceEventTypeToTraceLevel(TraceEventType.Verbose), 0L);
            }
        }

        public sealed override void WriteLine(string message)
        {
            this.Write(message);
        }

        public string Delimiter
        {
            get
            {
                if (this.m_initializedDelim == 0)
                {
                    lock (this.m_Lock)
                    {
                        if (this.m_initializedDelim == 0)
                        {
                            if (base.Attributes.ContainsKey("delimiter"))
                            {
                                this.m_delimiter = base.Attributes["delimiter"];
                            }
                            this.m_initializedDelim = 1;
                        }
                    }
                    if (this.m_delimiter == null)
                    {
                        throw new ArgumentNullException("Delimiter");
                    }
                    if (this.m_delimiter.Length == 0)
                    {
                        throw new ArgumentException("Please provide non-empty Delimiter");
                    }
                }
                return this.m_delimiter;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Delimiter");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException("Please provide non-empty Delimiter");
                }
                lock (this.m_Lock)
                {
                    this.m_delimiter = value;
                    this.m_initializedDelim = 1;
                }
            }
        }

        public sealed override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// The TraceEventType doesn't match trace level
        /// According http://msdn.microsoft.com/en-us/library/windows/desktop/aa363711(v=vs.85).aspx possible values are:
        /// 1 TRACE_LEVEL_CRITICAL Abnormal exit or termination events
        /// 2 TRACE_LEVEL_ERROR Severe error events
        /// 3 TRACE_LEVEL_WARNING Warning events such as allocation failures
        /// 4 TRACE_LEVEL_INFORMATION Non-error events such as entry or exit events
        /// 5 TRACE_LEVEL_VERBOSE Detailed trace events
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        byte TraceEventTypeToTraceLevel(TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                    return 1;
                case TraceEventType.Error:
                    return 2;
                case TraceEventType.Warning:
                    return 3;
                case TraceEventType.Information:
                    return 4;
                case TraceEventType.Verbose:
                    return 5;
                default:
                    return (byte)eventType;
            }
        }
    }

    [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    public class DelimitedListWithTimeTraceListener : TextWriterTraceListener
    {
        private string delimiter;
        private bool initializedDelim;
        private string secondaryDelim;

        public DelimitedListWithTimeTraceListener(Stream stream)
            : base(stream)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListWithTimeTraceListener(TextWriter writer)
            : base(writer)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListWithTimeTraceListener(string fileName)
            : base(fileName)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListWithTimeTraceListener(Stream stream, string name)
            : base(stream, name)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListWithTimeTraceListener(TextWriter writer, string name)
            : base(writer, name)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListWithTimeTraceListener(string fileName, string name)
            : base(fileName, name)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "delimiter" };
        }

        internal bool IsEnabled(TraceOptions opts)
        {
            return ((opts & this.TraceOutputOptions) != TraceOptions.None);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
            {
                this.WriteHeader(source, eventType, id);
                this.Write(this.Delimiter);
                this.WriteEscaped(data.ToString());
                this.Write(this.Delimiter);
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                this.WriteHeader(source, eventType, id);
                this.Write(this.Delimiter);
                if (data != null)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (i != 0)
                        {
                            this.Write(this.secondaryDelim);
                        }
                        this.WriteEscaped(data[i].ToString());
                    }
                }
                this.Write(this.Delimiter);
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
            {
                this.WriteHeader(source, eventType, id);
                this.WriteEscaped(message);
                this.Write(this.Delimiter);
                this.Write(this.Delimiter);
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                this.WriteHeader(source, eventType, id);
                if (args != null)
                {
                    this.WriteEscaped(string.Format(CultureInfo.InvariantCulture, format, args));
                }
                else
                {
                    this.WriteEscaped(format);
                }
                this.Write(this.Delimiter);
                this.Write(this.Delimiter);
                this.WriteFooter(eventCache);
            }
        }

        private void WriteEscaped(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                int num;
                StringBuilder builder = new StringBuilder("\"");
                int startIndex = 0;
                while ((num = message.IndexOf('"', startIndex)) != -1)
                {
                    builder.Append(message, startIndex, num - startIndex);
                    builder.Append("\"\"");
                    startIndex = num + 1;
                }
                builder.Append(message, startIndex, message.Length - startIndex);
                builder.Append("\"");
                this.Write(builder.ToString());
            }
        }

        private void WriteFooter(TraceEventCache eventCache)
        {
            if (eventCache != null)
            {
                if (this.IsEnabled(TraceOptions.ProcessId))
                {
                    this.Write(eventCache.ProcessId.ToString(CultureInfo.InvariantCulture));
                }
                this.Write(this.Delimiter);
                if (this.IsEnabled(TraceOptions.LogicalOperationStack))
                {
                    this.WriteStackEscaped(eventCache.LogicalOperationStack);
                }
                this.Write(this.Delimiter);
                if (this.IsEnabled(TraceOptions.ThreadId))
                {
                    this.WriteEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
                }
                this.Write(this.Delimiter);
                if (this.IsEnabled(TraceOptions.DateTime))
                {
                    this.WriteEscaped(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
                }
                this.Write(this.Delimiter);
                if (this.IsEnabled(TraceOptions.Timestamp))
                {
                    this.Write(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
                }
                this.Write(this.Delimiter);
                if (this.IsEnabled(TraceOptions.Callstack))
                {
                    this.WriteEscaped(eventCache.Callstack);
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    this.Write(this.Delimiter);
                }
            }
            this.WriteLine("");
        }

        private void WriteHeader(string source, TraceEventType eventType, int id)
        {
            this.WriteEscaped(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffff"));
            this.Write(this.Delimiter);
            this.WriteEscaped(source);
            this.Write(this.Delimiter);
            this.Write(eventType.ToString());
            this.Write(this.Delimiter);
            this.Write(id.ToString(CultureInfo.InvariantCulture));
            this.Write(this.Delimiter);
        }

        private void WriteStackEscaped(Stack stack)
        {
            StringBuilder builder = new StringBuilder("\"");
            bool flag = true;
            foreach (object obj2 in stack)
            {
                int num;
                if (!flag)
                {
                    builder.Append(", ");
                }
                else
                {
                    flag = false;
                }
                string str = obj2.ToString();
                int startIndex = 0;
                while ((num = str.IndexOf('"', startIndex)) != -1)
                {
                    builder.Append(str, startIndex, num - startIndex);
                    builder.Append("\"\"");
                    startIndex = num + 1;
                }
                builder.Append(str, startIndex, str.Length - startIndex);
            }
            builder.Append("\"");
            this.Write(builder.ToString());
        }

        public string Delimiter
        {
            get
            {
                lock (this)
                {
                    if (!this.initializedDelim)
                    {
                        if (base.Attributes.ContainsKey("delimiter"))
                        {
                            this.delimiter = base.Attributes["delimiter"];
                        }
                        this.initializedDelim = true;
                    }
                }
                return this.delimiter;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Delimiter");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException("Parameter can't be an empty string", "Delimiter");
                }
                lock (this)
                {
                    this.delimiter = value;
                    this.initializedDelim = true;
                }
                if (this.delimiter == ",")
                {
                    this.secondaryDelim = ";";
                }
                else
                {
                    this.secondaryDelim = ",";
                }
            }
        }
    }
}
