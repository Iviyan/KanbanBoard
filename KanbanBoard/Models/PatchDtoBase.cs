//#define FodyAutoProperty

#if FodyAutoProperty
using AutoProperties;
#endif

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KanbanBoard.Models;

public abstract class PatchDtoBase
{
#if FodyAutoProperty
    [InterceptIgnore]
#endif
    public HashSet<string> ChangedProperties { get; set; } = new();
    
    public bool IsFieldPresent(string propertyName) 
        => ChangedProperties.Contains(propertyName);

    protected void SetHasProperty([CallerMemberName] string propertyName = null!)
        => ChangedProperties.Add(propertyName);

#if FodyAutoProperty
    [SetInterceptor]
    protected void SetValue<T>(string name, T genricNewValue, out T refToBackingField)
    {
        refToBackingField = genricNewValue;
        ChangedProperties.Add(name);
    }
    [GetInterceptor]
    protected T GetValue<T>(ref T refToBackingField) => refToBackingField;
#endif
}