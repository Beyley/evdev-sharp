using EvDevSharp.Enums;

namespace EvDevSharp.EventArgs;

public class OnKeyEventArgs : System.EventArgs {
    public OnKeyEventArgs(EvDevKeyCode key, EvDevKeyValue value) => (this.Key, this.Type) = (key, value);

    public EvDevKeyCode Key { get; set; }
    public EvDevKeyValue Type { get; set; }
}
