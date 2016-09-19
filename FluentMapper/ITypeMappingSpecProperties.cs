using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentMapping
{
    public interface ITypeMappingSpecProperties<TTgt, TSrc>
    {
        IEnumerable<Action<TTgt, TSrc>> MappingActions { get; }
        IEnumerable<PropertyInfo> SourceProperties { get; }
        IEnumerable<PropertyInfo> TargetProperties { get; }
        IAssembler<TTgt, TSrc> Assembler { get; }
    }

    public interface ITypeMappingSpecTransforms<TTgt, TSrc>
    {
        TypeMappingSpec<TTgt, TSrc> WithSourceProperties(IEnumerable<PropertyInfo> sourceProperties);

        TypeMappingSpec<TTgt, TSrc> WithTargetProperties(IEnumerable<PropertyInfo> targetProperties);

        TypeMappingSpec<TTgt, TSrc> WithMappingActions(IEnumerable<Action<TTgt, TSrc>> mappingActions);

        TypeMappingSpec<TTgt, TSrc> WithAssembler(IAssembler<TTgt, TSrc> assembler);
    }
}