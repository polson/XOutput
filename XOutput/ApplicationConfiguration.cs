using XOutput.Devices.Input.Mouse;
using XOutput.Tools;

namespace XOutput;

public static class ApplicationConfiguration
{
    [ResolverMethod]
    public static ArgumentParser GetArgumentParser()
    {
        return new ArgumentParser();
    }

    [ResolverMethod]
    public static RegistryModifier GetRegistryModifier()
    {
        return new RegistryModifier();
    }

    [ResolverMethod]
    public static MouseHook GetMouseHook()
    {
        return new MouseHook();
    }
}