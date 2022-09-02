namespace EvDevSharp;

public class OnKeyEventArgs : EventArgs {
	public OnKeyEventArgs(EvDevKeyCode key, EvDevKeyValue value) {
		(this.Key, this.Value) = (key, value);
	}

	public EvDevKeyCode  Key   { get; set; }
	public EvDevKeyValue Value { get; set; }
}
