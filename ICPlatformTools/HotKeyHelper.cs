using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICPlatformTools
{
    public class HotKeyHelper
    {
        private KeyModifiers keyModifiers;
        private Keys registerKey;
        private int hotKeyId;

        private IntPtr handle;

        private static HashSet<int> keyId = new HashSet<int>();

        public int HotKeyId 
        {
            get { return hotKeyId; }
        }

        public HotKeyHelper(Form form, KeyModifiers keyModifiers, Keys registerKey)
        {
            for (int i = 1; i < Int16.MaxValue; i++)
            {
                if (!keyId.Contains(i))
                {
                    hotKeyId = i;
                    keyId.Add(i);
                    break;
                }
            }

            this.handle = form.Handle;
            this.keyModifiers = keyModifiers;
            this.registerKey = registerKey;
        }

        ~HotKeyHelper()
        {
            if (handle != IntPtr.Zero)
            {
                UnRegisterHotKey();
            }
        }

        public void RegisterHotKey()
        {
            NativeMethods.RegisterHotKey(handle, HotKeyId, (uint)keyModifiers, registerKey);
        }

        public void UnRegisterHotKey()
        {
            NativeMethods.UnregisterHotKey(handle, HotKeyId);
        }

        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }
    }
}
