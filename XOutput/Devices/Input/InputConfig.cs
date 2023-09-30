namespace XOutput.Devices.Input;

public class InputConfig
{
    public InputConfig()
    {
        ForceFeedback = false;
    }

    public InputConfig(int forceFeedbackCount)
    {
        ForceFeedback = forceFeedbackCount > 0;
    }

    /// <summary>
    ///     Enables the force feedback for the controller.
    /// </summary>
    public bool ForceFeedback { get; set; }
}