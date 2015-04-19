using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIP8_VM
{
    /// <summary>
    /// Main VM module
    /// </summary>
    public class Chip8
    {
        private byte[] _memory = new byte[4096];
        public byte[] Memory
        {
            get { return _memory; }
            set { _memory = value; }
        }
        //screen memory
        public byte[] Screen = new byte[64 * 32];

        //VF is used as flag
        public byte[] V = new byte[16];

        //used for address storing
        public ushort I;

        //Timers
        public byte delay_timer;
        public byte sound_timer;

        public ushort[] stack = new ushort[16];
        public ushort sp;
        public ushort pc;

        public ushort[] key = new ushort[16];

        public Chip8()
        {
            
        }

        public LoadGame(Stream gameStream)
        {

        }
    }




}
