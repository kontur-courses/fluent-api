using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting.Core
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly Dictionary<Type, Delegate> _alternativeSerializationByTypes;
        private readonly Dictionary<string, Delegate> _alternativeSerializationByNames;
        private readonly HashSet<Type> _excludingTypes;
        private readonly HashSet<string> _excludingNames;
        private readonly HashSet<object> _parents;
        private const int MaxNestingLevel = 10;

        Dictionary<Type, Delegate> IPrintingConfig.AlternativeSerializationByTypes =>
            _alternativeSerializationByTypes;

        Dictionary<string, Delegate> IPrintingConfig.AlternativeSerializationByNames
            => _alternativeSerializationByNames;

        public PrintingConfig()
        {
            _alternativeSerializationByTypes = new Dictionary<Type, Delegate>();
            _alternativeSerializationByNames = new Dictionary<string, Delegate>();
            _excludingTypes = new HashSet<Type>();
            _excludingNames = new HashSet<string>();
            _parents = new HashSet<object>();
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
        {
            throw new NotImplementedException();
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            throw new NotImplementedException();
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            throw new NotImplementedException();
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            throw new NotImplementedException();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}