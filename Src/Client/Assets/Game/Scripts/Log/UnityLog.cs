using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using log4net.Core;
using UnityEngine;

namespace Common
{
    public static class UnityLog
    {
        private static ILog log;

        public static void Init(string name)
        {
            log = LogManager.GetLogger(name);
        }

        public static void Info(object message)
        {
            LogWithLocation(Level.Info, message?.ToString() ?? "", false);
        }

        public static void InfoFormat(string format, object arg0)
        {
            LogWithLocation(Level.Info, string.Format(format, arg0), false);
        }

        public static void InfoFormat(string format, object arg0, object arg1)
        {
            LogWithLocation(Level.Info, string.Format(format, arg0, arg1), false);
        }

        public static void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            LogWithLocation(Level.Info, string.Format(format, arg0, arg1, arg2), false);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            LogWithLocation(Level.Info, string.Format(format, args), false);
        }

        public static void Warning(object message)
        {
            LogWithLocation(Level.Warn, message?.ToString() ?? "", true);
        }

        public static void WarningFormat(string format, object arg0)
        {
            LogWithLocation(Level.Warn, string.Format(format, arg0), true);
        }

        public static void WarningFormat(string format, object arg0, object arg1)
        {
            LogWithLocation(Level.Warn, string.Format(format, arg0, arg1), true);
        }

        public static void WarningFormat(string format, object arg0, object arg1, object arg2)
        {
            LogWithLocation(Level.Warn, string.Format(format, arg0, arg1, arg2), true);
        }

        public static void WarningFormat(string format, params object[] args)
        {
            LogWithLocation(Level.Warn, string.Format(format, args), true);
        }

        public static void Error(object message)
        {
            LogWithLocation(Level.Error, message?.ToString() ?? "", true);
        }

        public static void ErrorFormat(string format, object arg0)
        {
            LogWithLocation(Level.Error, string.Format(format, arg0), true);
        }

        public static void ErrorFormat(string format, object arg0, object arg1)
        {
            LogWithLocation(Level.Error, string.Format(format, arg0, arg1), true);
        }

        public static void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            LogWithLocation(Level.Error, string.Format(format, arg0, arg1, arg2), true);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            LogWithLocation(Level.Error, string.Format(format, args), true);
        }

        public static void Fatal(object message)
        {
            LogWithLocation(Level.Fatal, message?.ToString() ?? "", true);
        }

        public static void FatalFormat(string format, object arg0)
        {
            LogWithLocation(Level.Fatal, string.Format(format, arg0), true);
        }

        public static void FatalFormat(string format, object arg0, object arg1)
        {
            LogWithLocation(Level.Fatal, string.Format(format, arg0, arg1), true);
        }

        public static void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            LogWithLocation(Level.Fatal, string.Format(format, arg0, arg1, arg2), true);
        }

        public static void FatalFormat(string format, params object[] args)
        {
            LogWithLocation(Level.Fatal, string.Format(format, args), true);
        }

        private static void LogWithLocation(Level level, string message, bool includeStackTrace)
        {
            if (log == null)
                return;

            // 获取调用者信息
            CallerInfo callerInfo = GetCallerInfo();

            // 如果需要包含堆栈信息，则使用Unity的堆栈信息
            string finalMessage = message;
            if (includeStackTrace)
            {
                string unityStackTrace = UnityLogger.StackTraceHelper.GetCurrentStackTrace();
                if (!string.IsNullOrEmpty(unityStackTrace))
                {
                    finalMessage = message + Environment.NewLine + "StackTrace:" + Environment.NewLine + unityStackTrace;
                }
                else
                {
                    finalMessage = message + Environment.NewLine + "StackTrace:" + Environment.NewLine + Environment.StackTrace;
                }
            }

            // 构建具有位置信息的LoggingEvent
            log4net.Core.LocationInfo locationInfo = new log4net.Core.LocationInfo(
                callerInfo.ClassName,
                callerInfo.MethodName,
                callerInfo.FileName,
                callerInfo.LineNumber
            );

            LoggingEvent loggingEvent = new LoggingEvent(
                typeof(UnityLog),
                log.Logger.Repository,
                log.Logger.Name,
                level,
                finalMessage,
                null
            );

            // 使用反射设置位置信息
            SetLocationInfo(loggingEvent, locationInfo);

            log.Logger.Log(loggingEvent);
        }

        private static void SetLocationInfo(LoggingEvent loggingEvent, log4net.Core.LocationInfo locationInfo)
        {
            var field = typeof(LoggingEvent).GetField("m_locationInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(loggingEvent, locationInfo);
            }
        }

        private struct CallerInfo
        {
            public string ClassName;
            public string MethodName;
            public string FileName;
            public string LineNumber;
        }

        private static CallerInfo GetCallerInfo()
        {
            CallerInfo info = new CallerInfo();

            try
            {
                // 首先尝试使用Unity的堆栈信息
                string unityStackTrace = UnityLogger.StackTraceHelper.GetCurrentStackTrace();
                if (!string.IsNullOrEmpty(unityStackTrace))
                {
                    info = ParseUnityStackTrace(unityStackTrace);
                    if (!string.IsNullOrEmpty(info.ClassName))
                    {
                        return info;
                    }
                }

                // 回退到标准的StackTrace方法
                StackTrace stackTrace = new StackTrace(true);
                StackFrame[] frames = stackTrace.GetFrames();

                if (frames != null)
                {
                    // 跳过当前类的方法，找到真正的调用者
                    for (int i = 0; i < frames.Length; i++)
                    {
                        StackFrame frame = frames[i];
                        MethodBase method = frame.GetMethod();

                        if (method != null && method.DeclaringType != typeof(UnityLog))
                        {
                            info.ClassName = method.DeclaringType?.FullName ?? "";
                            info.MethodName = method.Name ?? "";
                            info.FileName = System.IO.Path.GetFileName(frame.GetFileName()) ?? "";
                            info.LineNumber = frame.GetFileLineNumber().ToString();
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 如果获取堆栈信息失败，使用默认值
                info.ClassName = "Unknown";
                info.MethodName = "Unknown";
                info.FileName = "Unknown";
                info.LineNumber = "0";
            }

            return info;
        }

        private static CallerInfo ParseUnityStackTrace(string stackTrace)
        {
            CallerInfo info = new CallerInfo();

            if (string.IsNullOrEmpty(stackTrace))
                return info;

            // 解析Unity堆栈，找到第一个非UnityEngine和非Log的调用
            string[] lines = stackTrace.Split('\n');
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (!string.IsNullOrEmpty(trimmedLine) &&
                    !trimmedLine.Contains("UnityEngine.Debug") &&
                    !trimmedLine.Contains("UnityLogger") &&
                    !trimmedLine.Contains("Common.Log") &&
                    !trimmedLine.Contains("UnityLog") &&
                    trimmedLine.Contains(" (at "))
                {
                    // 提取方法名和文件位置
                    int atIndex = trimmedLine.IndexOf(" (at ");
                    if (atIndex > 0)
                    {
                        string methodInfo = trimmedLine.Substring(0, atIndex);
                        string fileInfo = trimmedLine.Substring(atIndex + 5).TrimEnd(')');

                        // 解析方法信息 (ClassName:MethodName)
                        int colonIndex = methodInfo.LastIndexOf(':');
                        if (colonIndex > 0)
                        {
                            info.ClassName = methodInfo.Substring(0, colonIndex);
                            info.MethodName = methodInfo.Substring(colonIndex + 1);
                        }
                        else
                        {
                            info.MethodName = methodInfo;
                        }

                        // 提取文件名和行号
                        int fileColonIndex = fileInfo.LastIndexOf(':');
                        if (fileColonIndex > 0)
                        {
                            string fullFileName = fileInfo.Substring(0, fileColonIndex);
                            info.LineNumber = fileInfo.Substring(fileColonIndex + 1);

                            // 只保留文件名，不要完整路径
                            info.FileName = System.IO.Path.GetFileName(fullFileName);
                        }
                    }
                    break;
                }
            }

            return info;
        }
    }
}
