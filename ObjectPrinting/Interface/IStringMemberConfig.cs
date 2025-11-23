namespace ObjectPrinting.Interface;

public interface IStringMemberConfig<TOwner>
{
    PrintingConfig<TOwner> Trim(int maxLength);
}