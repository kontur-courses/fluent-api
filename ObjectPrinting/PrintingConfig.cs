using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<MemberInfo> _excludedMembers = new HashSet<MemberInfo>();
        private readonly HashSet<Type> _excludedTypes = new HashSet<Type>();

        private readonly Dictionary<MemberInfo, IPrintingConfig> _memberSerializers =
            new Dictionary<MemberInfo, IPrintingConfig>();

        private readonly Dictionary<Type, IPrintingConfig> _typeSerializers = new Dictionary<Type, IPrintingConfig>();
        
        private readonly Type[] _finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        public PrintingConfig<TOwner> Excluding<T>()
        {
            _excludedTypes.Add(typeof(T));
            return this;
        }

        public PrinterConfigSerialization<TOwner, TSerializator> With<TSerializator>()
        {
            var typeSerializationConfig = new PrinterConfigSerialization<TOwner, TSerializator>(this);
            _typeSerializers[typeof(TSerializator)] = typeSerializationConfig;
            return typeSerializationConfig;
        }

        public PrintingConfig<TOwner> With<TSerializator>(Func<TSerializator, string> serializer)
        {
            var typeSerializationConfig = new PrinterConfigSerialization<TOwner, TSerializator>(this);
            typeSerializationConfig.SetSerialization(serializer);
            _typeSerializers[typeof(TSerializator)] = typeSerializationConfig;
            return this;
        }

        public PrinterConfigSerialization<TOwner, TProp> ForMember<TProp>(
            Expression<Func<TOwner, TProp>> memberSelector)
        {
            var memberSerializationConfig = new PrinterConfigSerialization<TOwner, TProp>(this);
            var members = GetMemberFromSelectorByName(memberSelector);
            foreach (var member in members)
                _memberSerializers[member] = memberSerializationConfig;

            return memberSerializationConfig;
        }

        public PrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            _excludedMembers.UnionWith(GetMemberFromSelectorByName(memberSelector));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private IEnumerable<MemberInfo> GetMemberFromSelectorByName<TProp>(
            Expression<Func<TOwner, TProp>> memberSelector)
        {
            var memberName = memberSelector.Body.ToString().Split('.').Last();
            var members = typeof(TOwner).GetMember(memberName)
                .Where(member => member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property);

            if (!members.Any())
                throw new ArgumentException($"Field/Property {memberName} is not found!");

            return members;
        }

        private string PrintToString(object obj, int nestingLevel, int recursionLimit = 50)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            if (nestingLevel >= recursionLimit)
                return "";

            if (_finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                if (_excludedTypes.Contains(propertyInfo.PropertyType) || _excludedMembers.Contains(propertyInfo))
                    continue;

                sb.Append(identation + propertyInfo.Name + " = " + GetPartToAppend(propertyInfo, obj, nestingLevel));
            }

            return sb.ToString();
        }

        private string GetPartToAppend(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            var val = propertyInfo.GetValue(obj);
            if (_typeSerializers.ContainsKey(propertyInfo.PropertyType))
                return _typeSerializers[propertyInfo.PropertyType].PrintObject(val) + '\n';

            if (_memberSerializers.ContainsKey(propertyInfo))
                return _memberSerializers[propertyInfo].PrintObject(val) + '\n';

            return PrintToString(val,  nestingLevel + 1);
        }
    }
}