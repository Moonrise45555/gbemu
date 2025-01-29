using System.Data;
using Raylib_cs;


namespace EmuMemory
{
    public static class Input
    {
        private static byte JOYP
        {
            get
            {
                return (byte)Memory.RAM[0xFF00];
            }
            set
            {
                Memory.RAM[0xFF00] = (byte)(value);
            }
        }
        public static void GetInputs()
        {


            JOYP |= 0b1111;
            byte selectionMode = (byte)(JOYP >> 4);



            if (selectionMode == 0b10 || selectionMode == 0b00)
            {
                if (Raylib.IsKeyDown(KeyboardKey.W))
                {
                    JOYP &= 0b11111011;
                    Interrupts.SetInterruptRequest(Sources.Joypad);
                }
                if (Raylib.IsKeyDown(KeyboardKey.S))
                {
                    JOYP &= 0b11110111; Interrupts.SetInterruptRequest(Sources.Joypad);
                }
                if (Raylib.IsKeyDown(KeyboardKey.A))
                {
                    JOYP &= 0b11111101; Interrupts.SetInterruptRequest(Sources.Joypad);
                }
                if (Raylib.IsKeyDown(KeyboardKey.D))
                {
                    JOYP &= 0b11111110; Interrupts.SetInterruptRequest(Sources.Joypad);
                }
            }
            if (selectionMode == 0b1 || selectionMode == 0b00)
            {
                if (Raylib.IsKeyDown(KeyboardKey.Kp0))
                {
                    JOYP &= 0b11111101; Interrupts.SetInterruptRequest(Sources.Joypad);
                }
                if (Raylib.IsKeyDown(KeyboardKey.KpDecimal))
                {
                    JOYP &= 0b11111110; Interrupts.SetInterruptRequest(Sources.Joypad);
                }
                if (Raylib.IsKeyDown(KeyboardKey.Kp2))
                {
                    JOYP &= 0b11111011; Interrupts.SetInterruptRequest(Sources.Joypad);
                }
                if (Raylib.IsKeyDown(KeyboardKey.Kp3))
                {
                    JOYP &= 0b11110111; Interrupts.SetInterruptRequest(Sources.Joypad);
                }
            }








        }







    }
}
