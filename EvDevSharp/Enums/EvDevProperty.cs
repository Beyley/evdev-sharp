namespace EvDevSharp.Enums;

public enum EvDevProperty {
    /// <summary>
    ///     Needs a pointer
    /// </summary>
    InputPropPointer = 0x00,
    /// <summary>
    ///     Direct input device
    /// </summary>
    InputPropDirect = 0x01,
    /// <summary>
    ///     Has button(s) under pad
    /// </summary>
    InputPropButtonpad = 0x02,
    /// <summary>
    ///     Touch rectangle only
    /// </summary>
    InputPropSemiMt = 0x03,
    /// <summary>
    ///     Softbuttons at top of pad
    /// </summary>
    InputPropTopbuttonpad = 0x04,
    /// <summary>
    ///     Is a pointing stick
    /// </summary>
    InputPropPointingStick = 0x05,
    /// <summary>
    ///     Has accelerometer
    /// </summary>
    InputPropAccelerometer = 0x06,
    InputPropMax = 0x1f,
    InputPropCnt = InputPropMax + 1
}
