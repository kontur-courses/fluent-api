using System.Reflection;

namespace ObjectPrinting.Actions;

public interface IAction
{
    bool CanHandle(PropertyInfo property);
    ActionResult Handle(object value);
}