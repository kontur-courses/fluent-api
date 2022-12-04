using System;

namespace ObjectPrinting
{
    public class Trimmer<TOwner>
    {
        public String EditingPropertyInfoName;
        public PrintingConfig<TOwner> PrintingConfig; 
        
        public PrintingConfig<TOwner> Trim(int length)
        {
            PrintingConfig.PropertiesToCrop.Add(EditingPropertyInfoName, length);
            return PrintingConfig;
        }
    }
}