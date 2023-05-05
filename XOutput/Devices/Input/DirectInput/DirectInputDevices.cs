﻿using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using SharpDX.DirectInput;


namespace XOutput.Devices.Input.DirectInput;

/// <summary>
///     General DirectInput device manager class.
/// </summary>
public sealed class DirectInputDevices : IDisposable
{
    /// <summary>
    ///     Id of the emulated SCP device
    /// </summary>
    private const string EmulatedSCPID = "028e045e-0000-0000-0000-504944564944";

    private readonly SharpDX.DirectInput.DirectInput directInput = new();

    /// <summary>
    ///     Disposes all resources.
    /// </summary>
    public void Dispose()
    {
        directInput.Dispose();
    }

    ~DirectInputDevices()
    {
        Dispose();
    }

    /// <summary>
    ///     Gets the current available DirectInput devices.
    /// </summary>
    /// <param name="allDevices">No filter</param>
    /// <returns>List of devices</returns>
    public IEnumerable<DeviceInstance> GetInputDevices(bool allDevices)
    {
        if (allDevices)
            return directInput.GetDevices().Where(di => di.Type != DeviceType.Keyboard && di.Type != DeviceType.Mouse);
        return directInput.GetDevices().Where(di =>
            di.Type == DeviceType.Joystick || di.Type == DeviceType.Gamepad || di.Type == DeviceType.FirstPerson);
    }

    /// <summary>
    ///     Creates a wrapper for the native <paramref name="deviceInstance" />.
    /// </summary>
    /// <param name="deviceInstance">native instance</param>
    /// <returns>Wrapped instance</returns>
    public DirectDevice CreateDirectDevice(DeviceInstance deviceInstance, string deviceName)
    {
        try
        {
            var joystick = new Joystick(directInput, deviceInstance.InstanceGuid);
            if (joystick.Information.ProductGuid.ToString() == EmulatedSCPID ||
                (joystick.Capabilities.AxeCount < 1 && joystick.Capabilities.ButtonCount < 1))
            {
                joystick.Dispose();
                return null;
            }

            joystick.Properties.BufferSize = 128;
            var device = new DirectDevice(deviceInstance, joystick, deviceName);
            return device;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to create device " + deviceInstance.InstanceGuid + " " + deviceInstance.InstanceName +
                         ex);
            return null;
        }
    }
}