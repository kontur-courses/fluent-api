﻿using System;

namespace ObjectPrinting.Extensions;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propConfig, 
        int maxLen)
    {
        if (propConfig.Property is null)
            throw new ArgumentException("The name of the property is not specified.");
        
        propConfig.ParentConfig.AddLengthProperty(propConfig.Property, maxLen);
        
        return propConfig.ParentConfig;
    }
}