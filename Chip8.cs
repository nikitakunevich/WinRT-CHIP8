using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CHIP8_VM.Utility;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Microsoft.Graphics.Canvas;

namespace CHIP8_VM
{
    public enum VMState
    {
        Offline,
        Running,
        WaitForKey,
        Paused
    }

    /// <summary>
    /// Main VM module
    /// </summary>
    public class Chip8
    {
        public int dotSize;

        public Windows.UI.Xaml.Controls.MediaElement mediaElement;
        public ICanvasAnimatedControl displayCanvas;
        public CanvasAnimatedDrawEventArgs displayArgs;

        private const int CPU_FREQ = 60;

        public readonly int BitsInByte = 8; 
        public static readonly int screenW = 64;
        public static readonly int screenH = 32;

        private TimeSpan cycleInterval = TimeSpan.FromMilliseconds((1.0d / CPU_FREQ) * 1000);
        private DateTime lastTime = DateTime.Now;
        public VMState State;

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
        public byte DelayTimer;
        public byte SoundTimer;

        public ushort[] stack = new ushort[0x16];
        public ushort SP;
        public ushort PC;

        public bool[] key = new bool[16];

        private Stream _gameStream;

        private Stopwatch s_watch;

        public Chip8()
        {
            s_watch = new Stopwatch();
        }


        public void InitVM(Stream gameStream)
        {
            State = VMState.Offline;
            _gameStream = gameStream;

            //Loading fonts to the beginning of VM memory
            Array.Copy(Constants.chip8_fontset, Memory, Constants.chip8_fontset.Count());
            PC = 0x200;
            LoadProgramToMemory();
        }

        public void Pause()
        {

        }

        public void Stop()
        {

        }

        public void Update()
        {
            if (State == VMState.Running)
            {
                OpCode cmd = Helpers.ToOpCodeBigEndian(Memory, PC);
                ExecCommand(cmd);


            }
            /*else
                displayCanvas.Invalidate();*/
        }

        public Action<int> KeyPressed = new Action<int>((key) => { });

        void LoadProgramToMemory()
        {
            int streamLen = (int)_gameStream.Length;
            var buffer = new byte[streamLen];
            _gameStream.ReadStreamToBuf(buffer, 0, streamLen);
            Array.Copy(buffer, 0, Memory, 0x200, streamLen);
        }

        public void ExecCommand(OpCode opcode)
        {
            s_watch.Start();
            switch (opcode & 0xf000)
            {
                case 0x0000:
                    switch (opcode)
                    {
                        case 0x00E0: //CLR
                            Array.Clear(Screen, 0, Screen.Count());
                            //Debug.WriteLine("CLR");
                            break;
                        case 0x00EE: //RET
                            SP--;
                            PC = stack[SP];
                            //Debug.WriteLine("RET");
                            break;

                        default:
                            break;
                    }
                    PC += 2;
                    break;
                case 0x1000: //0x1NNN: JMP NNN
                    PC = opcode.NNN;
                    //Debug.WriteLine("JP " + opcode.ToString("X3") + "h");
                    break;

                case 0x2000: //0x2NNN: CALL NNN
                    stack[SP] = PC;
                    PC = opcode.NNN;
                    SP++;
                    //Debug.WriteLine("CALL " + opcode.ToString("X3") + "h");
                    break;

                case 0x3000: //0x3VNN: SKIP V, NN
                    if (V[opcode.X] == opcode.NN)
                        PC += 2;
                    PC += 2;
                    //Debug.WriteLine(string.Format("SE V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    break;

                case 0x4000: //0x4VNN SKIPN V, NN
                    if (V[opcode.X] != opcode.NN)
                        PC+=2;
                    PC += 2;
                    //Debug.WriteLine(string.Format("SNE V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    break;

                case 0x5000: //
                    if (V[opcode.X] == V[opcode.Y])
                        PC+=2;
                    PC += 2;
                    //Debug.WriteLine(string.Format("SE V{0:X}, V{1:X}", opcode.X, opcode.Y));
                    break;

                case 0x6000:
                    V[opcode.X] = opcode.NN;
                    PC += 2;
                    //Debug.WriteLine(string.Format("LD V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    break;

                case 0x7000:
                    V[opcode.X] += opcode.NN;
                    PC += 2;
                    //Debug.WriteLine(string.Format("ADD V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    break;

                case 0x8000:
                    switch (opcode.N)
                    {
                        case 0:
                            V[opcode.X] = V[opcode.Y];
                            //Debug.WriteLine(string.Format("LD V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            break;

                        case 1:
                            V[opcode.X] |= V[opcode.Y];
                            //Debug.WriteLine(string.Format("OR V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            break;

                        case 2:
                            V[opcode.X] &= V[opcode.Y];
                            //Debug.WriteLine(string.Format("AND V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            break;

                        case 3:
                            V[opcode.X] ^= V[opcode.Y];
                            //Debug.WriteLine(string.Format("XOR V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            break;

                        case 4:
                            V[0xF] = (V[opcode.X] + V[opcode.Y] > 255) ? (byte)1 : (byte)0;
                            V[opcode.X] += V[opcode.Y];
                            //Debug.WriteLine(string.Format("ADD V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            break;

                        case 5:
                            V[0xF] = V[opcode.X] > V[opcode.Y] ? (byte)1 : (byte)0;
                            V[opcode.X] -= V[opcode.Y];
                            //Debug.WriteLine(string.Format("SUB V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            break;

                        case 6:
                            V[0xF] = (byte)(V[opcode.X] & 1);
                            V[opcode.X] >>= 1;
                            //Debug.WriteLine(string.Format("SHR V{0:X}{{, V{1:X}}}", opcode.X, opcode.Y));
                            break;
                        case 7:
                            V[0xF] = V[opcode.Y] > V[opcode.X] ? (byte)1 : (byte)0;
                            V[opcode.X] = (byte)(V[opcode.Y] - V[opcode.X]);
                            //Debug.WriteLine(string.Format("SUBN V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            break;
                        case 0xE:
                            V[0xF] = (byte)(V[opcode.X] >> 7);
                            V[opcode.X] <<= 1;
                            //Debug.WriteLine(string.Format("SHL V{0:X}{{, V{1:X}}}", opcode.X, opcode.Y));
                            break;
                        default:
                            break;
                    }
                    PC += 2;
                    break;

                case 0x9000:
                    if (V[opcode.X] != V[opcode.Y])
                        PC+=2;
                    PC += 2;
                    //Debug.WriteLine(string.Format("SNE V{0:X}, V{1:X}", opcode.X, opcode.Y));
                    break; 

                case 0xA000:
                    I = opcode.NNN;
                    PC += 2;
                    //Debug.WriteLine(string.Format("LD I, {0:X3}h", opcode.NNN));
                    break;

                case 0xB000:
                    PC = (ushort)(opcode.NNN + V[0]);
                    //Debug.WriteLine(string.Format("JP V0, {0:X3}h", opcode.NNN));
                    break;

                case 0xC000:
                    var rnd = new Random();
                    V[opcode.X] = (byte)(rnd.Next(256) & opcode.NN);
                    PC += 2;
                    //Debug.WriteLine(string.Format("RND V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    break;
                case 0xD000:
                    int x = V[opcode.X];
                    int y = V[opcode.Y];
                    byte[] buffer = new byte[opcode.N];
                    Array.Copy(Memory, I, buffer, 0, opcode.N);
                    for (var lineY = 0; lineY < opcode.N; lineY++)
                    {
                        for (var lineX = 0; lineX < BitsInByte; lineX++)
                        {
                            if ((buffer[lineY] & (0x80 >> lineX)) != 0)
                            {

                                int realX = lineX + x;
                                int realY = lineY + y;
                                //wrapping
                                if (realY >= screenH)
                                {
                                    realY = realY - screenH;
                                }
                                if (realY < 0)
                                {
                                    realY = realY + screenH;
                                }
                                if (realX >= screenW)
                                {
                                    realX = realX - screenW;
                                }
                                if (realX < 0)
                                {
                                    realY = realX + screenW;
                                }

                                if (Screen[realX + ((realY) * screenW)] == 1)
                                {
                                    //collision
                                    V[0xF] = 1;
                                }

                                Screen[realY * screenW + realX] ^= 1;
                            }
                        }
                    }
                    PC += 2;
                    //Debug.WriteLine(string.Format("DRW V{0:X}, V{1:X}, {2:X}", opcode.X, opcode.Y, opcode.N));
                    break;


                case 0xE000:
                    if (opcode.NN == 0x9E)
                    {
                        if (key[V[opcode.X]] == true)
                            PC+=2;
                        PC += 2;
                        //Debug.WriteLine(string.Format("SKP V{0:X}", opcode.X));
                        break;
                    }
                    else if (opcode.NN == 0xA1)
                    {
                        if (!key[V[opcode.X]])
                            PC+=2;
                        PC += 2;
                        //Debug.WriteLine(string.Format("SKNP V{0:X}", opcode.X));
                        break;
                    }
                    break;
                case 0xF000:
                    switch (opcode.NN)
                    {
                        case 0x07:
                            V[opcode.X] = DelayTimer;
                            PC += 2;
                            //Debug.WriteLine(string.Format("LD V{0:X}, DT", opcode.X));
                            break;
                        case 0x0A:
                            State = VMState.WaitForKey;
                            KeyPressed = new Action<int>((key) => { V[opcode.X] = (byte)key; State = VMState.Running; });
                            PC += 2;
                            //Debug.WriteLine(string.Format("LD V{0:X},K", opcode.X));
                            break;
                        case 0x15:
                            DelayTimer = V[opcode.X];
                            PC += 2;
                            //Debug.WriteLine(string.Format("LD DT, V{0:X}", opcode.X));
                            break;
                        case 0x18:
                            SoundTimer = V[opcode.X];
                            PC += 2;
                            //Debug.WriteLine(string.Format("LD ST, V{0:X}", opcode.X));
                            break;

                        case 0x1E:
                            //TODO: should i do this?
                            V[0xF] = I + V[opcode.X] > 0xFFF ? (byte)1 : (byte)0; 
                            I += V[opcode.X];
                            PC += 2;
                            //Debug.WriteLine(string.Format("ADD I, V{0:X}", opcode.X));
                            break;
                        case 0x29:
                            I = (ushort)(V[opcode.X] * 5);
                            PC += 2;
                            //Debug.WriteLine(string.Format("LD F, V{0:X}", opcode.X));
                            break;
                        case 0x33:
                            var number = V[opcode.X];

                            for (var i = 3; i > 0; i--)
                            {
                                Memory[I + i - 1] = (byte)(number % 10);
                                number /= 10;
                            }
                            PC += 2;
                            //Debug.WriteLine(string.Format("LD B, V{0:X}", opcode.X));
                            break;
                        case 0x55:
                            Array.Copy(V, 0, Memory, I, opcode.X);
                            //I += (ushort)(opcode.X + 1);
                            PC += 2;
                            //Debug.WriteLine(string.Format("LD [I], V{0:X}", opcode.X));
                            break;
                        case 0x65:
                            Array.Copy(Memory, I, V, 0, opcode.X);
                            //I += (ushort)(opcode.X + 1);
                            PC += 2;
                            //Debug.WriteLine(string.Format("LD V{0:X}, I", opcode.X));
                            break;
                    }
                    break;
            }
            s_watch.Stop();
            s_watch.Restart();
            Debug.WriteLine("Process tck = {0}, mS = {1}", s_watch.ElapsedTicks, s_watch.ElapsedMilliseconds);
            s_watch.Stop();
            Debug.WriteLine("dbg write tck = {0}, mS = {1}", s_watch.ElapsedTicks, s_watch.ElapsedMilliseconds);
            s_watch.Restart();

            s_watch.Restart();
            if (DelayTimer > 0)
                --DelayTimer;

            if (SoundTimer > 0)
            {
                --SoundTimer;
                if (SoundTimer == 1)
                {
                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    s_watch.Restart();
                    dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        mediaElement.Play();
                    });
                    s_watch.Stop();
                    int a = 5;
                }
                    //Task.Run(() => mediaElement.Play());
            }

            Debug.WriteLine("timer tck = {0}, mS = {1}", s_watch.ElapsedTicks, s_watch.ElapsedMilliseconds);
            s_watch.Restart();

            //CanvasBitmap.CreateFromBytes()
            var session = displayArgs.DrawingSession;
            for (var i = 0; i < Chip8.screenH; i++)
            {
                for (var j = 0; j < Chip8.screenW; j++)
                {
                    if (Screen[i * Chip8.screenW + j] == 1)
                        //session.DrawImage()
                        session.FillRectangle(j * dotSize, i * dotSize, dotSize, dotSize, Colors.Green);
                }
            }
            //displayCanvas.Invalidate();
            s_watch.Stop();
            Debug.WriteLine("Draw tck = {0}, mS = {1}", s_watch.ElapsedTicks, s_watch.ElapsedMilliseconds);
            /*var cur_time = DateTime.Now;
            var fill_time = cycleInterval - (cur_time - lastTime);
            if (fill_time.Ticks > 0)
                Task.Delay(fill_time);
            lastTime = DateTime.Now;*/
        }
    }
}
