using System;

namespace ObjectPrinting
{
    public static class TypeTaker
    {
        public static string GetWholeTypeName(this Type type)
        {
            var name = type.Name;
            if (!type.IsGenericType) return name.ToString();
            var iBacktick = name.IndexOf('`');
            if (iBacktick > 0)
            {
                name = name.Remove(iBacktick);
            }

            name += "<";
            var typeParameters = type.GetGenericArguments();
            for (var i = 0; i < typeParameters.Length; ++i)
            {
                var typeParamName = GetWholeTypeName(typeParameters[i]);
                name += (i == 0 ? typeParamName : "," + typeParamName);
            }

            return name + ">";
        }
    }
}