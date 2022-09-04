using EvDevSharp.Enums;

namespace EvDevSharp.EventArgs;

public class OnSynEventArgs : System.EventArgs {
    public OnSynEventArgs(EvDevSynCode code, int value) => (this.Code, this.Value) = (code, value);

    public EvDevSynCode Code { get; set; }
    public int Value { get; set; }
}
