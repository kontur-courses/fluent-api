using System;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> FinalTypes = new HashSet<Type>
        {
            typeof(int), 
            typeof(double), 
            typeof(float), 
            typeof(string),
            typeof(DateTime), 
            typeof(TimeSpan),
            typeof(Guid)
        };
        private ImmutableHashSet<Type> excludedTypes;
        private ImmutableHashSet<MemberInfo> excludedMembers;
        private readonly ImmutableDictionary<Type, Delegate> typeSerializers;
        private readonly ImmutableDictionary<MemberInfo, Delegate> memberSerializers;
        private HashSet<object> visited;

        private bool shouldSkipCycles = true;
        private bool shouldCyclesThrow = false;
        private bool shouldShowMessage = false;
        private string cycleReferenceMessage = "Cycle reference";

        public PrintingConfig()
        {
           excludedTypes = ImmutableHashSet.Create<Type>();
           excludedMembers = ImmutableHashSet.Create<MemberInfo>();
           typeSerializers = ImmutableDictionary.Create<Type, Delegate>();
           memberSerializers = ImmutableDictionary.Create<MemberInfo, Delegate>();
        }

        private PrintingConfig(PrintingConfig<TOwner> config)
        {
            excludedTypes = config.excludedTypes;
            excludedMembers = config.excludedMembers;
            memberSerializers = config.memberSerializers;
            typeSerializers = config.typeSerializers;
            visited = config.visited;
        }
        
        internal PrintingConfig(PrintingConfig<TOwner> config,
            Type type,
            Delegate serializer) : this(config)
        {
            typeSerializers = config.typeSerializers.Add(type, serializer);
        }

        internal PrintingConfig(PrintingConfig<TOwner> config,
            MemberInfo memberInfo,
            Delegate serializer) : this(config)
        {
            memberSerializers = config.memberSerializers.Add(memberInfo, serializer);
        }

        internal PrintingConfig(PrintingConfig<TOwner> config,
            bool shouldSkipCycles = false,
            bool shouldCyclesThrow = false,
            bool shouldShowMessage = false,
            string message = "Cycle reference") : this(config)
        {
            this.shouldCyclesThrow = shouldCyclesThrow;
            this.shouldShowMessage = shouldShowMessage;
            this.shouldSkipCycles = shouldSkipCycles;
            cycleReferenceMessage = message;
        }

        public string PrintToString(TOwner obj)
        {
            visited = new HashSet<object>();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (FinalTypes.Contains(type))
                return obj + Environment.NewLine;

            if (visited.Contains(obj))
            {
                if (shouldCyclesThrow)
                    throw new InvalidOperationException($"Cycle reference detected on {obj}");
                return cycleReferenceMessage + Environment.NewLine;
            }

            visited.Add(obj);
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            
            if (obj is IEnumerable collection)
                return PrintCollection(sb, collection, indentation, nestingLevel);

            PrintMembers(sb, type, obj, indentation, nestingLevel);

            visited.Remove(obj);

            return sb.ToString();
        }

        private void PrintMembers(StringBuilder sb, Type type, object obj, string indentation, int nestingLevel)
        {
            var members = type.GetMembers()
                .Where(member => member.MemberType == MemberTypes.Field 
                                 || member.MemberType == MemberTypes.Property)
                .Where(member => !excludedMembers.Contains(member)
                                && !excludedTypes.Contains(member.GetMemberType()));
            foreach (var memberInfo in members)
            {
                var tabulation = indentation + memberInfo.Name + " = ";
                var memberValue = memberInfo.GetValue(obj);
                if (visited.Contains(memberValue) && shouldSkipCycles)
                    continue;

                sb.Append(tabulation)
                    .Append(PrintMember(memberInfo, memberValue, nestingLevel));
            }
        }

        private string PrintMember(MemberInfo memberInfo, object memberValue, int nestingLevel)
        {
            if (typeSerializers.TryGetValue(memberInfo.GetMemberType(), out var serializer))
                return serializer.DynamicInvoke(memberValue) + Environment.NewLine;

            return memberSerializers.TryGetValue(memberInfo, out serializer)
                ? serializer.DynamicInvoke(memberValue) + Environment.NewLine
                : PrintToString(memberValue, nestingLevel + 1);
        }

        private string PrintCollection(StringBuilder sb, 
            IEnumerable collection, 
            string indentation,
            int nestingLevel)
        {
            foreach (var item in collection)
            {
                sb.Append(indentation)
                    .Append(PrintToString(item, nestingLevel));
            }

            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes = excludedTypes.Add(typeof(T));
            return new PrintingConfig<TOwner>(this);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> memberSelector)
        {
            var memberInfo = memberSelector.GetMemberInfoFromExpression();
            excludedMembers = excludedMembers.Add(memberInfo);
            return new PrintingConfig<TOwner>(this);
        }

        public CycleReferencePrintingConfig<TOwner> ForCycles()
        {
            return new CycleReferencePrintingConfig<TOwner>(this);
        }

        public IMemberPrintingConfig<TOwner, T> Printing<T>()
        {
            return new TypePrintingConfig<TOwner, T>(this);
        }

        public IMemberPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> memberSelector)
        {
            var memberInfo = memberSelector.GetMemberInfoFromExpression();
            return new MemberPrintingConfig<TOwner, T>(this, memberInfo);
        }
    }
}