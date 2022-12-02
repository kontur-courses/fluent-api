using System.Reflection;

namespace ObjectPrinter;

public class InstanceInfo
{
	private InstanceInfo(PropertyInfo property, object value)
		: this(property.PropertyType, property.Name, value, property)
	{
	}

	private InstanceInfo(FieldInfo property, object value)
		: this(property.FieldType, property.Name, value, property)
	{
	}

	private InstanceInfo(Type type, string name, object value, MemberInfo memberInfo)
	{
		Type = type;
		Name = name;
		Value = value;
		MemberInfo = memberInfo;
	}

	public Type Type { get; }
	public string Name { get; }
	public object Value { get; }
	public MemberInfo MemberInfo { get; }

	public static List<InstanceInfo> GetInstances(object obj, BindingFlags flags)
	{
		var properties = obj
			.GetType()
			.GetProperties(flags)
			.Select(p => new InstanceInfo(p, p.GetValue(obj)));

		var fields = obj
			.GetType()
			.GetFields(flags)
			.Select(f => new InstanceInfo(f, f.GetValue(obj)));

		return properties.Concat(fields).ToList();
	}
}