using Raylib_cs;
using EmuMemory;
using System.ComponentModel;
namespace Rendering
{
    static class PPU
    {
        static int AddressingMode = 0;
        public static void RenderLoop()
        {

            if (Raylib.WindowShouldClose())
            {
                Raylib.CloseWindow();
            }
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);
            AddressingMode = (Memory.MemRead(0xFF40) & 0b00010000) >> 4;
            BGRender();
            Raylib.EndDrawing();
            //requests VBlank interrupt
            Memory.MemWrite(0xFF0F, (byte)(Memory.MemRead(0xFF0F) | 1));
            if ((Memory.MemRead(0xFF0F) & 1) != 1)
            {
                throw new Exception();
            }

        }

        public static void BGRender()
        {
            byte SCY = Memory.MemRead(0xFF42);
            byte SCX = Memory.MemRead(0xFF43);
            for (var i = 0; i < (32 * 32); i++)
            {
                byte TPX = (byte)(i % 32);
                byte TPY = (byte)Math.Floor((decimal)(i / 32));
                byte TileIndex = Memory.MemRead((ushort)(0x9800 + i));

                if (1 == 1)
                {
                    for (var j = 0; j < 64; j++)
                    {
                        byte val = Memory.MemRead((ushort)(TileIndex * 16 + 0x8000 + (j * 2) - 0x100));
                        if ((val << (j % 8)) >> (7 - (j % 8)) == 0)


                        {
                            Raylib.DrawPixel((i % 32) * 8 + j, i / 32 + j, Color.Black);
                        }
                    }


                }






            }
            //fetch tile indexes from the VRAM tile maps
            //figure out position in that tile map
            //draw to the screen

        }
    }
}