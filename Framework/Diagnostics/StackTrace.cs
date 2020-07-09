using System;
using System.Text;

namespace Framework.Diagnostics
{
    /// <inheritdoc />
    public static class StackTrace
    {
        /// <inheritdoc />
        public static string GetStackFrameString(int depth = 6)
        {
            try
            {
                var sb = new StringBuilder();
                var stackTrace = new System.Diagnostics.StackTrace();
                var stackFrames = stackTrace.GetFrames();
                var count = Math.Min(stackFrames.Length, depth);
                if (count > 1)
                {
                    var method = stackFrames[1].GetMethod();
                    sb.Append(method.ReflectedType.Name).Append(".").AppendLine(method.Name);
                    for (int i = 2; i < count; i++)
                    {
                        method = stackFrames[i].GetMethod();
                        sb.Append(method.ReflectedType.Name).Append(".").AppendLine(method.Name);
                    }
                }
                return sb.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
