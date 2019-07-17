using HoloToolkit.Sharing.SyncModel;

namespace HoloIslandVis.Sharing
{
    [SyncDataClass]
    public class SyncEnableModel : SyncObject
    {
        public delegate void SyncBoolChangedHandler(SyncBool syncBool);
        public event SyncBoolChangedHandler SyncBoolChanged = delegate { };

        private string _name;

        [SyncData] public SyncBool SyncEnabled;

        public SyncEnableModel(string name) : base(name)
        {

        }

        protected override void NotifyPrimitiveChanged(SyncPrimitive primitive)
        {
            if (primitive is SyncBool)
            {
                SyncBoolChanged((SyncBool)primitive);
            }
        }
    }
}
