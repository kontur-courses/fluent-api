using System;

namespace ObjectPrinting.Tests;

internal static class TestOutput
{
    public static string Lines(params string[] lines)
    {
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
