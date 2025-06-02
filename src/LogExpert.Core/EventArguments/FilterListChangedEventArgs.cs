
using LogExpert.Core.Interface;

namespace LogExpert.Core.EventArguments;

public class FilterListChangedEventArgs(ILogWindow logWindow)
{
    #region Properties

    public ILogWindow LogWindow { get; } = logWindow;

    #endregion
}