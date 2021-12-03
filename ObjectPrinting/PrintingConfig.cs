using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly ExcludingConfig excludingConfig = new ExcludingConfig();
        private readonly ObjectSerializer objectSerializer = new ObjectSerializer();
        private readonly CycleConfig<TOwner> cycleConfig;
        private Func<int, string> indentationCreator;
        private string endLine;
        
        private readonly HashSet<object> visited = new HashSet<object>();
        
        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            this.cycleConfig = new CycleConfig<TOwner>(this);
            SetIndentation(level => new string('\t', level + 1));
            SetEndLine(Environment.NewLine);
        }


        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
        {
            return new MemberPrintingConfig<TOwner, TMemberType>(this);
        }

        public CycleConfig<TOwner> OnCycleFound()
        {
            return cycleConfig;
        }

        public PrintingConfig<TOwner> SetIndentation(Func<int, string> levelToIndentation)
        {
            this.indentationCreator = levelToIndentation;
            return this;
        }

        public PrintingConfig<TOwner> SetEndLine(string newLine)
        {
            this.endLine = newLine;
            return this;
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                return new MemberPrintingConfig<TOwner, TMemberType>(this, member);
            }

            throw new InvalidOperationException();
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
            {
                var excludedMember = memberExpression.Member;
                excludingConfig.Exclude(excludedMember);
                return this;
            }

            throw new InvalidOperationException();
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingConfig.Exclude<TPropType>();
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> SpecializeSerialization(MemberInfo member, Func<object, string> serializer)
        {
            objectSerializer.Specialize(member, serializer);
            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + GetEndLine();
            
            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj + GetEndLine();
            
            visited.Add(obj);
            
            var builder = new StringBuilder();
            
            builder
                .Append(type.Name)
                .Append(GetEndLine());

            if (obj is IEnumerable collection)
                SerializeCollection(builder, collection, nestingLevel);
            else
                SerializeMembers(builder, obj, nestingLevel);
            
            visited.Remove(obj);

            return builder.ToString();
        }

        private void SerializeCollection(StringBuilder builder, IEnumerable collection, int nestingLevel)
        {
            var indentation = GetIndentation(nestingLevel);
            visited.Add(collection);

            foreach (var element in collection)
            {
                builder
                    .Append(indentation)
                    .Append(PrintToString(element, nestingLevel + 1));
            }
        }

        private void SerializeMembers(StringBuilder builder, object obj, int nestingLevel)
        {
            var indentation = GetIndentation(nestingLevel);
            var type = obj.GetType();

            foreach (var memberInfo in type.GetFieldsAndProperties().Where(x => !MemberIsExcluded(x)))
            {
                builder.Append(indentation)
                    .Append(memberInfo.Name)
                    .Append(" = ");
                var value = memberInfo.GetValue(obj);

                if (IsAlreadyPrinted(value))
                    cycleConfig.AppendCycleText(builder).Append(GetEndLine());

                else if (objectSerializer.TryGetSerializationFunction(memberInfo, out var serializer))
                {
                    builder.Append(serializer.DynamicInvoke(value))
                        .Append(GetEndLine());
                }

                else
                {
                    builder
                        .Append(PrintToString(value, nestingLevel + 1));
                }
            }
        }

        private bool MemberIsExcluded(MemberInfo member)
        {
            return excludingConfig.IsExcluded(member);
        }

        private bool IsAlreadyPrinted(object obj)
        {
            return visited.Contains(obj);
        }

        private string GetEndLine() => this.endLine;
        
        private string GetIndentation(int nestingLevel) => indentationCreator(nestingLevel);
    }
}