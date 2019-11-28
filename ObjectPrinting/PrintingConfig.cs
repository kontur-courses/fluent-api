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
        private readonly HashSet<Type> typesExcluding = new HashSet<Type>();
        private readonly Dictionary<Type, IPropertySerializingConfig<TOwner>> typesSerializers = new Dictionary<Type, IPropertySerializingConfig<TOwner>>();
        private readonly List<Stack<MemberInfo>> propsExcluding = new List<Stack<MemberInfo>>();
        private readonly List<(Stack<MemberInfo> MemberPath, IPropertySerializingConfig<TOwner> Serializer)> propsSerializers = new List<(Stack<MemberInfo>, IPropertySerializingConfig<TOwner>)>();
        private readonly Dictionary<object, int> printedObjects = new Dictionary<object, int>();
        private int maxNestingLevel = 1000;
        private object ownerObject = null;
        private readonly Stack<MemberInfo> currentMemberPath = new Stack<MemberInfo>();

        public PrintingConfig()
        {
        }

        public PrintingConfig(TOwner obj)
        {
            ownerObject = obj;
        }

        public string PrintToString()
        {
            printedObjects.Clear();
            currentMemberPath.Clear();
            TryPrintToString(ownerObject, 0, out var objString);
            return objString;
        }

        public string PrintToString(TOwner obj)
        {
            ownerObject = obj;
            return PrintToString();
        }

        private bool TryPrintToString(object obj, int nestingLevel, out string objString)
        {
            if (obj == null)
            {
                objString = "null" + Environment.NewLine;
                return true;
            }

            if (typesExcluding.Contains(obj.GetType()))
            {
                objString = default;
                return false;
            }

            if (typesSerializers.TryGetValue(obj.GetType(), out var propertySerializingConfig))
            {
                objString = propertySerializingConfig.Serialize(obj);
                return true;
            }

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            if (finalTypes.Contains(obj.GetType()))
            {
                objString = obj + Environment.NewLine;
                return true;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            var alreadyPrinted = printedObjects.Keys.Contains(obj);
            var printedLevel = alreadyPrinted ? printedObjects[obj] : int.MaxValue;
            if (printedLevel >= nestingLevel)
            {
                if (!alreadyPrinted)
                    printedObjects.Add(obj, nestingLevel);

                if (nestingLevel == maxNestingLevel)
                {
                    sb.Append($"{identation}The maximum nesting level ({maxNestingLevel}) has been reached...");
                }
                else
                {
                    if (obj is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            if (TryPrintToString(item, nestingLevel + 1, out var itemString))
                                sb.Append($"{identation}Item = {itemString}");
                        }
                    }
                    else
                    {
                        var members = type.GetProperties().Concat<MemberInfo>(type.GetFields()).ToArray();
                        foreach (var member in members)
                        {
                            currentMemberPath.Push(member);

                            if (propsExcluding.Any(item => IsObjectsBranchMatchesMembersChain(item)))
                            {
                                currentMemberPath.Pop();
                                continue;
                            }

                            int propSerializerInd = propsSerializers.FindIndex(funcs => IsObjectsBranchMatchesMembersChain(funcs.MemberPath));
                            if (propSerializerInd >= 0)
                            {
                                sb.Append($"{identation}{member.Name} = {propsSerializers[propSerializerInd].Serializer.Serialize(GetMemberObject(obj, member))}");
                                currentMemberPath.Pop();
                                continue;
                            }

                            if (TryPrintToString(GetMemberObject(obj, member), nestingLevel + 1, out string propString))
                                sb.Append($"{identation}{member.Name} = {propString}");

                            currentMemberPath.Pop();
                        }
                    } 
                }
            }

            objString = sb.ToString();
            return true;
        }

        private object GetMemberObject(object owner, MemberInfo memberInfo)
        {
            return memberInfo is PropertyInfo property ? property.GetValue(owner)
                : memberInfo is FieldInfo field ? field.GetValue(owner)
                : throw new ArgumentException("Nested member must be field or property.");
        }

        private bool IsObjectsBranchMatchesMembersChain(Stack<MemberInfo> membersChain)
        {
            if (currentMemberPath.Count < membersChain.Count) return false;

            var pathIterator = currentMemberPath.GetEnumerator(); 
            var memberIterator = membersChain.GetEnumerator();

            MemberInfo pathItem = null;
            MemberInfo memberChainItem = null;
            while (pathIterator.MoveNext() && memberIterator.MoveNext())
            {
                pathItem = pathIterator.Current;
                memberChainItem = memberIterator.Current;
                if (pathItem.Name != memberChainItem.Name)
                    return false;
            }           

            return pathItem.ReflectedType == memberChainItem.ReflectedType;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            typesExcluding.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> SetMaxNestingLevel(int level)
        {
            maxNestingLevel = level;
            return this;
        }

        public PrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> func)
        {
            var membersChain = new Stack<MemberInfo>();

            Expression expression = func.Body;
            while (expression is MemberExpression memberAccessOperation)
            {
                membersChain.Push(memberAccessOperation.Member);
                expression = memberAccessOperation.Expression;
            }

            if (membersChain.Count == 0)
                throw new ArgumentException("Func must be a member access expression.");
            else
            {
                propsExcluding.Add(new Stack<MemberInfo>(membersChain)/*stack is reversing here*/);
                return this;
            }
        }

        public PropertySerializingConfig<TOwner, T> Serialize<T>()
        {
            var propSerializerConfig = new PropertySerializingConfig<TOwner, T>(this);
            typesSerializers.Add(typeof(T), propSerializerConfig);
            return propSerializerConfig;
        }

        public PropertySerializingConfig<TOwner, T> Serialize<T>(Expression<Func<TOwner, T>> func)
        {
            var membersChain = new Stack<MemberInfo>();

            Expression expression = func.Body;
            while (expression is MemberExpression memberAccessOperation)
            {
                membersChain.Push(memberAccessOperation.Member);
                expression = memberAccessOperation.Expression;
            }

            if (membersChain.Count == 0)
                throw new ArgumentException("Func must be a member access expression.");
            else
            {
                var propSerializerConfig = new PropertySerializingConfig<TOwner, T>(this);
                propsSerializers.Add((new Stack<MemberInfo>(membersChain)/*stack is reversing here*/, propSerializerConfig));
                return propSerializerConfig;
            }
        }
    }
}