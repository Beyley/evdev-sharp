using EvDevSharp.Enums;

namespace EvDevSharp.EventArgs;

public class OnRelativeEventArgs : System.EventArgs {
	public OnRelativeEventArgs(EvDevRelativeAxisCode axis, int value) {
		(this.Axis, this.Value) = (axis, value);
	}

	public EvDevRelativeAxisCode Axis  { get; set; }
	public int                   Value { get; set; }
}
