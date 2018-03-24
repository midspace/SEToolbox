namespace SEToolbox.Interop
{
    using Sandbox.Game.Multiplayer;

    public class MySession
    {
        private MySession(MySyncLayer syncLayer, bool registerComponents = true)
        {
            // Dummy replacement for the Sandbox.Game.World.MySession constructor of the same parameters.
            // So we can create it without getting involed with Havok and other depdancies.
        }
    }
}
