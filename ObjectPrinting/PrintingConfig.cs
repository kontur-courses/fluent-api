using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> _finalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(bool)
        };

        private readonly HashSet<Type> _specialFinalTypes = new()
        {
            typeof(ITuple), typeof(KeyValuePair<,>)
        };

        private readonly HashSet<Type> _excludedTypes = new();

        private readonly HashSet<IEnumerable<string>> _excludedMembers = new(
            new EnumerableEqualityComparer<string>());


        private readonly Dictionary<Type, Func<object, string>> _customPrintingTypes = new();

        private readonly Dictionary<IEnumerable<string>, Func<object, string>> _customPrintingMembers = new(
            new EnumerableEqualityComparer<string>());

        public string PrintToString(TOwner obj) =>
            PrintToString(obj, 0, ImmutableList<string>.Empty, ImmutableHashSet<object>.Empty);

        private string PrintToString(
            object? obj, int nestingLevel,
            ImmutableList<string> memberPath,
            ImmutableHashSet<object> parents
        )
        {
            if (obj is null)
                return "null";
            if (parents.Contains(obj))
                return "[Loop reference]";
            parents = parents.Add(obj);
            if (TryFinalPrintToString(obj, memberPath, out var result))
                return result!;
            if (obj is IEnumerable enumerable)
                return PrintToStringEnumerable(enumerable, nestingLevel, memberPath, parents);
            return obj.GetType().Name + Environment.NewLine +
                   string.Join(Environment.NewLine, PrintToStringMembers(obj, nestingLevel, memberPath, parents));
        }

        private bool TryFinalPrintToString(object obj, ImmutableList<string> memberPath, out string? result)
        {
            result = default;
            var type = obj.GetType();

            if (_customPrintingMembers.TryGetValue(memberPath, out var memberPrint))
                result = memberPrint(obj);
            else if (_customPrintingTypes.TryGetValue(type, out var typePrint))
                result = typePrint(obj);
            else if (_finalTypes.Contains(type))
                result = obj.ToString();
            else
            {
                if (type.IsGenericType)
                    type = type.GetGenericTypeDefinition();
                if (_specialFinalTypes.Any(final => final.IsAssignableFrom(type)))
                    result = obj.ToString();
            }

            return result is not null;
        }

        private string PrintToStringEnumerable(
            IEnumerable enumerable,
            int nestingLevel,
            ImmutableList<string> memberPath,
            ImmutableHashSet<object> parents
        )
        {
            var printed = enumerable
                .Cast<object?>()
                .Select(obj => PrintToString(obj, nestingLevel + 1, memberPath, parents))
                .ToArray();

            if (!printed.All(obj => obj.Contains(Environment.NewLine)))
                return '{' + string.Join(", ", printed) + '}';

            var builder = new StringBuilder()
                .AppendLine("{");
            foreach (var str in printed)
                builder
                    .Append('\t', nestingLevel + 1)
                    .Append(str)
                    .AppendLine();

            return builder.Append('}').ToString();
        }

        private IEnumerable<string> PrintToStringMembers(
            object obj, int nestingLevel,
            ImmutableList<string> memberPath,
            ImmutableHashSet<object> parents
        )
        {
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var member in obj.GetType().GetProperties().OfType<MemberInfo>().Concat(obj.GetType().GetFields()))
            {
                if (_excludedTypes.Contains(member.GetFieldPropertyType()))
                    continue;
                var currentMemberPath = memberPath.Add(member.Name);
                if (_excludedMembers.Contains(currentMemberPath))
                    continue;

                var printValue = PrintToString(
                    member.GetFieldPropertyValue(obj),
                    nestingLevel + 1,
                    currentMemberPath,
                    parents
                );

                yield return $"{indentation}{member.Name} = {printValue}";
            }
        }

        public PrintingConfig<TOwner> Exclude<TMember>()
        {
            _excludedTypes.Add(typeof(TMember));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            _excludedMembers.Add(selector.GetMemberPath());
            return this;
        }

        public MemberPrintingConfig<TOwner, TMember> Printing<TMember>()
        {
            var memberCfg = new MemberPrintingConfig<TOwner, TMember>(this);
            _customPrintingTypes[typeof(TMember)] = obj => ((IMemberPrintingConfig<TOwner>) memberCfg).Print(obj);
            return memberCfg;
        }

        public MemberPrintingConfig<TOwner, TMember> Printing<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            var memberCfg = new MemberPrintingConfig<TOwner, TMember>(this);
            _customPrintingMembers[selector.GetMemberPath()] =
                obj => ((IMemberPrintingConfig<TOwner>) memberCfg).Print(obj);

            return memberCfg;
        }
    }
}