namespace EmuMemory
{
    public static class Registers
    {
        public static byte IME = 0;
        public static ushort PC;
        public static byte Flags = 0;

        public static bool IfCondition(int ConditionIndex)
        {

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
        public static Byte[] RAM = new byte[65536];
        public static void MemWrite16b(ushort address, ushort data)
        {
            Memory.MemWrite(address, (byte)(data & 0x00FF));
            Memory.MemWrite((ushort)(address + 1), (byte)(data >> 8));
        }
        public static byte MemRead(ushort index)
        {
            return RAM[index];
        }

        public static void MemWrite(ushort index, byte Data)
        {
            RAM[index] = Data;
        }

        public static ushort MemRead16b(ushort address)
        {
            return (ushort)(RAM[address] + (RAM[address + 1] << 8));
        }
    }




}