using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ObjectPrinter.PrintingConfigs;

namespace ObjectPrinter;

public class ObjectPrinter<TFor>
{
	private readonly PrintingConfig<TFor> config;
	private HashSet<object> printedMembers;


	public ObjectPrinter(PrintingConfig<TFor> config)
	{
		this.config = config;
	}

	private static string NewLine => Environment.NewLine;

	public static PrintingConfig<TFor> Configuration()
	{
		return new PrintingConfig<TFor>();
	}

	public string PrintToString(TFor obj)
	{
		printedMembers = new HashSet<object>();
		return PrintObject(obj, 0);
	}

	private string PrintObject(object? obj, int nestingLevel)
	{

		if (obj is null)
			return $"null{NewLine}";

		var type = obj.GetType();

		if (IsSimple(type))
			return $"{obj}{NewLine}";

		if (!printedMembers.Add(obj))
			return config.CyclicReferenceRule(obj);

		if (obj is IEnumerable collection)
			return PrintCollection(collection, nestingLevel);
		
		var instances = InstanceInfo.GetInstances(obj, BindingFlags.Public | BindingFlags.Instance);
		var printedProperties = PrintInstances(instances, nestingLevel + 1);

		return $"{type.Name}{NewLine}{printedProperties}";
	}

	private static bool IsSimple(Type type)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			// nullable type, check if the nested type is simple.
			return IsSimple(type.GetGenericArguments()[0]);
		}

		return IsPrimitive(type);
	}

	private static bool IsPrimitive(Type type) => type.IsPrimitive || type.IsValueType || type == typeof(string);
	

	private string PrintInstances(IEnumerable<InstanceInfo> instances, int nestingLevel)
	{
		var tabulation = new string('\t', nestingLevel);
		var printedInstances = new StringBuilder();

		foreach (var instance in instances)
		{
			if (IsExcludedType(instance.Type))
				continue;

			if (IsExcludedMember(instance.MemberInfo))
				continue;

			var value = instance.Value;
			if (InstanceContainSelfPrintingRule(instance.MemberInfo))
			{
				var result = config.MembersPrintingRules[instance.MemberInfo].DynamicInvoke(value);
				Append(instance.Name, result.ToString());
				continue;
			}

			if (TypeContainSelfPrintingRule(instance.Type))
			{
				var result = config.TypesPrintingRules[instance.Type].DynamicInvoke(value);
				Append(instance.Name, result.ToString());
				continue;
			}

			var printedValue = PrintObject(value, nestingLevel + 1);
			Append(instance.Name, printedValue, false);
		}

		return printedInstances.ToString();

		bool IsExcludedType(Type type) => config.ExcludingTypes.Contains(type);
		bool IsExcludedMember(MemberInfo member) => config.ExcludingProperties.Contains(member);
		bool TypeContainSelfPrintingRule(Type type) => config.TypesPrintingRules.ContainsKey(type);
		bool InstanceContainSelfPrintingRule(MemberInfo member) => config.MembersPrintingRules.ContainsKey(member);

		void Append(string typeName, string value, bool withNewLine = true)
		{
			if (value == string.Empty) return;

			var ending = withNewLine ? NewLine : string.Empty;
			printedInstances.Append($"{tabulation}{typeName} = {value}{ending}");
		}
	}

	private string PrintCollection(IEnumerable collection, int nestingLevel)
	{
		if (collection is IDictionary dict)
			return PrintDictionary(dict, nestingLevel);

		const string separator = ", ";
		var start = collection is Array ? "[" : "(";
		var end = collection is Array ? "]" : ")";

		var sb = new StringBuilder();
		var printedElementsCount = 0;
		foreach (var element in collection)
		{
			if (printedElementsCount >= config.MaxCollectionElementPrinted)
			{
				sb.Append($"...{separator}");
				break;
			}

			printedElementsCount++;
			sb.Append($"{element}{separator}");
		}

		if (sb.Length > start.Length + separator.Length + end.Length)
			sb.Remove(sb.Length - separator.Length, separator.Length);

		return $"{start}{sb}{end}{NewLine}";
	}

	private string PrintDictionary(IDictionary dict, int nestingLevel)
	{
		const string separator = " : ";
		var start = $"{{{NewLine}";
		var end = $"}}{NewLine}";
		var tabulation = new string('\t', nestingLevel + 1);

		var sb = new StringBuilder();
		foreach (DictionaryEntry dictionaryEntry in dict)
			sb.Append($"{tabulation}{dictionaryEntry.Key}{separator}{dictionaryEntry.Value}{NewLine}");

		return $"{start}{sb}{end}";
	}
}