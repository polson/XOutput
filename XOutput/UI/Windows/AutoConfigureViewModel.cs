﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using XOutput.Devices;
using XOutput.Devices.Input;
using XOutput.Devices.Input.Mouse;
using XOutput.Devices.Mapper;
using XOutput.Devices.XInput;
using XOutput.Tools;

namespace XOutput.UI.Windows;

public class AutoConfigureViewModel : ViewModelBase<AutoConfigureModel>
{
    private const int WaitTime = 5000;
    private const int ShortAxisWaitTime = 3000;
    private const int ShortWaitTime = 1000;
    private const int BlinkTime = 500;
    private readonly IEnumerable<IInputDevice> inputDevices;
    private readonly InputSource[] inputTypes;
    private readonly InputMapper mapper;
    private readonly Dictionary<InputSource, double> referenceValues = new();
    private readonly DispatcherTimer timer = new();
    private readonly XInputTypes[] valuesToRead;
    private DateTime lastTime;
    private XInputTypes xInputType;

    public AutoConfigureViewModel(AutoConfigureModel model, IEnumerable<IInputDevice> inputDevices, InputMapper mapper,
        XInputTypes[] valuesToRead) : base(model)
    {
        this.mapper = mapper;
        this.inputDevices = inputDevices;
        this.valuesToRead = valuesToRead;
        xInputType = valuesToRead.First();
        if (valuesToRead.Length > 1)
        {
            Model.ButtonsVisibility = Visibility.Collapsed;
            Model.TimerVisibility = Visibility.Visible;
        }
        else
        {
            Model.ButtonsVisibility = Visibility.Visible;
            Model.TimerVisibility = Visibility.Collapsed;
        }

        inputTypes = inputDevices.SelectMany(i => i.InputSources).ToArray();
        timer.Interval = TimeSpan.FromMilliseconds(BlinkTime);
        timer.Tick += TimerTick;
        timer.Start();
    }

    public Func<bool> IsMouseOverButtons { get; set; }

    private void TimerTick(object sender, EventArgs e)
    {
        Model.Highlight = !Model.Highlight;
    }

    public void Initialize()
    {
        ReadReferenceValues();
        foreach (var inputDevice in inputDevices) inputDevice.InputChanged += ReadValues;
        Model.XInput = xInputType;
        SetTime(false);
    }

    protected void ReadReferenceValues()
    {
        foreach (var type in inputTypes)
        foreach (var inputDevice in inputDevices)
            referenceValues[type] = inputDevice.Get(type);
    }

    /// <summary>
    ///     Reads the current values, and if the values have changed enough saves them.
    /// </summary>
    private void ReadValues(object sender, DeviceInputChangedEventArgs e)
    {
        if (e.Device is Mouse && (IsMouseOverButtons?.Invoke() ?? false)) return;
        var inputDevice = e.Device;
        InputSource maxType = null;
        double maxDiff = 0;
        foreach (var type in e.ChangedValues)
        {
            var oldValue = referenceValues[type];
            var newValue = inputDevice.Get(type);
            var diff = Math.Abs(newValue - oldValue);
            if (diff > maxDiff)
            {
                maxType = type;
                maxDiff = diff;
            }
        }

        if (maxDiff > 0.3 && maxType != Model.MaxType)
        {
            Model.MaxType = maxType;
            CalculateStartValues();
        }

        if (Model.MaxType != null) CalculateValues();
    }

    public bool SaveDisableValues()
    {
        var mapperCollection = mapper.GetMapping(xInputType);
        var md = mapperCollection.Mappers[0]; // TODO
        if (md.InputType == null) md.Source = inputTypes.First();
        md.MinValue = Model.XInput.GetDisableValue();
        md.MaxValue = Model.XInput.GetDisableValue();
        return Next();
    }

    public bool SaveValues()
    {
        if (Model.MaxType != null)
        {
            var mapperCollection = mapper.GetMapping(xInputType);
            var md = mapperCollection.Mappers[0]; // TODO
            md.Source = Model.MaxType;
            md.MinValue = Model.MinValue / 100;
            md.MaxValue = Model.MaxValue / 100;
            return Next();
        }

        return SaveDisableValues();
    }

    public bool IncreaseTime()
    {
        Model.TimerValue += DateTime.Now.Subtract(lastTime).TotalMilliseconds;
        lastTime = DateTime.Now;
        return Model.TimerValue > Model.TimerMaxValue;
    }

    public void Close()
    {
        foreach (var inputDevice in inputDevices) inputDevice.InputChanged -= ReadValues;
        timer.Stop();
    }

    protected void SetTime(bool shortTime)
    {
        Model.TimerValue = 0;
        if (shortTime)
            Model.TimerMaxValue = xInputType.IsAxis() ? ShortAxisWaitTime : ShortWaitTime;
        else
            Model.TimerMaxValue = WaitTime;
        lastTime = DateTime.Now;
    }

    protected bool Next()
    {
        Model.MaxType = null;
        var index = Array.IndexOf(valuesToRead, xInputType);
        SetTime(false);
        if (index + 1 < valuesToRead.Length)
        {
            xInputType = valuesToRead[index + 1];
            Model.XInput = xInputType;
            return true;
        }

        return false;
    }

    private void CalculateValues()
    {
        var current = Model.MaxType.InputDevice.Get(Model.MaxType);

        var min = Math.Min(current, Model.MinValue / 100);
        var minValue = Math.Round(min * 100);

        var max = Math.Max(current, Model.MaxValue / 100);
        var maxValue = Math.Round(max * 100);

        if (!Helper.DoubleEquals(minValue, Model.MinValue) || !Helper.DoubleEquals(maxValue, Model.MaxValue))
        {
            Model.MinValue = minValue;
            Model.MaxValue = maxValue;
            SetTime(true);
        }
    }

    private void CalculateStartValues()
    {
        var current = Model.MaxType.InputDevice.Get(Model.MaxType);
        var reference = referenceValues[Model.MaxType];

        var min = Math.Min(current, reference);
        Model.MinValue = Math.Round(min * 100);

        var max = Math.Max(current, reference);
        Model.MaxValue = Math.Round(max * 100);

        SetTime(true);
    }
}