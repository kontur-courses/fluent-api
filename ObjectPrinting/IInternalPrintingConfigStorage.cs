using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting;

internal interface IInternalPrintingConfigStorage
{
    LinkedList<(Type Type, Func<object, string> Serializer)> AssignableTypeSerializers { get; }

    HashSet<Type> IgnoredTypesFromAssignableCheck { get; }

    Dictionary<MemberInfo, Func<object, string>> MemberSerializers { get; }

    HashSet<Type> TypeExcluding { get; }

    Dictionary<Type, Func<object, string>> TypeSerializers { get; }

    HashSet<MemberInfo> MemberExcluding { get; }
}