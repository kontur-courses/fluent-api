using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> _finalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> _excludedTypes = new();

        private readonly HashSet<IEnumerable<string>> _excludedMembers = new(
            new EnumerableEqualityComparer<string>());


        private readonly Dictionary<Type, Func<object, string>> _customPrintingTypes = new();

        private readonly Dictionary<IEnumerable<string>, Func<object, string>> _customPrintingMembers = new(
            new EnumerableEqualityComparer<string>());

        public string PrintToString(TOwner obj) =>
            PrintToString(obj, 0, ImmutableList<string>.Empty);

        private string PrintToString(object? obj, int nestingLevel, ImmutableList<string> memberPath)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (_customPrintingMembers.TryGetValue(memberPath, out var printFunc))
                return printFunc(obj) + Environment.NewLine;

            if (_customPrintingTypes.TryGetValue(type, out var printFunc1))
                return printFunc1(obj) + Environment.NewLine;

            if (_finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var builder = new StringBuilder();
            builder.AppendLine(type.Name);
            foreach (var member in type.GetProperties())
            {
                if (_excludedTypes.Contains(member.PropertyType))
                    continue;
                var currentMemberPath = memberPath.Add(member.Name);
                if (_excludedMembers.Contains(currentMemberPath))
                    continue;
                builder
                    .Append('\t', nestingLevel + 1)
                    .Append(member.Name)
                    .Append(" = ")
                    .Append(PrintToString(member.GetValue(obj), nestingLevel + 1, currentMemberPath));
            }

            return builder.ToString();
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