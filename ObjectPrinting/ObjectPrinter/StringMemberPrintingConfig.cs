using System;
using System.Reflection;

namespace ObjectPrinting.ObjectPrinter;

public class StringPrintConfig<TOwner>(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo)
    : MemberPrintingConfig<TOwner, string>(printingConfig, memberInfo)
{
    public PrintingConfig<TOwner> StringTrimmedToLength(int length) //по сути уже не нужен, так как дублирует поведение TrimmedToLength.
    {                                                               //Просто хотелось соблюсти формальности, чтобы этот метод отображался только у string.
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        printingConfig.TrimmedMembers[memberInfo] = length;
        return PrintingConfig;
    }
}