﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace XOutput.Devices;

/// <summary>
///     Event delegate for DeviceInputChanged event.
/// </summary>
/// <param name="sender">the <see cref="IDevice" /> with changed input</param>
/// <param name="e">event arguments</param>
public delegate void DeviceInputChangedHandler(object sender, DeviceInputChangedEventArgs e);

/// <summary>
///     Event argument class for DeviceInputChanged event.
/// </summary>
public class DeviceInputChangedEventArgs : EventArgs
{
    protected IEnumerable<int> changedDPads;
    protected IEnumerable<InputSource> changedValues;

    protected IDevice device;

    public DeviceInputChangedEventArgs(IDevice device)
    {
        this.device = device;
        changedValues = new InputSource[0];
        changedDPads = new int[0];
    }

    /// <summary>
    ///     Gets the changed device.
    /// </summary>
    public IDevice Device => device;

    /// <summary>
    ///     Gets the changed values.
    /// </summary>
    public IEnumerable<InputSource> ChangedValues => changedValues;

    /// <summary>
    ///     Gets the changed DPad values.
    /// </summary>
    public IEnumerable<int> ChangedDPads => changedDPads;

    public void Refresh(IEnumerable<InputSource> changedValues)
    {
        this.changedValues = changedValues;
    }

    public void Refresh(IEnumerable<int> changedDPads)
    {
        this.changedDPads = changedDPads;
    }

    public void Refresh(IEnumerable<InputSource> changedValues, IEnumerable<int> changedDPads)
    {
        this.changedDPads = changedDPads;
        this.changedValues = changedValues;
    }

    /// <summary>
    ///     Gets if the value of the type has changed.
    /// </summary>
    /// <param name="type">input type</param>
    /// <returns></returns>
    public bool HasValueChanged(InputSource type)
    {
        return changedValues.Contains(type);
    }

    /// <summary>
    ///     Gets if the value of the DPad has changed.
    /// </summary>
    /// <param name="type">input type</param>
    /// <returns></returns>
    public bool HasDPadChanged(int dPadIndex)
    {
        return changedDPads.Contains(dPadIndex);
    }
}