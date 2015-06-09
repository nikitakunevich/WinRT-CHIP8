using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CHIP8_VM.Utility
{
    public struct OpCode : IFormattable
    {
        public readonly ushort code;
        public byte X
        {
            get
            {
                return (byte)((code & 0x0F00) >> 8);
            }
        }
        public byte Y
        {
            get
            {
                return (byte)((code & 0x00F0) >> 4);
            }
        }
        public byte N
        {
            get
            {
                return (byte)(code & 0x000F);
            }
        }
        public byte NN
        {
            get
            {
                return (byte)(code & 0x00FF);
            }
        }
        public ushort NNN
        {
            get
            {
                return (ushort)(code & 0x0FFF);
            }
        }
        OpCode(ushort val)
        {
            code = val;
        }
        public static implicit operator OpCode(ushort value)
        {
            return new OpCode(value);
        }
        public static implicit operator ushort(OpCode value)
        {
            return value.code;
        }

        public override string ToString()
        {
            return code.ToString();
        }
    
        public string ToString(string format)
        {
            return code.ToString(format);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return code.ToString(format, formatProvider);
        }
}

    public class Disassembly
    {
        public static Stream Disassemble(Stream stream)
        {
            List<byte> binData = new List<byte>();

            int leftToRead = (int)stream.Length;
            var buf = new byte[leftToRead];
            stream.ReadStreamToBuf(buf, 0, 0);
            binData.AddRange(buf);

            List<string> asmCmds = new List<string>();
            for(int i = 0; i < binData.Count; i+=2)
            {
                //TODO: check
                OpCode opcode = Helpers.ToOpCodeBigEndian(binData.ToArray(), i);
                //BitConverter.ToUInt16({ code[1],code[0]}/*code.Reverse().ToArray()*/, 0);
                asmCmds.Add(GetDisasmString(opcode) + "\n");
            }
            MemoryStream ms = new MemoryStream();
            foreach (var cmd in asmCmds)
            {
                byte[] tempBuf = UTF8Encoding.UTF8.GetBytes(cmd);
                ms.Write(tempBuf, 0, tempBuf.Length);
            }
            ms.Position = 0;
            return ms;
        }
                
        public static string DisassembleToText(Stream stream)
        {
            var outStream = Disassemble(stream);

            int leftToRead = (int)outStream.Length;
            byte[] buffer = new byte[leftToRead];
            outStream.ReadStreamToBuf(buffer, 0, leftToRead);
            return UTF8Encoding.UTF8.GetString(buffer, 0, leftToRead);
        }

        static string GetDisasmString(OpCode opcode)
        {
            switch (opcode & 0xf000)
            {
                case 0x0000:
                    switch (opcode)
                    {
                        case 0x00E0:
                            return "CLS";

                        case 0x00EE:
                            return ("RET");
                            
                        default:
                            return ("SYS " + opcode.ToString("X3") + "h");
                    }
                    
                case 0x1000:
                    return ("JP " + opcode.ToString("X3") + "h");
                    
                case 0x2000:
                    return ("CALL " + opcode.ToString("X3") + "h");
                    
                case 0x3000:
                    return (string.Format("SE V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    
                case 0x4000:
                    return (string.Format("SNE V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    
                case 0x5000:
                    return (string.Format("SE V{0:X}, V{1:X}", opcode.X, opcode.Y));
                    
                case 0x6000:
                    return (string.Format("LD V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    
                case 0x7000:
                    return (string.Format("ADD V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    
                case 0x8000:
                    switch (opcode.N)
                    {
                        case 0:
                            return (string.Format("LD V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            
                        case 1:
                            return (string.Format("OR V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            
                        case 2:
                            return (string.Format("AND V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            
                        case 3:
                            return (string.Format("XOR V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            
                        case 4:
                            return (string.Format("ADD V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            
                        case 5:
                            return (string.Format("SUB V{0:X}, V{1:X}", opcode.X, opcode.Y));
                            
                        case 6:
                            return (string.Format("SHR V{0:X}{{, V{1:X}}}", opcode.X, opcode.Y));
                            
                        case 7:
                            return (string.Format("SUBN V{0:X}, V{1:X}", opcode.X, opcode.Y));
                        case 0xE:
                            return (string.Format("SHL V{0:X}{{, V{1:X}}}", opcode.X, opcode.Y));
                        default:
                            return (opcode.ToString());
                    }
                    
                case 0x9000:
                    return (string.Format("SNE V{0:X}, V{1:X}", opcode.X, opcode.Y));
                    
                case 0xA000:
                    return (string.Format("LD I, {0:X3}h", opcode.NNN));
                    
                case 0xB000:
                    return (string.Format("JP V0, {0:X3}h", opcode.NNN));
                    
                case 0xC000:
                    return (string.Format("RND V{0:X}, {1:X2}h", opcode.X, opcode.NN));
                    
                case 0xD000:
                    return string.Format("DRW V{0:X}, V{1:X}, {2:X}", opcode.X, opcode.Y, opcode.N);
                case 0xE000:
                    if (opcode.NN == 0x9E)
                    {
                        return (string.Format("SKP V{0:X}", opcode.X));
                    }
                    else if (opcode.NN == 0xA1)
                    {
                        return (string.Format("SKNP V{0:X}", opcode.X));
                    }
                    return opcode.ToString();
                case 0xF000:
                    switch (opcode.NN)
                    {
                        case 0x07:
                            return (string.Format("LD V{0:X}, DT", opcode.X));
                            
                        case 0x0A:
                            return (string.Format("LD V{0:X},K", opcode.X));
                            
                        case 0x15:
                            return (string.Format("LD DT, V{0:X}", opcode.X));
                            
                        case 0x18:
                            return (string.Format("LD ST, V{0:X}", opcode.X));
                            
                        case 0x1E:
                            return (string.Format("ADD I, V{0:X}", opcode.X));
                            
                        case 0x29:
                            return (string.Format("LD F, V{0:X}", opcode.X));
                            
                        case 0x33:
                            return (string.Format("LD B, V{0:X}", opcode.X));
                            
                        case 0x55:
                            return (string.Format("LD [I], V{0:X}", opcode.X));
                            
                        case 0x65:
                            return (string.Format("LD V{0:X}, I", opcode.X));
                            
                        default:
                            return opcode.ToString();
                    }
                default:
                    return opcode.ToString();
            }
        }
    }
}
