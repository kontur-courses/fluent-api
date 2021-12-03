using System;
using System.Globalization;

namespace Homework.CultureContexts
{
    public interface ICultureConfigurator<TOwner>
    {
        public ICultureIntermediateConfigurator<TOwner> For<TType>(CultureInfo currentCulture) where TType : IConvertible;
        public IPrinterConfigurator<TOwner> ForAllOthers(CultureInfo culture);
    }
}