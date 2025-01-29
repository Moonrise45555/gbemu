using Rendering;

namespace EmuMemory
{
    public static class Registers
    {
        public static byte IME = 0;
        public static byte IMEpendingstate = 0;
        public static ushort PC;
        public static byte Flags = 0;


        public static void IMECheck()
        {
            if (Registers.IMEpendingstate != 0)
            {
                Registers.IMEpendingstate = 0;
                Registers.IME = 1;
            }
        }


        public static bool IfCondition(int ConditionIndex)
        {
            //checks the condition indicated by the argument given, and returns true or false based on it

            switch (ConditionIndex)
            {
                case 3:
                    if (ReadFlag("C") == 1)
                    {
                        return true;
                    }
                    else return false;

                case 2:
                    return ReadFlag("C") != 1;
                case 1:
                    return ReadFlag("Z") == 1;
                case 0:
                    return ReadFlag("Z") != 1;
            }
            throw new Exception();
        }
        public static void SetFlags8bAdd(byte operand1, byte operand2, byte operand3 = 0, string SubMode = "+")
        {
            //performs an 8 bit addition between the operands and sets flags based on it


            int a = (operand1 + operand2 + operand3);

            int abyte = 1;
            int ahalfadd = 1;
            int halfadd = (operand1 & 0x0F) + (operand2 & 0x0F) + (operand3 & 0x0F);
            if (SubMode == "-")
            {
                abyte = (operand1 - operand2 - operand3);
                ahalfadd = ((operand1 & 0x0F) - (operand2 & 0x0F) - (operand3 & 0x0F));
                a = 1;
                halfadd = 1;

            }
            if ((byte)a == 0 || (byte)abyte == 0)
                Registers.SetFlag("Z", 1);
            else Registers.SetFlag("Z", 0);

            Registers.SetFlag("N", 0);

            if (a > byte.MaxValue || abyte < 0 || abyte > byte.MaxValue)
                Registers.SetFlag("C", 1);
            else Registers.SetFlag("C", 0);

            if (halfadd > 0x0F || ahalfadd < 0)
            {
                Registers.SetFlag("H", 1);
            }
            else Registers.SetFlag("H", 0);
        }
        public static void SetFlag(string flag, int value)
        {
            //sets the flag indicated by the string to the value
            if (value != 0 && value != 1)
            {

                throw new Exception("cant set flag to a non boolean!");
            }
            switch (flag)
            {
                case "Z":
                    Flags = (byte)((Flags & 0b01111111) | (value << 7));
                    break;

                case "N":
                    Flags = (byte)((Flags & 0b10111111) | ((value << 6)));
                    break;

                case "H":
                    Flags = (byte)((Flags & 0b11011111) | ((value << 5)));
                    break;

                case "C":
                    Flags = (byte)((Flags & 0b11101111) | (value << 4));
                    break;

                default:
                    throw new Exception("invalid flag requested!");

            }
        }

        public static int ReadFlag(string flag)
        {
            //returns the value of the flag
            switch (flag)
            {
                case "Z":
                    return (Flags & 0b10000000) >> 7;


                case "N":
                    return (Flags & 0b01000000) >> 6;

                case "H":
                    return (Flags & 0b00100000) >> 5;

                case "C":
                    return (Flags & 0b00010000) >> 4;

                default:
                    throw new Exception("invalid flag requested!");


            }
        }
        static ushort SP = 0xFF00;
        /* array of 8-bit registers*/
        public static byte[] r8 = new byte[8];
        public static ushort getr16(int index)
        {
            //returns the 16b register indicated by the argument

            ushort[] _r16 = new ushort[4];
            for (int i = 0; i < 3; i++)
            {
                _r16[i] = (ushort)((r8[i * 2] << 8) + r8[i * 2 + 1]);
            }
            _r16[3] = SP;
            return _r16[index];
        }



        public static void setr16(ushort input, int index)
        {
            //sets the 16b register indicated by the index to the 16 bit value argument given


            if (index > 3 || index < 0)
            {
                throw new IndexOutOfRangeException("cant be over 3 or under zero!");
            }
            if (index == 3)
            {
                SP = input;
            }
            else
            {
                r8[index * 2 + 1] = (byte)(input & 0x00FF);
                r8[index * 2] = (byte)(input >> 8);
            }
        }

    }
    public static class Memory
    {
        public static byte[] ROM = new byte[1];
        public static byte[] BootROM = new byte[1];
        public static int MBCType;
        public static Byte[] RAM = new byte[65536];
        public static void MemWrite16b(ushort address, ushort data)
        {
            //writes a 16b number to the address in memory
            Memory.MemWrite(address, (byte)(data & 0x00FF));
            Memory.MemWrite((ushort)(address + 1), (byte)(data >> 8));
        }




        public static byte MemRead(ushort index)
        {
            if (index == 0xFF00)
            {
                Input.GetInputs();
            }




            if (index >= 0xE000 && index <= 0xFDFF)
            {
                //redirect reads to echo ram
                return RAM[index - 0xE000 + 0xC000];
            }
            //returns the value at a given address in memory
            return RAM[index];
        }
        public static void SwitchMB(int index)
        {
            if (index == 0)
            {
                index = 1;
            }
            for (var i = 0; i < 0x4000; i++)
            {
                RAM[0x4000 + i] = ROM[i + 0x4000 * index];
            }
        }

        public static void MemWrite(ushort index, byte Data)
        {
            if (index == 0xFF00)
            {
                byte val = (byte)(Data | 0b001111);

                RAM[0xFF00] = val;
                Input.GetInputs();
                return;
            }
            //MBC stuff
            if (index >= 0x0000 && index <= 0x7FFF)
            {
                //wrote into "ROM", handle based on MBCtype
                switch (MBCType)
                {
                    case 0:
                        //MBCtype 0 means no mbc, so we dont care about writes, nothing to do
                        return;
                    case 1:
                        //MBCType 1 measn we actually have to care about stuff happening
                        if (index >= 0x2000 && index <= 0x3FFF)
                        {
                            SwitchMB(Data & 0b11111);
                            return;
                        }
                        //CHECK FOR ADDITIONAL RAM
                        return;
                }

            }

            if (index == 0xFF45 || index == 0xFF44 || index == 0xFF41)
            {
                PPU.STATupdate();
            }
            if (index == 0xFF50)
            {
                //unmaps boot rom
                for (var i = 0; i < 0x100; i++)
                {

                    Memory.RAM[i] = Memory.ROM[i];


                }
            }

            if (index == 0xFF02 && Data == 0x81)
            {
                //prints out anything sent to the serial port
                Console.Write((char)RAM[0xFF01]);
            }
            if (index >= 0xE000 && index <= 0xFDFF)
            {
                //redirect writes to echo ram
                RAM[index - 0xE000 + 0xC000] = Data;
                return;
            }
            //writes the given data at the address in memory
            if (index == 0xFF46)
            {
                OAMDMATransfer(Data);
            }
            RAM[index] = Data;
        }

        public static void OAMDMATransfer(byte source)
        {
            //copies data from the given address to the object attribute memory
            for (var i = 0; i < 0xA0; i++)
            {
                RAM[0xFE00 + i] = RAM[i + (source << 8)];
            }
            Program.Tick(160);
        }

        public static ushort MemRead16b(ushort address)
        {
            //reads a 16b number from the address in memory
            return (ushort)(RAM[address] + (RAM[(ushort)(address + 1)] << 8));
        }
    }




}