namespace EvDevSharp.EventArgs;

public class EvDevEventArgs : System.EventArgs {
    public EvDevEventArgs(int code, int value) => (this.Code, this.Value) = (code, value);

    public int Code { get; set; }
    public int Value { get; set; }
}
