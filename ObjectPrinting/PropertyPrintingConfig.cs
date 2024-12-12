using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
	private readonly PrintingConfig<TOwner> parentConfig;
	private readonly MemberInfo? propertyMemberInfo;

	internal PrintingConfig<TOwner> ParentConfig => parentConfig;
	internal MemberInfo? PropertyMemberInfo => propertyMemberInfo;

	public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, MemberInfo? propertyMemberInfo)
	{
		this.parentConfig = parentConfig;
		this.propertyMemberInfo = propertyMemberInfo;
	}

	public PrintingConfig<TOwner> Using(Func<TPropType, string> serialize)
	{
		parentConfig.AddPropertySerializer(propertyMemberInfo, serialize);

		return parentConfig;
	}
}