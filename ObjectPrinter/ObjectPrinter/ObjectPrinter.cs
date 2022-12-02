using System.Collections;
using System.Reflection;
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

		if (obj is ICollection collection)
			return PrintCollection(collection, nestingLevel);

		if (type.IsClass)
			if (!printedMembers.Add(obj))
				return $"Cycling reference{NewLine}";

		if (type.IsSerializable)
			return $"{obj}{NewLine}";

		var instances = InstanceInfo.GetInstances(obj, BindingFlags.Public | BindingFlags.Instance);
		var printedProperties = PrintInstances(instances, nestingLevel + 1);

		return $"{type.Name}{NewLine}{printedProperties}";
	}

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
				Append(instance.Name, result);
				continue;
			}

			if (TypeContainSelfPrintingRule(instance.Type))
			{
				var result = config.TypesPrintingRules[instance.Type].DynamicInvoke(value);
				Append(instance.Name, result);
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

		void Append(string typeName, object? value, bool withNewLine = true)
		{
			var ending = withNewLine ? NewLine : string.Empty;
			printedInstances.Append($"{tabulation}{typeName} = {value}{ending}");
		}
	}

	private string PrintCollection(ICollection collection, int nestingLevel)
	{
		if (collection is IDictionary dict)
			return PrintDictionary(dict, nestingLevel);

		const string separator = ", ";
		var start = collection is Array ? "[" : "(";
		var end = collection is Array ? "]" : ")";

		var sb = new StringBuilder();
		foreach (var element in collection) sb.Append($"{element}{separator}");

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