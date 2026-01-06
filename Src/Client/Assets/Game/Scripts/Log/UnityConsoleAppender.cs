using Common;
using log4net.Appender;
using log4net.Core;
using UnityEngine;

public class UnityConsoleAppender : AppenderSkeleton
{
    // 防止无限循环的标志位
    private static bool isLoggingFromAppender = false;

    public static bool IsLoggingFromAppender
    {
        get { return isLoggingFromAppender; }
    }

    protected override void Append(LoggingEvent loggingEvent)
    {
        // 设置标志位，防止无限循环
        isLoggingFromAppender = true;

        try
        {
            // 格式化日志消息
            string formattedMessage = RenderLoggingEvent(loggingEvent);

            // 根据日志级别选择合适的Unity控制台输出方法
            if (loggingEvent.Level >= Level.Error)
            {
                Log.Error(formattedMessage);
            }
            else if (loggingEvent.Level >= Level.Warn)
            {
                Log.Warning(formattedMessage);
            }
            else
            {
                Log.Info(formattedMessage);
            }
        }
        finally
        {
            // 清除标志位
            isLoggingFromAppender = false;
        }
    }
}
