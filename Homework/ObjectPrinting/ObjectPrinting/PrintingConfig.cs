using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();

        private readonly Dictionary<Type, Func<object, string>> alternateMemberSerialisatorByType =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, string>> cultureInfoApplierByType =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<MemberInfo, Func<object, string>> individualSetUpFuncByMemberInfo =
            new Dictionary<MemberInfo, Func<object, string>>();

        private readonly Dictionary<MemberInfo, int> maxValueLengthByMemberInfo =
            new Dictionary<MemberInfo, int>();

        private readonly int serialiseDepth;
        private readonly int sequencesMaxLength;
        private int mutualMaxMembersLength;
        private MemberInfo currentSettingUpMember;

        public PrintingConfig(int serialiseDepth, int sequencesMaxLength)
        {
            this.serialiseDepth = serialiseDepth;
            this.sequencesMaxLength = sequencesMaxLength;
        }

        void IPrintingConfig.SetCultureInfoApplierForNumberType<TNumber>(Func<TNumber, string> cultureInfoApplier)
        {
            if (currentSettingUpMember is null)
                cultureInfoApplierByType[typeof(TNumber)] = ApplyCultureInfo;
            else
                individualSetUpFuncByMemberInfo[currentSettingUpMember] = ApplyCultureInfo;

            string ApplyCultureInfo(object number) => cultureInfoApplier((TNumber)number);
        }

        void IPrintingConfig.SetMaxValueLengthForStringMember(int maxValueLength)
        {
            if (currentSettingUpMember is null)
                mutualMaxMembersLength = maxValueLength;
            else
                maxValueLengthByMemberInfo[currentSettingUpMember] = maxValueLength;
        }

        void IPrintingConfig.SetAlternateMemberSerialisator<TMemberType>(
            Func<TMemberType, string> alternateSerialisator)
        {
            if (currentSettingUpMember is null)
                alternateMemberSerialisatorByType[typeof(TMemberType)] = SerialiseMember;
            else
                individualSetUpFuncByMemberInfo[currentSettingUpMember] = SerialiseMember;

            string SerialiseMember(object memberValue) => alternateSerialisator((TMemberType)memberValue);
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
        {
            currentSettingUpMember = null;

            return new MemberPrintingConfig<TOwner, TMemberType>(this);
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            currentSettingUpMember = GetMemberInfoFromMemberExpression(memberSelector);

            return new MemberPrintingConfig<TOwner, TMemberType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var memberInfo = GetMemberInfoFromMemberExpression(memberSelector);

            excludedMembers.Add(memberInfo);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            excludedTypes.Add(typeof(TMemberType));

            return this;
        }

        private static MemberInfo GetMemberInfoFromMemberExpression<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression))
                throw new ArgumentException("Passed expression has to represent accessing a property of field",
                                            nameof(memberSelector));
            return memberExpression.Member;
        }

        public string PrintToString(TOwner printedObject) => PrintToString(printedObject, 0);

        private string PrintToString(object printedObject, int nestingLevel)
        {
            if (printedObject is null)
                return PrintingConfigHelper.NullRepresentation + Environment.NewLine;

            var objectRuntimeType = printedObject.GetType();

            if (excludedTypes.Contains(objectRuntimeType))
                return string.Empty;

            if (nestingLevel > serialiseDepth)
                throw new ApplicationException(
                    $"Was detected nesting level more than specified serialiseDepth ({serialiseDepth})");

            if (alternateMemberSerialisatorByType.TryGetValue(objectRuntimeType, out var alternateSerialisator))
                return alternateSerialisator(printedObject) + Environment.NewLine;

            if (cultureInfoApplierByType.TryGetValue(objectRuntimeType, out var cultureInfoApplier))
                return cultureInfoApplier(printedObject) + Environment.NewLine;

            if (PrintingConfigHelper.IsFinalType(objectRuntimeType))
                return printedObject + Environment.NewLine;

            switch (printedObject)
            {
                case IDictionary dictionary:
                    return SerialiseDictionary(dictionary, nestingLevel);
                case IEnumerable enumerable:
                    return SerialiseEnumerable(enumerable, nestingLevel);
            }

            var objectSerialisationBuilder = new StringBuilder();

            objectSerialisationBuilder.AppendLine(objectRuntimeType.Name);

            objectSerialisationBuilder.Append(PrintAllMembers(printedObject, objectRuntimeType, nestingLevel));

            return objectSerialisationBuilder.ToString();
        }

        private string PrintAllMembers(object printedObject, Type objectRuntimeType, int nestingLevel)
        {
            var membersBuilder = new StringBuilder();
            var indentation = new string(PrintingConfigHelper.Indentation, nestingLevel + 1);

            var validTypeMembers = GetPropertiesAndFields(objectRuntimeType).Where(pInfo => !IsExcludedMember(pInfo));

            foreach (var memberInfo in validTypeMembers)
            {
                var memberValue = memberInfo.GetValue(printedObject);

                var memberValueSerialisation = SerialiseMemberValue(memberInfo, memberValue);

                memberValueSerialisation = TrimValueIfNecessary(memberInfo, memberValueSerialisation);

                var memberSerialisation = string.Concat(indentation,
                                                        memberInfo.Name,
                                                        " = ",
                                                        memberValueSerialisation);

                membersBuilder.Append(memberSerialisation);
            }

            return membersBuilder.ToString();

            string SerialiseMemberValue(MemberInfo memberInfo, object memberValue) =>
                individualSetUpFuncByMemberInfo.TryGetValue(memberInfo, out var setUpFunc)
                    ? setUpFunc(memberValue) + Environment.NewLine
                    : PrintToString(memberValue, nestingLevel + 1);
        }

        private static IEnumerable<MemberInfo> GetPropertiesAndFields(Type type) =>
            type.GetMembers()
                .Where(memberInfo => memberInfo.MemberType == MemberTypes.Property ||
                                     memberInfo.MemberType == MemberTypes.Field);

        private bool IsExcludedMember(MemberInfo memberInfo) =>
            excludedTypes.Contains(memberInfo.GetMemberType()) ||
            excludedMembers.Contains(memberInfo);

        private string TrimValueIfNecessary(MemberInfo memberInfo, string memberValueSerialisation)
        {
            if (memberInfo.GetMemberType() != typeof(string)) return memberValueSerialisation;

            var maxMemberLength = maxValueLengthByMemberInfo.TryGetValue(memberInfo, out var maxValueLength)
                                      ? maxValueLength
                                      : mutualMaxMembersLength;

            if (maxMemberLength > 0)
                return TruncateString(memberValueSerialisation, maxMemberLength) +
                       $"...{Environment.NewLine}";

            return memberValueSerialisation;
        }

        private static string TruncateString(string str, int maxLength) =>
            string.IsNullOrEmpty(str) ? str : str.Substring(0, Math.Min(str.Length, maxLength));

        // ReSharper disable once SuggestBaseTypeForParameter
        private string SerialiseDictionary(IDictionary dictionary, int nestingLevel)
        {
            var dictionaryBuilder = new StringBuilder();

            if (dictionary.Count > sequencesMaxLength)
                throw new ApplicationException($@"Was detected sequence with length ({dictionary.Count
                                                   }) more than specified sequencesMaxLength ({sequencesMaxLength})");

            foreach (DictionaryEntry entry in dictionary)
            {
                var key = PrintToString(entry.Key, nestingLevel).TrimLineTerminator();
                var value = PrintToString(entry.Value, nestingLevel).TrimLineTerminator();

                key = WrapIfCollection(key, entry.Key);
                value = WrapIfCollection(value, entry.Value);

                dictionaryBuilder.Append($"[{key}]: {value} ");
            }

            dictionaryBuilder.Remove(dictionaryBuilder.Length - 1, 1); // removed redundant whitespace
            dictionaryBuilder.AppendLine();

            return dictionaryBuilder.ToString();
        }

        private string SerialiseEnumerable(IEnumerable enumerable, int nestingLevel) =>
            string.Join(' ', enumerable.Cast<object>()
                            .Select((obj, index) =>
                            {
                                if (index > sequencesMaxLength)
                                    throw new ApplicationException(
                                        $@"Was detected sequence with length more than specified sequencesMaxLength ({
                                            sequencesMaxLength})");

                                return WrapIfCollection(PrintToString(obj, nestingLevel).TrimLineTerminator(), obj);
                            }))
          + Environment.NewLine;

        private static string WrapIfCollection(string objectSerialisation, object wrappedObject)
        {
            var objectType = wrappedObject.GetType();

            return wrappedObject is IEnumerable && !PrintingConfigHelper.IsFinalType(objectType)
                       ? $"{objectType}({objectSerialisation})"
                       : objectSerialisation;
        }
    }
}