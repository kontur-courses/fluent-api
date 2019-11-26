using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
	public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
	{
		private readonly PrintingConfig<TOwner> _parentConfig;
		private readonly Dictionary<Type, Func<object, string>> _typesPrintingBehaviors;
		private readonly Dictionary<string, Func<object, string>> _propertiesPrintingBehaviors;
		private readonly string _memberName;

		public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, string memberName = null)
		{
			_parentConfig = parentConfig;
			var printingConfig = (IPrintingConfig<TOwner>) parentConfig;
			_typesPrintingBehaviors = printingConfig.TypesPrintingBehaviors;
			_propertiesPrintingBehaviors = printingConfig.PropertiesPrintingBehaviors;
			_memberName = memberName;
		}

		public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
		{
			if (_memberName == null)
				_typesPrintingBehaviors.Add(typeof(TPropType), obj => print((TPropType) obj));
			else
				_propertiesPrintingBehaviors.Add(_memberName, obj => print((TPropType) obj));
			return _parentConfig;
		}

		PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => _parentConfig;
		string IPropertyPrintingConfig<TOwner, TPropType>.MemberName => _memberName;
	}

	public interface IPropertyPrintingConfig<TOwner, TPropType>
	{
		PrintingConfig<TOwner> ParentConfig { get; }
		string MemberName { get; }
	}
}