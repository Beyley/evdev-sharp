namespace EvDevSharp;

public class EvDevEventArgs : EventArgs {
	public EvDevEventArgs(int code, int value) {
		(this.Code, this.Value) = (code, value);
	}

	public int Code  { get; set; }
	public int Value { get; set; }
}
