using EvDevSharp.Enums;

namespace EvDevSharp.EventArgs;

public class OnAbsoluteEventArgs : System.EventArgs {
    public OnAbsoluteEventArgs(EvDevAbsoluteAxisCode axis, int value) => (this.Axis, this.Value) = (axis, value);

    public EvDevAbsoluteAxisCode Axis { get; set; }
    public int Value { get; set; }
}
