using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
	public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
	{
		private readonly PrintingConfig<TOwner> _printingConfig;
		private readonly Dictionary<Type, Func<object, string>> _typesPrintingBehaviors;
		private readonly Expression<Func<TOwner, TPropType>> _member;
		private IPropertyPrintingConfig<TOwner, TPropType> propertyPrintingConfigImplementation;

		public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
			Expression<Func<TOwner, TPropType>> member = null)
		{
			_printingConfig = printingConfig;
			_typesPrintingBehaviors = ((IPrintingConfig<TOwner>) _printingConfig).TypesPrintingBehaviors;
			_member = member;
		}

		public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
		{
			_typesPrintingBehaviors.Add(typeof(TPropType), obj => print((TPropType)obj));
			return _printingConfig;
		}

		PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => _printingConfig;
		Expression<Func<TOwner, TPropType>> IPropertyPrintingConfig<TOwner, TPropType>.Member => _member;
	}

	public interface IPropertyPrintingConfig<TOwner, TPropType>
	{
		PrintingConfig<TOwner> ParentConfig { get; }
		Expression<Func<TOwner, TPropType>> Member { get; }
	}
}