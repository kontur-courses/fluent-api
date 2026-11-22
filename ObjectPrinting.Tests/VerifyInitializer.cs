using System.Runtime.CompilerServices;

namespace ObjectPrinting.Tests;

public static class VerifyInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.FixNewlinesOnRead();
    }
}