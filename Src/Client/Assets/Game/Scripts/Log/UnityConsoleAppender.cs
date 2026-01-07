using log4net.Appender;
using log4net.Core;
using UnityEngine;

/// <summary>
/// Unity 控制台 Appender：将 log4net 日志输出到 Unity 控制台
/// </summary>
public class UnityConsoleAppender : AppenderSkeleton
{
    /// <summary>
    /// 防止无限循环的标志位
    /// 注意：此标志由 UnityLogger 检查以防止递归日志
    /// </summary>
    private static bool isLoggingFromAppender = false;

    /// <summary>
    /// 获取当前是否正在从 Appender 输出日志
    /// </summary>
    public static bool IsLoggingFromAppender
    {
        get { return isLoggingFromAppender; }
    }

    /// <summary>
    /// 输出日志到 Unity 控制台
    /// 注意：必须使用 Debug.Log* 而不是 Log.*，否则不会输出到 Unity 控制台
    /// UnityLogger 会通过检查 IsLoggingFromAppender 标志来防止无限循环
    /// </summary>
    protected override void Append(LoggingEvent loggingEvent)
    {
        // 设置标志位，防止无限循环
        isLoggingFromAppender = true;

        try
        {
            // 格式化日志消息
            string formattedMessage = RenderLoggingEvent(loggingEvent);

            // 根据日志级别选择合适的 Unity 控制台输出方法
            if (loggingEvent.Level >= Level.Error)
            {
                Debug.LogError(formattedMessage);
            }
            else if (loggingEvent.Level >= Level.Warn)
            {
                Debug.LogWarning(formattedMessage);
            }
            else
            {
                Debug.Log(formattedMessage);
            }
        }
        finally
        {
            // 清除标志位
            isLoggingFromAppender = false;
        }
    }
}
