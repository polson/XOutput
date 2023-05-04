using System.Collections.Generic;
using XOutput.Devices.XInput.Vigem;
using XOutput.Diagnostics;

namespace XOutput.Devices.XInput
{
    /// <summary>
    /// Tests the XInput emulation devices.
    /// </summary>
    public class XInputDiagnostics : IDiagnostics
    {
        /// <summary>
        /// Returns null as no object can be associated with this test.
        /// <para>Implements <see cref="IDiagnostics.Source"/></para>
        /// </summary>
        public object Source => null;

        /// <summary>
        /// <para>Implements <see cref="IDiagnostics.GetResults()"/></para>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DiagnosticsResult> GetResults()
        {
            return new DiagnosticsResult[]
            {
                GetVigemDeviceResult(),
                GetXDeviceResult(),
            };
        }

        public DiagnosticsResult GetXDeviceResult()
        {
            DiagnosticsResult result = new DiagnosticsResult
            {
                Type = XInputDiagnosticsTypes.XDevice,
            };
            if (GetVigemDeviceResult().State != DiagnosticsResultState.Passed)
            {
                result.Value = false;
                result.State = DiagnosticsResultState.Failed;
            }
            else
            {
                result.Value = true;
                result.State = DiagnosticsResultState.Passed;
            }
            return result;
        }

        public DiagnosticsResult GetVigemDeviceResult()
        {
            DiagnosticsResult result = new DiagnosticsResult
            {
                Type = XInputDiagnosticsTypes.VigemDevice,
            };
            if (VigemDevice.IsAvailable())
            {
                result.Value = true;
                result.State = DiagnosticsResultState.Passed;
            }
            else
            {
                result.Value = false;
                result.State = DiagnosticsResultState.Warning;
            }
            return result;
        }
    }
}
