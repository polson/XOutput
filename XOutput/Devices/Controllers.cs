using System.Collections.Generic;
using XOutput.Devices.Input;

namespace XOutput.Devices;

/// <summary>
///     Threadsafe method to get ID of the controllers.
/// </summary>
public sealed class Controllers
{
    private readonly List<GameController> controllers = new();

    private readonly List<int> ids = new();
    private readonly object lockObject = new();

    private Controllers()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of the class.
    /// </summary>
    public static Controllers Instance { get; } = new();

    /// <summary>
    ///     Disposes a used ID.
    /// </summary>
    /// <param name="id">ID to remove</param>
    public void DisposeId(int id)
    {
        lock (lockObject)
        {
            ids.Remove(id);
        }
    }

    public void Add(GameController controller)
    {
        controllers.Add(controller);
        Update(controller, InputDevices.Instance.GetDevices());
    }

    public void Remove(GameController controller)
    {
        controllers.Remove(controller);
        Update(controller, InputDevices.Instance.GetDevices());
    }

    public void Update(GameController controller, IEnumerable<IInputDevice> inputDevices)
    {
        controller.Mapper.Attach(inputDevices);
        controller.XInput.UpdateSources(controller.Mapper.GetInputs());
    }

    public void Update(IEnumerable<IInputDevice> inputDevices)
    {
        foreach (var controller in controllers) Update(controller, inputDevices);
    }

    public IEnumerable<GameController> GetControllers()
    {
        return controllers.ToArray();
    }
}