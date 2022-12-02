using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using ObjectPrinting.Solved;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public SerializerSettings settings;
        public Type configType;

        private Type[] finalTypes = new[]
            {
                typeof(int),
                typeof(double),
                typeof(float),
                typeof(string),
                typeof(Guid),
                typeof(DateTime),
                typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            configType = typeof(TOwner);
            settings = new SerializerSettings();
        }

        public TypeConfig<TOwner, TPropType> SelectType<TPropType>()
        {
            return new TypeConfig<TOwner, TPropType>(this, settings);
        }

        public MemberConfig<TOwner, TPropType> SelectProperty<TPropType>(Expression<Func<TOwner, TPropType>> memberExpression)
        {
            var member = ((MemberExpression)memberExpression.Body).Member;
            return new MemberConfig<TOwner, TPropType>(this, member, settings);
        }

        public string PrintToString(object obj, int nestingLevel)
        {
            if (obj == string.Empty) return string.Empty;
            var serializer = new Serializer(settings, obj, nestingLevel);
            var objectString = serializer.ToString();

            return objectString;
        }
    }
}