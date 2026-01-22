using System;

namespace cwlogs.util;

public static class LambdaUtils
{
    public static string CleanLambdaMessage(string message)
    {
        // Example: 2026-01-22T08:24:52.392Z 0fbd43a3-1435-4cc0-8209-2de3b5f1a461 INFO
        var parts = message.Split(['\t', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3)
        {
            if (parts[0].Contains('T') && parts[0].EndsWith('Z') && parts[1].Contains('-'))
            {
                var foundCount = 0;
                var currentPos = 0;

                while (foundCount < 3 && currentPos < message.Length)
                {
                    while (currentPos < message.Length && !char.IsWhiteSpace(message[currentPos])) currentPos++;
                    foundCount++;
                    while (currentPos < message.Length && char.IsWhiteSpace(message[currentPos])) currentPos++;
                }

                if (foundCount >= 3)
                {
                    return message.Substring(currentPos).TrimStart();
                }
            }
        }

        return message;
    }
}
