namespace ObjectPrinting.UnitTests;

public class For_IgnoreMembers_CycleInheritance_Type
{
    public For_IgnoreMembers_CycleInheritance_Type()
    {
        Children = new(this);
    }

    public NestedType Children { get; set; }

    public class NestedType
    {
        public NestedType(For_IgnoreMembers_CycleInheritance_Type parent)
        {
            Parent = parent;
        }

        public For_IgnoreMembers_CycleInheritance_Type Parent { get; set; }
    }
}