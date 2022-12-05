using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinter.PrintingConfigs;

public class PrintingConfig<TOwner>
{
	public PrintingConfig()
	{
		ExcludingTypes = new HashSet<Type>();
		ExcludingProperties = new HashSet<MemberInfo>();
		TypesPrintingRules = new Dictionary<Type, Delegate>();
		MembersPrintingRules = new Dictionary<MemberInfo, Delegate>();
		CyclicReferenceRule = _ => throw new ArgumentException("Cycling reference");
		MaxCollectionElementPrinted = int.MaxValue;
	}

	public HashSet<Type> ExcludingTypes { get; }
	public HashSet<MemberInfo> ExcludingProperties { get; }

	public Dictionary<Type, Delegate> TypesPrintingRules { get; }
	public Dictionary<MemberInfo, Delegate> MembersPrintingRules { get; }

	public Func<object, string> CyclicReferenceRule { get; private set; }

	public int MaxCollectionElementPrinted { get; private set; }

	public InstancePrintingConfig<TOwner, TPropType> Printing<TPropType>()
	{
		return new TypePrintingConfig<TOwner, TPropType>(this);
	}

	public InstancePrintingConfig<TOwner, TPropType> Printing<TPropType>(
		Expression<Func<TOwner, TPropType>> memberSelector)
	{
		var memberInfo = GetMemberInfo(memberSelector);
		return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
	}

	public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
	{
		var memberInfo = GetMemberInfo(memberSelector);
		ExcludingProperties.Add(memberInfo);
		return this;
	}

	public PrintingConfig<TOwner> Excluding<TPropType>()
	{
		ExcludingTypes.Add(typeof(TPropType));
		return this;
	}

	public PrintingConfig<TOwner> CyclingRefPrinting(Func<object, string> rule)
	{
		CyclicReferenceRule = rule;
		return this;
	}

	public PrintingConfig<TOwner> MaxElementInCollection(int max)
	{
		MaxCollectionElementPrinted = max;
		return this;
	}

	public ObjectPrinter<TOwner> Build()
	{
		return new ObjectPrinter<TOwner>(this);
	}

	private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
	{
		var lambda = memberSelector as LambdaExpression;

		if (lambda.Body is not MemberExpression body)
			throw new ArgumentException();

		return body.Member;
	}
}