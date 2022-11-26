using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
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

        private readonly HashSet<IEnumerable<string>> _excludedMembers = new(new EnumerableEqualityComparer<string>());

        public string PrintToString(TOwner obj) =>
            PrintToString(obj, 0, ImmutableList<string>.Empty);

        private string PrintToString(object? obj, int nestingLevel, ImmutableList<string> memberPath)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

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

        public MemberPrintingConfig<TOwner> Printing<TMember>()
        {
            return new MemberPrintingConfig<TOwner>(this);
        }

        public MemberPrintingConfig<TOwner> Printing<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            return new MemberPrintingConfig<TOwner>(this);
        }
    }
}