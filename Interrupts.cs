using System.Net;
using EmuMemory;
namespace EmuMemory
{
    public enum Sources
    {
        VBlank,
        STAT,
        Timer,
        Serial,
        Joypad

    }




    public static class Interrupts
    {
        public static Dictionary<Sources, ushort> InterruptHandler = new Dictionary<Sources, ushort>()
        {
            {Sources.VBlank, 0x40},
            {Sources.STAT, 0x48},
            {Sources.Timer, 0x50},
            {Sources.Serial, 0x58},
            {Sources.Joypad, 0x60}
        };



        public static byte IF
        {
            get
            {
                return Memory.MemRead(0xFF0F);
            }
            set
            {
                Memory.MemWrite(0xFF0F, value);
            }
        }

        public static byte IE
        {
            get
            {
                return Memory.MemRead(0xFFFF);
            }
            set
            {
                Memory.MemWrite(0xFFFF, value);
            }
        }

        public static void SetInterruptRequest(Sources source, int value = 1)
        {
            //since the enum casted to a number is equal to the bit index inside IF and IE, it can use those to shift 
            IF = (byte)((IF & (~(0b1 << (int)source))) | (value << (int)source));
        }


        public static bool CheckInterruptValidRequest(Sources source)
        {
            //since the enum casted to a number is equal to the bit index inside IF and IE, it can use those to bit shift
            return ((IE & IF & (1 << (int)source)) >= 1) && Registers.IME == 1;
        }
    }
}