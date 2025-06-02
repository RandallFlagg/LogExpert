using LogExpert.Core.Entities;
using LogExpert.Core.EventArguments;

namespace LogExpert.Core.EventHandlers;

public delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArgs e);
public delegate void FileSizeChangedEventHandler(object sender, LogEventArgs e);
