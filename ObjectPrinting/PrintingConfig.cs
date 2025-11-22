using System.Collections.Generic;
using System.Globalization;
using ObjectPrinting.Actions;

namespace ObjectPrinting;

public class PrintingConfig
{
    private readonly List<IAction> actions;

    public PrintingConfig()
    {
        actions = [];
    }

    public void AddAction(IAction action)
    {
        actions.Add(action);
    }

    public IList<IAction> Actions => actions.AsReadOnly();
}