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
			return "null" + Environment.NewLine;

		var type = obj.GetType();

		if (type.IsClass)
			if (!printedMembers.Add(obj))
				return $"Cycling reference{Environment.NewLine}";

		if (type.IsSerializable)
			return obj + Environment.NewLine;

		var instances = InstanceInfo.GetInstances(obj, BindingFlags.Public | BindingFlags.Instance);
		var printedProperties = PrintInstances(instances, obj, nestingLevel + 1);

		return $"{type.Name}{Environment.NewLine}{printedProperties}";
	}

	private string PrintInstances(IEnumerable<InstanceInfo> instances, object obj, int nestingLevel)
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

		void Append(string typeName, object? value, bool newLine = true)
		{
			var ending = newLine ? Environment.NewLine : string.Empty;
			printedInstances.Append($"{tabulation}{typeName} = {value}{ending}");
		}
	}
}