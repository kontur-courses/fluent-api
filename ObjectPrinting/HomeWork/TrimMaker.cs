namespace ObjectPrinting.HomeWork
{
    public static class TrimMaker
    {
        public static string MakeTrim(string identation, Borders typeBorders, SerializationMemberInfo serializationMemberInfo)
        {
            return identation + serializationMemberInfo.MemberName + " = " +
                   serializationMemberInfo.MemberValue.ToString().Substring(typeBorders.Start, typeBorders.Length) + "\r\n";
        }
    }
}
