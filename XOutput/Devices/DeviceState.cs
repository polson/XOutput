using System.Collections.Generic;
using System.Linq;

namespace XOutput.Devices;

/// <summary>
///     Holds a current state of the device.
/// </summary>
public class DeviceState
{
    protected readonly List<int> allDpads;
    protected readonly List<int> changedDpad;

    /// <summary>
    ///     Created once not to create memory waste.
    /// </summary>
    protected readonly List<InputSource> changedSources;

    protected DPadDirection[] dpadDirections;
    protected IEnumerable<InputSource> inputSources;

    public DeviceState(IEnumerable<InputSource> inputSources, int dPadCount)
    {
        this.inputSources = inputSources.ToArray();
        changedSources = new List<InputSource>(inputSources.Count());
        dpadDirections = new DPadDirection[dPadCount];
        allDpads = Enumerable.Range(0, dpadDirections.Length).ToList();
        changedDpad = new List<int>();
    }

    /// <summary>
    ///     Gets the current values.
    /// </summary>
    public IEnumerable<InputSource> InputSources => inputSources;

    /// <summary>
    ///     Gets the current DPad values.
    /// </summary>
    public IEnumerable<DPadDirection> DpadDirections => dpadDirections;

    /// <summary>
    ///     Sets new DPad values.
    /// </summary>
    /// <param name="newDPads">new values</param>
    /// <returns>changed DPad indices</returns>
    public bool SetDPad(int i, DPadDirection newValue)
    {
        var oldValue = dpadDirections[i];
        if (newValue != oldValue)
        {
            dpadDirections[i] = newValue;
            changedDpad.Add(i);
            return true;
        }

        return false;
    }

    public void ResetChanges()
    {
        changedSources.Clear();
        changedDpad.Clear();
    }

    public void MarkChanged(InputSource source)
    {
        changedSources.Add(source);
    }

    public IEnumerable<InputSource> GetChanges()
    {
        return changedSources;
    }

    public IEnumerable<int> GetChangedDpads()
    {
        return changedDpad;
    }
}