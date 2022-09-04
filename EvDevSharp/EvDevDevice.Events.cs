using EvDevSharp.EventArgs;

namespace EvDevSharp;

public sealed partial class EvDevDevice {
    public delegate void OnAbsoluteEventHandler(object sender, OnAbsoluteEventArgs e);

    public delegate void OnAutoRepeatEventHandler(object sender, EvDevEventArgs e);

    public delegate void OnForceFeedbackEventHandler(object sender, EvDevEventArgs e);

    public delegate void OnForceFeedbackStatusEventHandler(object sender, EvDevEventArgs e);

    public delegate void OnKeyEventHandler(object sender, OnKeyEventArgs e);

    public delegate void OnLedEventHandler(object sender, EvDevEventArgs e);

    public delegate void OnMiscellaneousEventHandler(object sender, EvDevEventArgs e);

    public delegate void OnPowerEventHandler(object sender, EvDevEventArgs e);

    public delegate void OnRelativeEventHandler(object sender, OnRelativeEventArgs e);

    public delegate void OnSoundEventHandler(object sender, EvDevEventArgs e);

    public delegate void OnSwitchEventHandler(object sender, EvDevEventArgs e);

    public delegate void OnSynEventHandler(object sender, OnSynEventArgs e);

    ///<summary>This event corresponds to evdev EV_SYN event type.</summary>
    public event OnSynEventHandler? OnSynEvent;
    ///<summary>This event corresponds to evdev EV_KEY event type.</summary>
    public event OnKeyEventHandler? OnKeyEvent;
    ///<summary>This event corresponds to evdev EV_REL event type.</summary>
    public event OnRelativeEventHandler? OnRelativeEvent;
    ///<summary>This event corresponds to evdev EV_ABS event type.</summary>
    public event OnAbsoluteEventHandler? OnAbsoluteEvent;
    ///<summary>This event corresponds to evdev EV_MSC event type.</summary>
    public event OnMiscellaneousEventHandler? OnMiscellaneousEvent;
    ///<summary>This event corresponds to evdev EV_SW event type.</summary>
    public event OnSwitchEventHandler? OnSwitchEvent;
    ///<summary>This event corresponds to evdev EV_LED event type.</summary>
    public event OnLedEventHandler? OnLedEvent;
    ///<summary>This event corresponds to evdev EV_SND event type.</summary>
    public event OnSoundEventHandler? OnSoundEvent;
    ///<summary>This event corresponds to evdev EV_REP event type.</summary>
    public event OnAutoRepeatEventHandler? OnAutoRepeatEvent;
    ///<summary>This event corresponds to evdev EV_FF event type.</summary>
    public event OnForceFeedbackEventHandler? OnForceFeedbackEvent;
    ///<summary>This event corresponds to evdev EV_PWR event type.</summary>
    public event OnPowerEventHandler? OnPowerEvent;
    ///<summary>This event corresponds to evdev EV_FF_STATUS event type.</summary>
    public event OnForceFeedbackStatusEventHandler? OnForceFeedbackStatusEvent;
}
