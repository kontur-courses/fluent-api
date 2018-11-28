namespace ObjectPrinterTests.TestClasses
{
    public class TestClass
    {
        private string privateField = "privateField";
        private string PrivateProperty => "PrivateProperty";

        protected string ProtectedField = "ProtectedField";
        protected string ProtectedProperty => "PrivateProperty";

        internal string InternalField = "InternalField";
        internal string InternalProperty => "InternalProperty";

        protected internal string ProtectedInternalField = "ProtectedInternalField";
        protected internal string ProtectedInternalProperty => "ProtectedInternalProperty";

        public string PublicField = "PublicField";
        public string PublicProperty => "PublicProperty";
    }
}