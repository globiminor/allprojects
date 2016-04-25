using System;
using System.Text;

namespace Cards
{
    public class ExceptionUtils
    {
        public static string ToString(Exception e)
        {
            return ToString(e, string.Empty);
        }

        public static string ToString(Exception e, string message)
        {
            string separator = string.Format("{0}-->", Environment.NewLine);
            return ToString(e, message, separator);
        }

        public static string ToString(Exception e, string message, string separator)
        {
            return ToString(e, message, separator, false);
        }

        public static string ToString(Exception e, string message, string separator, bool ignoreStackTrace)
        {
            if (message == null)
            {
                message = string.Empty;
            }

            StringBuilder sb = new StringBuilder(message);
            Exception ie = e;
            while (ie != null)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }
                sb.Append(ie.Message);
                ie = ie.InnerException;
            }
            if (!ignoreStackTrace)
            {
                sb.AppendLine();
                sb.AppendLine("StackTrace:");
                ie = e;
                while (ie != null)
                {
                    sb.AppendLine(ie.StackTrace);
                    ie = ie.InnerException;
                }
            }
            return sb.ToString();
        }
    }
}
