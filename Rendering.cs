using Raylib_cs;
using EmuMemory;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
namespace Rendering
{
    static class PPU
    {

        static int AddressingMode
        {
            get
            {
                return (LCDC & 0b00010000) >> 4;
            }
        }
        static int LCDC3
        {
            get
            {
                return (LCDC & 0b00001000) >> 3;
            }


        }
        static byte LCDC
        {
            get
            {
                return Memory.MemRead(0xFF40);
            }

            set
            {
                Memory.MemWrite(0xFF40, value);
            }
        }

        static byte LYC
        {
            get
            {
                return Memory.MemRead(0xFF45);
            }

            set
            {
                Memory.MemWrite(0xFF45, value);

            }
        }

        static byte STAT
        {
            get
            {
                return Memory.MemRead(0xFF41);
            }

            set
            {
                Memory.MemWrite(0xFF41, value);
            }
        }





        static int ModeLength;
        private static bool InterruptLine = false;
        static byte crntMode
        {
            get
            {
                return (byte)(STAT & 0b11);
            }
            set
            {
                if (value > 3 || value < 0)
                {
                    throw new InvalidOperationException("mode can only be 0-3");
                }
                STAT = (byte)((STAT & 0b11111100) + value);
                STATupdate();
            }
        }
        static int accruedDots;
        static byte LY
        {
            get
            {
                return Memory.MemRead(0xFF44);
            }

            set
            {
                Memory.MemWrite(0xFF44, value);

            }
        }


        public static void STATupdate()
        {
            byte SelectionOptions = (byte)(STAT >> 3);

            if (LY == LYC)
            {
                Memory.RAM[0xFF41] |= 0b100;
            }
            else Memory.RAM[0xFF41] &= 0b11111011;

            //construct a value to AND with the selection 
            byte SelectionValues =
            (byte)(Convert.ToByte(crntMode == 0) +
            (Convert.ToByte(crntMode == 1) << 1) +
            (Convert.ToByte(crntMode == 2) << 2) +
            (Convert.ToByte(LY == LYC) << 3));

            bool NewInterruptLine = (SelectionValues & SelectionOptions) > 0;
            if (!InterruptLine && NewInterruptLine)
            {
                Interrupts.SetInterruptRequest(Sources.STAT);
            }
            InterruptLine = NewInterruptLine;


        }

        public static void PassDots(int dots)
        {
            if (Raylib.WindowShouldClose())
            {
                Raylib.CloseWindow();
            }
            switch (crntMode)
            {
                case 2:


                    accruedDots += dots;

                    if (accruedDots > 79)
                    {
                        //mode 2 "OAM scan" takes 80 dots: currently we dont do anything in it
                        //update mode and pass the rest of the dots to the next mode
                        crntMode = 3;
                        int excess = accruedDots - 80;
                        accruedDots = 0;

                        //compute time the next mode will take
                        //SCX penalty
                        byte SCXPenalty = (byte)(Memory.MemRead(0xFF43) % 8);

                        //WINDOW penalty
                        //window penalty code TODO

                        //OBJ penalty 
                        //obj penalty code TODO

                        int ModeLength = SCXPenalty + 172;

                        PassDots(excess);
                    }

                    break;
                case 3:
                    //mode 3 "Drawing pixels" waits a certain amount, then draws a line to the screen



                    accruedDots += dots;


                    if (accruedDots >= ModeLength)
                    {
                        //draw line to the screen

                        RenderLine(LY);


                        //prepare everythingfor the waiting mode
                        crntMode = 0;
                        int excess = accruedDots - ModeLength;
                        accruedDots = 0;
                        ModeLength = 376 - ModeLength;

                        PassDots(excess);


                    }

                    break;

                case 0:
                    accruedDots += dots;
                    if (accruedDots >= ModeLength)
                    {

                        //prepare everything for the next scanline
                        crntMode = 2;
                        LY++;
                        int excess = accruedDots - ModeLength;
                        accruedDots = 0;
                        ModeLength = 80;

                        if (LY == 144)
                        {
                            //in case that was the last scanline, enter VBlank
                            crntMode = 1;
                            ModeLength = 4560;
                            //request VBlank interrupt
                            Interrupts.SetInterruptRequest(Sources.VBlank);
                            Raylib.EndDrawing();
                        }

                        PassDots(excess);


                    }
                    break;

                case 1:
                    //Vblank
                    accruedDots += dots;

                    if (accruedDots >= 456)
                    {
                        int excess = accruedDots - 456;
                        accruedDots = 0;
                        LY++;
                        PassDots(excess);
                    }
                    if (LY == 154)
                    {
                        //reset to OAM scan, reset to top line, reset dots
                        crntMode = 2;
                        int excess = accruedDots - 4560;
                        accruedDots = 0;
                        LY = 0;
                        Raylib.BeginDrawing();
                        PassDots(excess);
                        Raylib.ClearBackground(Color.White);
                        // Console.WriteLine(1 / Raylib.GetFrameTime());
                    }
                    break;

            }
        }
        public static void RenderLine(int line)
        {
            //checks if LCD is enabled at all
            if ((LCDC & 0b10000000) > 0)
            {
                //checks for bg and window enable
                if ((LCDC & 1) > 0)
                {
                    BGRender(line);
                    //checks for window enable
                    if ((LCDC & 0b00100000) > 0)
                    {
                        WindowRender(line);
                    }
                }
                else Raylib.ClearBackground(Color.White);

                //checks for obj enable
                if ((LCDC & 0b10) > 0)
                {
                    OBJRender(line);
                }
                //Raylib.DrawText((1 / Raylib.GetFrameTime()).ToString(), 0, 30, 30, Color.Blue);




            }

        }

        public static void WindowRender(int line)
        {
            //sets which tilemap the window uses
            int MapOffset = 0x9800;
            if ((LCDC & 0b01000000) > 0)
            {
                MapOffset = 0x9C00;
            }

        }

        public static void OBJRender(int line)
        {
            int OBJSize = (1 + ((LCDC & 0b100) >> 2)) * 8;

            ushort ObjTileDataOffset = 0x8000;
            ushort OAMOffset = 0xFE00;
            List<Byte[]> PossibleObjs = new List<byte[]>();

            for (var ObjIndex = 0; ObjIndex < 40; ObjIndex++)
            {

                //enforce 10 obj per scanline limit
                if (PossibleObjs.Count == 10)
                {
                    break;
                }
                //makes a seperate object to store the data of the object we are looking at
                byte[] ObjData = new byte[4];

                //loads the data into the object
                for (var j = 0; j < 4; j++)
                {
                    ObjData[j] = Memory.MemRead((ushort)(OAMOffset + ObjIndex * 4 + j));
                }

                if (ObjData[0] != 0 && line > 130)
                {

                }

                //Y pos check
                if (ObjData[0] - 16 <= line && line - (ObjData[0] - 16) < OBJSize)
                {
                    PossibleObjs.Add(ObjData);

                }
            }
            for (var PX = 0; PX < 160; PX++)
            {
                //loops through all pixels on the line, checks every object if they overlap the pixel, if yes draw it
                for (var ObjIndex = 0; ObjIndex < PossibleObjs.Count; ObjIndex++)
                {
                    byte[] ObjData = PossibleObjs[ObjIndex];
                    //x check
                    if (ObjData[1] - 8 <= PX && PX - (ObjData[1] - 8) < 8)
                    {
                        //gets the palette the object uses
                        ushort Palette = Memory.MemRead((ushort)(((ObjData[3] & 0b00010000) >> 4) + 0xFF48));

                        //get position of the pixel in relation to the object
                        byte InObjX = (byte)(PX - (ObjData[1] - 8));
                        byte InObjY = (byte)(line - (ObjData[0] - 16));

                        //get position of the pixel in relation to the top left of the tile
                        byte InTileX = InObjX;
                        if ((ObjData[3] & 0b100000) > 0)
                        {
                            InTileX = (byte)(7 - InTileX);
                        }
                        byte InTileY = InObjY;

                        //handle large objects
                        byte TileNum = 0;
                        if (InObjY > 7)
                        {
                            TileNum = 1;
                            InTileY -= 8;
                        }
                        if ((ObjData[3] & 0b1000000) > 0)
                        {
                            if (OBJSize == 16)
                            {
                                TileNum = (byte)(1 - TileNum);
                            }
                            InTileY = (byte)(7 - InTileY);
                        }

                        //get the data of the tile the pixel is in
                        byte[] TileData = new byte[16];
                        for (var i = 0; i < 16; i++)
                        {
                            TileData[i] = Memory.MemRead((ushort)(ObjTileDataOffset + i + (ObjData[2] * 16 * (OBJSize / 8)) + TileNum));
                        }

                        //rowData contains the 2 bytes needed to compose the color data for one row
                        byte[] RowData = new byte[2];
                        RowData[0] = TileData[InTileY * 2];
                        RowData[1] = TileData[InTileY * 2 + 1];

                        //composes the 2 bit color index from the RowData
                        byte BitOne = (byte)((RowData[0] >> (7 - InTileX)) & 0b1);
                        byte BitTwo = (byte)((RowData[1] >> (7 - InTileX)) & 0b1);

                        //ColorIndex contains the data looked up in the palette to determine the actual color
                        byte ColorIndex = (byte)(BitOne + (BitTwo << 1));

                        //ColorNum contains the number form of the current color extracted from the pallete
                        byte ColorNum = (byte)((Palette & (0b00000011 << (ColorIndex * 2))) >> (ColorIndex * 2));

                        //switch statement through colornum to obtain the actual color in color form
                        Color PxlColor = Color.Blue;
                        switch (ColorNum)
                        {
                            case 00:
                                continue;

                            case 01:
                                PxlColor = Color.LightGray;
                                break;
                            case 02:
                                PxlColor = Color.DarkGray;
                                break;
                            case 03:
                                PxlColor = Color.Black;
                                break;
                        }

                        //draw the actual pixel to the screen
                        Raylib.DrawRectangle(PX * 4, line * 4, 4, 4, PxlColor);

                    }





                }
            }
        }

        public static void BGRender(int line)
        {

            byte SCY = Memory.MemRead(0xFF42);
            byte SCX = Memory.MemRead(0xFF43);
            byte PY = (byte)line;





            for (var PX = 0; PX < 160; PX++)
            {

                //P coordinates are the corrdinates of the pixel on the screen
                //SC coordinates is the scrolling variable
                //TP coordinates are the coordinates of the pixel in the greater 256x256 pixel map
                byte TPX = (byte)(PX + SCX);
                byte TPY = (byte)(PY + SCY);

                //Tile Coordinates are the coordinates of the tile in the 32x32 tilemap
                byte TileX = (byte)(TPX / 8);
                byte TileY = (byte)(TPY / 8);


                //InTile Coordinates are the coordinates of the pixel within the 8x8 tile its part of
                byte InTileX = (byte)(TPX % 8);
                byte InTileY = (byte)(TPY % 8);
                byte TileIndex = 255;
                if (LCDC3 == 0)
                {
                    TileIndex = Memory.MemRead((ushort)(0x9800 + TileX + TileY * 32));
                }
                else TileIndex = Memory.MemRead((ushort)(0x9C00 + TileX + TileY * 32));

                //TileIndex is the index the tile data has inside the tile data area





                //TileData is the 16b tile data containing the color indexes
                byte[] TileData = new byte[16];

                //make sure we pull the tile data from the correct places in memory
                if (AddressingMode == 1)
                {
                    for (var i = 0; i < 16; i++)
                    {
                        TileData[i] = Memory.MemRead((ushort)(0x8000 + TileIndex * 16 + i));
                    }
                }
                else if (AddressingMode == 0)
                {
                    for (var i = 0; i < 16; i++)
                    {
                        TileData[i] = Memory.MemRead((ushort)(0x9000 + (sbyte)TileIndex * 16 + i));
                    }
                }
                else throw new Exception("addressingmode was somehow neither 0 nor 1???");

                //rowData contains the 2 bytes needed to compose the color data for one row
                byte[] RowData = new byte[2];
                RowData[0] = TileData[InTileY * 2];
                RowData[1] = TileData[InTileY * 2 + 1];

                //composes the 2 bit color index from the RowData
                byte BitOne = (byte)((RowData[0] >> (7 - InTileX)) & 0b1);
                byte BitTwo = (byte)((RowData[1] >> (7 - InTileX)) & 0b1);

                //ColorIndex contains the data looked up in the palette to determine the actual color
                byte ColorIndex = (byte)(BitOne + (BitTwo << 1));

                //ColorNum contains the number form of the current color extracted from the pallete
                byte ColorNum = (byte)((Memory.MemRead(0xFF47) & (0b00000011 << (ColorIndex * 2))) >> (ColorIndex * 2));

                //switch statement through colornum to obtain the actual color in color form
                Color PxlColor = Color.Blue;
                switch (ColorNum)
                {
                    case 00:
                        PxlColor = Color.White;
                        break;
                    case 01:
                        PxlColor = Color.LightGray;
                        break;
                    case 02:
                        PxlColor = Color.DarkGray;
                        break;
                    case 03:
                        PxlColor = Color.Black;
                        break;
                }

                //draw the actual pixel to the screen
                Raylib.DrawRectangle(PX * 4, PY * 4, 4, 4, PxlColor);
            }



        }
    }
}