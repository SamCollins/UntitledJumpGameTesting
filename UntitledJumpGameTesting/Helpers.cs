using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UntitledJumpGameTesting
{
    public class Helpers
    {
        private bool DebugModeOn;

        public Helpers()
        {
            DebugModeOn = false;
        }

        public void LogDebug(string logMsg)
        {
            if (DebugModeOn)
                Debug.WriteLine(logMsg);
        }

        public void ToggleDebugLogging()
        {
            DebugModeOn = !DebugModeOn;
        }
    }
}
