namespace EvDevSharp;

public class OnAbsoluteEventArgs : EventArgs {
	public OnAbsoluteEventArgs(EvDevAbsoluteAxisCode axis, int value) {
		(this.Axis, this.Value) = (axis, value);
	}

	public EvDevAbsoluteAxisCode Axis  { get; set; }
	public int                   Value { get; set; }
}
