using Dalamud.Game;
using System;

namespace XIVComboPlugin
{
    class IconReplacerAddressResolver : BaseAddressResolver
    {
        public IntPtr GetIcon { get; private set; }
        public IntPtr IsIconReplaceable { get; private set; }

        protected override void Setup64Bit(SigScanner sig)
        {
            this.GetIcon = sig.ScanText("E8 ?? ?? ?? ?? 8B F8 3B DF");

            this.IsIconReplaceable = sig.ScanText("E8 ?? ?? ?? ?? 84 C0 74 4C 8B D3");
        }
    }
}
