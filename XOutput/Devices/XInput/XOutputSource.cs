using XOutput.Devices.Mapper;

namespace XOutput.Devices.XInput;

/// <summary>
///     Direct input source.
/// </summary>
public class XOutputSource : InputSource
{
    public XOutputSource(string name, XInputTypes type) : base(null, name, type.GetInputSourceType(), 0)
    {
        XInputType = type;
    }

    public XInputTypes XInputType { get; }

    internal bool Refresh(InputMapper mapper)
    {
        var mappingCollection = mapper.GetMapping(XInputType);
        if (mappingCollection != null)
        {
            var newValue = mappingCollection.GetValue(XInputType);
            return RefreshValue(newValue);
        }

        return false;
    }
}