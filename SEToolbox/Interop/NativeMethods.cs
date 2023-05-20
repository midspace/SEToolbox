namespace SEToolbox.Interop
{
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Input;

    internal static class NativeMethods
    {
        private const string User32Library = "user32.dll";

        [DllImport(User32Library, EntryPoint = "GetKeyState", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short _GetKeyState(int keyCode);

        internal static KeyStates GetKeyState(Keys key)
        {
            var state = KeyStates.None;

            var retVal = _GetKeyState((int)key);

            // If the high-order bit is 1, the key is down
            // otherwise, it is up.
            if ((retVal & 0x8000) == 0x8000)
                state |= KeyStates.Down;

            // If the low-order bit is 1, the key is toggled.
            if ((retVal & 1) == 1)
                state |= KeyStates.Toggled;

            return state;
        }
    }
}
