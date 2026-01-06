using Common;
using log4net.Appender;
using log4net.Core;

public class UnityConsoleAppender : AppenderSkeleton
{
    // é˜²æ­¢æ— é™å¾ªçŽ¯çš„æ ‡å¿—ä½?
    private static bool isLoggingFromAppender = false;

    public static bool IsLoggingFromAppender
    {
        get { return isLoggingFromAppender; }
    }

    protected override void Append(LoggingEvent loggingEvent)
    {
        // è®¾ç½®æ ‡å¿—ä½?ï¼Œé˜²æ­¢æ— é™å¾ªçŽ¯
        isLoggingFromAppender = true;

        try
        {
            // æ ¼å¼åŒ–æ—¥å¿—æ¶ˆæ?
            string formattedMessage = RenderLoggingEvent(loggingEvent);

            // æ ¹æ®æ—¥å¿—çº§åˆ«é€‰æ‹©åˆé€‚çš„UnityæŽ§åˆ¶å°è¾“å‡ºæ–¹æ³?
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
            // æ¸…é™¤æ ‡å¿—ä½?
            isLoggingFromAppender = false;
        }
    }
}
