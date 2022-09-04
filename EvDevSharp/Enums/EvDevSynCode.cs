namespace EvDevSharp.Enums;

public enum EvDevSynCode {
    SynReport   = 0,
    SynConfig   = 1,
    SynMtReport = 2,
    SynDropped  = 3,
    SynMax      = 0xf,
    SynCnt      = SynMax + 1
}
