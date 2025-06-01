using LogExpert.Core.Interface;

namespace LogExpert.UI.Dialogs.LogTabWindow;

public abstract class AbstractLogTabWindow ()
{
    public static StaticLogTabWindowData StaticData { get; set; } = new StaticLogTabWindowData();

    public static ILogTabWindow Create (string[] fileNames, int instanceNumber, bool showInstanceNumbers, IConfigManager configManager)
    {
        return new Controls.LogTabWindow.LogTabWindow(fileNames, instanceNumber, showInstanceNumbers, configManager);
    }
}