using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Extensibility.DesktopAuthBroker;
public record ServerOptions
{
    public string? RootUri { get; init; }
    public string? RelativeCallbackUri { get; init; }
}
