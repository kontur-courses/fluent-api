using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.HomeWork
{
    public class PrintingConfig<TOwner>
    {
        private const string NullToString = "null";

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(long), typeof(Guid)
        };

        private readonly HashSet<object> serializedMembers = new HashSet<object>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();


        //private readonly HashSet<string> excludedFieldsProperties = new HashSet<string>();
        private readonly HashSet<MemberInfo> excludedFieldsProperties = new HashSet<MemberInfo>();


        private string pinnedPropertyName; //!!!

        private MemberInfo pinnedPropertyName1;

        private readonly Dictionary<Type, Delegate> specialSerializationsForTypes =
            new Dictionary<Type, Delegate>();


        //private readonly Dictionary<string, Delegate> specialSerializationsForFieldsProperties =
            //new Dictionary<string, Delegate>();  //!!!

        private readonly Dictionary<MemberInfo, Delegate> specialSerializationsForFieldsProperties1 =
            new Dictionary<MemberInfo, Delegate>();


        private CultureInfo culture = CultureInfo.InvariantCulture;
        private int resultStartIndex;
        private int resultLength = int.MaxValue;



        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (serializedMembers.Contains(obj))
                return ReturnWhenCyclic(nestingLevel);

            var sb = new StringBuilder();
            if (obj == null)
                return NullToString + Environment.NewLine;

            var objType = obj.GetType();


            if ((finalTypes.Contains(objType) || objType.IsPrimitive) && !excludedTypes.Contains(objType))
                return ReturnWhenFinal(obj);


            var identation = new string('\t', nestingLevel + 1);
            sb.AppendLine(objType.Name);
            var fieldsAndProperties = GetFieldsAndProperties(objType);

            foreach (var memberInfo in fieldsAndProperties)
            {
                PrintMemberInformation(memberInfo, obj, sb, nestingLevel, identation);
            }
            return (nestingLevel != 0) ? sb.ToString() : GetTrimString(sb);
        }

        //private Delegate MakeSerializeDelegate(SerializationMemberInfo memberSerialization)
        private Delegate MakeSerializeDelegate(MemberInfo memberSerialization)
        {
            //var a = (MemberInfo)memberSerialization.MemberType;
            /*
            if (specialSerializationsForFieldsProperties.ContainsKey(memberSerialization.MemberName))
            {
                return specialSerializationsForFieldsProperties[memberSerialization.MemberName];
            }
            */

            var dsf = specialSerializationsForFieldsProperties1.Keys.Select(x => x.Name);
            var b = memberSerialization.Name.ToString();
            var c = "";

            if (specialSerializationsForFieldsProperties1.Keys.ToList().Contains(memberSerialization))
                return specialSerializationsForFieldsProperties1[memberSerialization];
            //}











            var a = memberSerialization.GetType();

            if (specialSerializationsForTypes.ContainsKey(memberSerialization.GetType()))
                return specialSerializationsForTypes[memberSerialization.GetType()];


            /*
            if (specialSerializationsForTypes.ContainsKey(memberSerialization.MemberType))
                return specialSerializationsForTypes[memberSerialization.MemberType];
            */
            return null;
        }

        private void PrintMemberInformation(MemberInfo memberInfo, object obj, StringBuilder sb, int nestingLevel, string identation)
        {
            SerializationMemberInfo memberSerialization = default;
            if (memberInfo is PropertyInfo propertyInfo)
            {
                var indexParameters = propertyInfo.GetIndexParameters();
                if (indexParameters.Length != 0)
                {
                    PrintIndexes(obj, sb, identation, propertyInfo.Name);
                    return;
                }
                memberSerialization =
                    new SerializationMemberInfo(propertyInfo, obj);
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                memberSerialization =
                    new SerializationMemberInfo(fieldInfo, obj);
            }

            //|| excludedFieldsProperties.Contains(memberSerialization.MemberName))
            if (memberSerialization == null || excludedTypes.Contains(memberSerialization.MemberType) || excludedFieldsProperties.Contains(memberInfo)) 
                return;

            var serializeDelegate = MakeSerializeDelegate(memberSerialization.MemberType);
            serializedMembers.Add(obj);

            if (serializeDelegate != null)
                sb.Append(FormSerializeDelegateString(identation, nestingLevel, memberSerialization,
                    serializeDelegate));
            else
                sb.Append(FormSerializeString(identation, nestingLevel, memberSerialization));
        }

        private string FormSerializeDelegateString(string identation, int nestingLevel,
            SerializationMemberInfo memberSerialization, Delegate serializeDelegate)
        {
            return identation + memberSerialization.MemberName + " = " +
                   PrintToString(serializeDelegate.DynamicInvoke(memberSerialization.MemberValue),
                       nestingLevel + 1);
        }

        private string FormSerializeString(string identation, int nestingLevel,
            SerializationMemberInfo memberSerialization)
        {
            return identation + memberSerialization.MemberName + " = " +
                   PrintToString(memberSerialization.MemberValue,
                       nestingLevel + 1);
        }

        private static IEnumerable<MemberInfo> GetFieldsAndProperties(Type objType)
        {
            var fields =
                objType.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>();
            var props =
                objType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>();
            return fields.Union(props);
        }

        private string GetTrimString(StringBuilder resultString)
        {
            return resultString.ToString()
                [resultStartIndex..Math.Min(resultLength, resultString.Length)];
        }

        private string ReturnWhenCyclic(int nestingLevel)
        {
            return ((nestingLevel != 0) ? "this (parentObj)"
                : GetTrimString(new StringBuilder("this (parentObj)"))) + "\r\n";
        }

        private string ReturnWhenFinal(object obj)
        {
            if (obj is IFormattable formatObj)
                return formatObj.ToString(null, culture) + Environment.NewLine;
            return obj + Environment.NewLine;
        }

        private void PrintIndexes(object obj, StringBuilder sb, string identation, string name) //!!!
        {
            if (!(obj is ICollection cast))
                sb.Append(name);
            else
            {
                var counter = 0;
                sb.Append(identation + name + " =\r\n");
                foreach (var parameter in cast)
                {
                    var value = parameter.ToString();
                    sb.Append(identation + "\t" + "Index " + counter + " = " + value + "\r\n");
                    counter++;
                }
            }
        }



        //Expression<Func<TOwner, TTarget>> expression where expression : MemberExpression



        public PrintingConfig<TOwner> ExcludedType<TExType>()
        {
            excludedTypes.Add(typeof(TExType));
            return this;
        }

        public PrintingConfig<TOwner> ExcludedProperty<TTarget>(Expression<Func<TOwner, TTarget>> propertyNameExpression)
        {
            if (propertyNameExpression.Body is MemberExpression propertyName)
            {
                var inputName = (propertyName).Member.Name;
                var member = propertyName.Member;

                if (GetFieldsAndProperties(typeof(TOwner)).Contains(member))
                    excludedFieldsProperties.Add(member);


                //if ((GetFieldsAndProperties(typeof(TOwner))).Select(x => x.Name).Contains(inputName))
                    //excludedFieldsProperties.Add(inputName);


                return this;
            }
            throw new InvalidExpressionException("Need member expression(which giving access to the field)");
            /*
            if (propertyNameExpression.Body is UnaryExpression unExpression)
            {
                if (!(unExpression.Operand is MemberExpression))
                    throw new InvalidExpressionException("Need member expression(which giving access to the field)");
                var inputName = ((MemberExpression)unExpression.Operand).Member.Name;
                if ((GetFieldsAndProperties(typeof(TOwner))).Select(x => x.Name).Contains(inputName))
                    excludedFieldsProperties.Add(inputName);
            }
            return this;
            */
        }

        public PrintingConfig<TOwner> SpecialSerializationType<TType>(Func<TType, string> specialSerializationForType) //!!!
        {
            specialSerializationsForTypes[typeof(TType)] = specialSerializationForType;
            return this;
        }

        public PrintingConfig<TOwner> SpecialSerializationField<TFieldType>(Func<TFieldType, string> serialization) //!!!
        {
            if (pinnedPropertyName1 != null)
            {
                //specialSerializationsForFieldsProperties[pinnedPropertyName] = serialization;
                specialSerializationsForFieldsProperties1[pinnedPropertyName1] = serialization;
                pinnedPropertyName1 = null;
            }
            return this;
        }

        

        public PrintingConfig<TOwner> SetCulture(Expression<Func<TOwner, CultureInfo>> inputCulture)
        {
            if (!(inputCulture.Body is NewExpression))
                throw new InvalidExpressionException("Need new Expression(creating a new object)");
            var cultureName = ((NewExpression)inputCulture.Body).Arguments.First().ToString();
            culture = new CultureInfo(cultureName[1..^1]);
            return this;
        }



        public PrintingConfig<TOwner> PinProperty(Expression<Func<TOwner, object>> propertyNameExpression)
        {
            //string propertyName = null;

            MemberInfo propertyMember = null;


            if (propertyNameExpression.Body is UnaryExpression unExpression)
            {
                if (!(unExpression.Operand is MemberExpression propertyMemberExpression))
                    throw new InvalidExpressionException("Need member expression(which giving access to the field)");
                //propertyName = ((MemberExpression) unExpression.Operand).Member.Name;
                //propertyMember = propertyMemberExpression.Member;
                propertyMember = propertyMemberExpression.Member;
            }




            /*
            if (!((GetFieldsAndProperties(typeof(TOwner))).Select(x => x.Name).Contains(propertyName)))
            {
                pinnedPropertyName = null;
            }
            else
                pinnedPropertyName = propertyName;
            */

            if ((GetFieldsAndProperties(typeof(TOwner))).Contains(propertyMember))
                pinnedPropertyName1 = propertyMember;
            else
            {
                pinnedPropertyName1 = null;
            }
            return this;
        }

        public PrintingConfig<TOwner> Trim<TStart,TLength>(Expression<Func<TOwner, Tuple<TStart, TLength>>> trimBorders) //!!!
        {
            if (!(trimBorders.Body is NewExpression))
                throw new InvalidExpressionException("Need new Expression(creating a new object)");

            var start = int.Parse(((NewExpression) trimBorders.Body).Arguments[0].ToString());
            var length = int.Parse(((NewExpression)trimBorders.Body).Arguments[1].ToString());
            
            if (start < 0 || length < 0)
                throw new ArgumentException();
            
            resultStartIndex = start;
            resultLength = length;
            return this;
        }
    }
}