using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using log4net;
using log4net.Core;
using System.Reflection;
using Common;

public static class UnityLogger
{
    // Unity堆栈追踪助手
    public static class StackTraceHelper
    {
        private static string _currentStackTrace;

        public static void SetCurrentStackTrace(string stackTrace)
        {
            _currentStackTrace = stackTrace;
        }

        public static string GetCurrentStackTrace()
        {
            return _currentStackTrace;
        }

        public static bool HasStackTrace()
        {
            return !string.IsNullOrEmpty(_currentStackTrace);
        }

        public static void ClearStackTrace()
        {
            _currentStackTrace = null;
        }
    }

    public static void Init()
    {
        Application.logMessageReceived += onLogMessageReceived;
        global::Common.Log.Init("Unity");
    }

    private static ILog log = LogManager.GetLogger("Unity");

    // 内置的循环检测标志，无需依赖外部类
    private static bool isProcessingLog = false;

    private static void onLogMessageReceived(string condition, string stackTrace, UnityEngine.LogType type)
    {
        // 如果正在处理日志，直接返回防止无限循环
        if (isProcessingLog)
        {
            return;
        }

        // 设置处理标志
        isProcessingLog = true;

        try
        {
            // 额外检查UnityConsoleAppender状态（如果可用）
            try
            {
                var appenderType = System.Type.GetType("UnityConsoleAppender");
                if (appenderType != null)
                {
                    var isLoggingProperty = appenderType.GetProperty("IsLoggingFromAppender",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (isLoggingProperty != null)
                    {
                        bool isLoggingFromAppender = (bool)isLoggingProperty.GetValue(null);
                        if (isLoggingFromAppender)
                        {
                            return;
                        }
                    }
                }
            }
            catch
            {
                // 如果UnityConsoleAppender不可用，继续使用内置的循环检测
            }

            ProcessLogMessage(condition, stackTrace, type);
        }
        finally
        {
            // 确保标志被重置
            isProcessingLog = false;
        }
    }

    private static void ProcessLogMessage(string condition, string stackTrace, UnityEngine.LogType type)
    {

        // 缓存当前的Unity堆栈信息，供Common.Log使用
        StackTraceHelper.SetCurrentStackTrace(stackTrace);

        // 解析堆栈获取真正的调用者信息
        CallerInfo callerInfo = GetCallerInfo(stackTrace);
        string message = condition;

        // 根据日志类型决定是否附加堆栈信息
        // Info和Warning级别不包含堆栈，Error和Fatal级别包含堆栈
        bool shouldIncludeStackTrace = (type == LogType.Error || type == LogType.Exception);

        if (shouldIncludeStackTrace && !string.IsNullOrEmpty(stackTrace))
        {
            message = string.Format("{0}\r\nStackTrace:\r\n{1}", condition, stackTrace.Replace("\n", "\r\n"));
        }

        Level logLevel;
        switch (type)
        {
            case LogType.Error:
                logLevel = Level.Error;
                break;
            case LogType.Assert:
                logLevel = Level.Debug;
                break;
            case LogType.Exception:
                logLevel = Level.Fatal;
                break;
            case LogType.Warning:
                logLevel = Level.Warn;
                break;
            default:
                logLevel = Level.Info;
                break;
        }

        // 构建具有位置信息的LoggingEvent
        log4net.Core.LocationInfo locationInfo = new log4net.Core.LocationInfo(
            callerInfo.ClassName,
            callerInfo.MethodName,
            callerInfo.FileName,
            callerInfo.LineNumber
        );

        LoggingEvent loggingEvent = new LoggingEvent(
            typeof(UnityLogger),
            log.Logger.Repository,
            log.Logger.Name,
            logLevel,
            message,
            null
        );

        // 使用反射设置位置信息
        SetLocationInfo(loggingEvent, locationInfo);

        log.Logger.Log(loggingEvent);

        // 清理堆栈信息缓存
        StackTraceHelper.ClearStackTrace();
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

    private static CallerInfo GetCallerInfo(string stackTrace)
    {
        CallerInfo info = new CallerInfo();

        if (string.IsNullOrEmpty(stackTrace))
            return info;

        // 解析堆栈，找到第一个非UnityEngine的调用
        string[] lines = stackTrace.Split('\n');
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (!string.IsNullOrEmpty(trimmedLine) &&
                !trimmedLine.Contains("UnityEngine.Debug") &&
                !trimmedLine.Contains("UnityLogger") &&
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
