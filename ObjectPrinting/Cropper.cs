using System;

namespace ObjectPrinting
{
    public class Cropper<TOwner>
    {
        public String EditingPropertyInfoName;
        public PrintingConfig<TOwner> PrintingConfig; 
        
        public PrintingConfig<TOwner> Crop(int length)
        {
            PrintingConfig.PropertiesToCrop.Add(EditingPropertyInfoName, length);
            return PrintingConfig;
        }
    }
}