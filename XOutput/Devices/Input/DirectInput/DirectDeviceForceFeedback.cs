using System;
using SharpDX;
using SharpDX.DirectInput;
using XOutput.Logging;

namespace XOutput.Devices.Input.DirectInput;

/// <summary>
///     Device that controls force feedback for a DirectInput device
/// </summary>
public class DirectDeviceForceFeedback : IDisposable
{
    private static readonly ILogger logger = LoggerFactory.GetLogger(typeof(DirectDeviceForceFeedback));
    private readonly EffectInfo force;
    private readonly int gain;
    private readonly Joystick joystick;
    private readonly int samplePeriod;
    private int axisCount;
    private DeviceObjectInstance bigActuator;
    private int[] bigAxes;
    private int[] bigDirections;
    private Effect bigEffect;
    private DeviceObjectInstance smallActuator;
    private int[] smallAxes;
    private int[] smallDirections;
    private Effect smallEffect;

    public DirectDeviceForceFeedback(Joystick joystick, EffectInfo force, DeviceObjectInstance actuator) : this(
        joystick, force, actuator, null)
    {
    }

    public DirectDeviceForceFeedback(Joystick joystick, EffectInfo force, DeviceObjectInstance bigActuator,
        DeviceObjectInstance smallActuator)
    {
        this.bigActuator = bigActuator;
        this.smallActuator = smallActuator;
        this.force = force;
        this.joystick = joystick;
        axisCount = 0;
        gain = joystick.Properties.ForceFeedbackGain;
        samplePeriod = joystick.Capabilities.ForceFeedbackSamplePeriod;
        RefreshAxes();
    }

    public DeviceObjectInstance BigActuator
    {
        get => bigActuator;
        set
        {
            if (bigActuator != value)
            {
                bigActuator = value;
                RefreshAxes();
            }
        }
    }

    public DeviceObjectInstance SmallActuator
    {
        get => smallActuator;
        set
        {
            if (smallActuator != value)
            {
                smallActuator = value;
                RefreshAxes();
            }
        }
    }

    /// <summary>
    ///     Disposes all resources.
    /// </summary>
    public void Dispose()
    {
        bigEffect?.Dispose();
        smallEffect?.Dispose();
    }

    /// <summary>
    ///     Sets the force feedback motor values.
    /// </summary>
    /// <param name="big">Big motor value</param>
    /// <param name="small">Small motor value</param>
    public void SetForceFeedback(double big, double small)
    {
        if (smallActuator != null) smallEffect = DoForceFeedback(smallEffect, smallAxes, smallDirections, small);
        if (bigActuator != null) bigEffect = DoForceFeedback(bigEffect, bigAxes, bigDirections, big);
    }

    private Effect DoForceFeedback(Effect oldEffect, int[] axes, int[] directions, double value)
    {
        var effectParams = new EffectParameters
        {
            Flags = EffectFlags.Cartesian | EffectFlags.ObjectIds,
            StartDelay = 0,
            SamplePeriod = samplePeriod,
            Duration = int.MaxValue,
            TriggerButton = -1,
            TriggerRepeatInterval = int.MaxValue,
            Gain = gain
        };
        effectParams.SetAxes(axes, directions);
        var cf = new ConstantForce
        {
            Magnitude = CalculateMagnitude(value)
        };
        effectParams.Parameters = cf;
        try
        {
            var newEffect = new Effect(joystick, force.Guid, effectParams);
            oldEffect?.Dispose();
            newEffect.Start();
            return newEffect;
        }
        catch (SharpDXException)
        {
            logger.Warning($"Failed to create and start effect for {ToString()}");
            return null;
        }
    }

    private void RefreshAxes()
    {
        bigEffect?.Dispose();
        smallEffect?.Dispose();
        bigEffect = null;
        smallEffect = null;
        axisCount = 0;
        if (smallActuator != null) axisCount++;
        if (bigActuator != null) axisCount++;
        if (axisCount == 0)
        {
            bigAxes = null;
            bigDirections = null;
            smallAxes = null;
            smallDirections = null;
        }
        else if (axisCount == 1)
        {
            if (smallActuator == null)
            {
                bigAxes = new[] { (int)bigActuator.ObjectId };
                bigDirections = new[] { 0 };
                smallAxes = null;
                smallDirections = null;
            }
            else
            {
                bigAxes = null;
                bigDirections = null;
                smallAxes = new[] { (int)smallActuator.ObjectId };
                smallDirections = new[] { 0 };
            }
        }
        else
        {
            bigAxes = new[] { (int)bigActuator.ObjectId, (int)smallActuator.ObjectId };
            bigDirections = new[] { 1, 0 };
            smallAxes = new[] { (int)smallActuator.ObjectId, (int)bigActuator.ObjectId };
            smallDirections = new[] { 1, 0 };
        }
    }

    /// <summary>
    ///     Calculates the magnitude value from 0-1 values.
    /// </summary>
    /// <param name="value">ratio</param>
    /// <returns>magnitude value</returns>
    private int CalculateMagnitude(double value)
    {
        return (int)(gain * value);
    }
}