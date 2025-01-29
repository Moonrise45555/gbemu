
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks.Dataflow;
using EmuMemory;
using Execution;
using System.ComponentModel;
using QuickType;
using Raylib_cs;
using Rendering;
using System.Collections;
using Debug;


public static class Program
{

    public static bool Halted = false;
    public const long DMGFREQUENCY = 4194304;
    static int AccumulatorDIV = 0;
    static int AccumulatorTIMA = 0;

    public static void Tick(int number = 1)
    {
        PPU.PassDots(number);




        //////
        //Timer stuff
        //////


        AccumulatorDIV += number;

        //counts the "excess" number
        if (AccumulatorDIV >= 64)
        {
            //if over T cycle number, increment timer and reset accumulator
            AccumulatorDIV -= 64;
            Memory.MemWrite(0xFF04, (byte)(Memory.MemRead(0xFF04) + 1));
        }

        //check if TIMA is enabled first
        if ((Memory.MemRead(0xFF07) & 0b100) > 1)
        {
            //determines the frequency with which TIMA is meant to be incremented atm
            int TIMAfrequency = 0;
            switch (Memory.MemRead(0xFF07) & 0b11)
            {
                case 00:
                    TIMAfrequency = 256;
                    break;
                case 0b01:
                    TIMAfrequency = 4;
                    break;
                case 0b10:
                    TIMAfrequency = 16;
                    break;
                case 0b11:
                    TIMAfrequency = 64;
                    break;
            }

            //increments TIMA if enough cycles have passed
            AccumulatorTIMA += number;
            if (AccumulatorTIMA >= TIMAfrequency)
            {
                //if overflow, reset to TMA and request interrupt
                if (Memory.MemRead(0xFF05) == 255)
                {

                    //reset to TMA
                    Memory.MemWrite(0xFF05, Memory.MemRead(0xFF06));

                    //request timer interrupt
                    Memory.MemWrite(0xFF0F, (byte)(Memory.MemRead(0xFF0F) | 0b100));

                }
                else Memory.MemWrite(0xFF05, (byte)(Memory.MemRead(0xFF05) + 1));
                //if over T cycle number, increment timer and reset accumulator
                AccumulatorTIMA -= TIMAfrequency;

            }


        }







    }

    static Sources[] sources = (Sources[])Enum.GetValues(typeof(Sources));
    public static bool CheckInterrupts()
    {
        //loops through every interrupt and checks whether its getting called

        foreach (Sources source in sources)
        {
            if (Interrupts.CheckInterruptValidRequest(source))
            {
                //disables interrupts
                Registers.IME = 0;

                //unrequestes the interrupt
                Interrupts.SetInterruptRequest(source, 0);

                //CALLs the interrupt handler
                Registers.setr16((ushort)(Registers.getr16(3) - 2), 3);
                Memory.MemWrite16b(Registers.getr16(3), Registers.PC);

                Registers.PC = Interrupts.InterruptHandler[source];


                return true;
            }
        }
        return false;

    }





    static void Main(string[] args)
    {

        Raylib.InitWindow(160 * 4, 144 * 4, "mario for thie wii?");

        Registers.PC = 0x000;


        Memory.BootROM = File.ReadAllBytes("../../../dmg0_boot.bin");
        Memory.ROM = File.ReadAllBytes("../../../sml.gb");

        for (var i = 0; i < Memory.BootROM.Length; i++)
        {
            Memory.RAM[i] = Memory.BootROM[i];
        }


        for (var i = 0; i < Math.Min(0x8000, Memory.ROM.Length); i++)
        {
            if (i >= 0x100)
            {
                Memory.RAM[i] = Memory.ROM[i];
            }

        }
        Memory.MBCType = Memory.MemRead(0x0147);
        long cycle = 0;
        Registers.r8[7] = 0x01;
        Registers.Flags = 0xB0;
        Registers.r8[0] = 0;
        Registers.r8[1] = 0x13;
        Registers.r8[2] = 0;
        Registers.setr16(0xFFFE, 3);
        Registers.r8[3] = 0xD8;
        Registers.r8[4] = 0x01;
        Registers.r8[5] = 0x4D;

        List<string> a = new List<string>();
        while (true)
        {




            if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.B))
            {
                if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.C))
                {
                    Dumping.DumpRange(0x8000, 0x97FF, "TILE DATA");


                }
                else
                {
                    Dumping.DumpRange(0x9800, 0x9bff, "TILE MAP");
                }

                throw new Exception();
            }
            cycle++;
            if (cycle % 4000000 == 0)
            {
                Console.WriteLine(1 / Raylib.GetFrameTime());
            }



            Execute(Registers.PC);
            /* a.Add("A:" + Registers.r8[7].ToString("X2") + " F:" + Registers.Flags.ToString("X2") + " B:" + Registers.r8[0].ToString("X2") +
             " C:" + Registers.r8[1].ToString("X2") + " D:" + Registers.r8[2].ToString("X2") + " E:" + Registers.r8[3].ToString("X2") +
             " H:" + Registers.r8[4].ToString("X2") + " L:" + Registers.r8[5].ToString("X2") + " SP:" + Registers.getr16(3).ToString("X4") +
             " PC:" + Registers.PC.ToString("X4") + " PCMEM:" + Memory.MemRead(Registers.PC).ToString("X2") + "," + Memory.MemRead((ushort)(Registers.PC + 1)).ToString("X2") + ","
              + Memory.MemRead((ushort)(Registers.PC + 2)).ToString("X2") + "," + Memory.MemRead((ushort)(Registers.PC + 3)).ToString("X2"));*/
            /* if (Raylib.IsKeyPressed(KeyboardKey.I))
             {


                 // Write the string array to a new file named "WriteLines.txt".
                 using (StreamWriter outputFile = new StreamWriter("../thing.txt"))
                 {
                     foreach (string line in a)
                         outputFile.WriteLine(line);
                 }
                 throw new Exception();
             }*/

            /* using (StreamWriter outputFile = new StreamWriter("../thing.txt"))
             {
                 foreach (string line in a)
                     outputFile.WriteLine(line);
             }
             throw new Exception();*/





        }






    }
    public static void Execute(ushort CrntPC)
    {


        //does not execute anything if HALTed
        if (Halted)
        {
            if ((Memory.MemRead(0xFFFF) & Memory.MemRead(0xFF0F)) == 0)
            {
                Tick();
                return;
            }
            Halted = false;
        }
        //handle the delaying behaviour of EI

        Registers.IMECheck();


        //jumps to interrupts if needed
        if (CheckInterrupts())
        {
            CrntPC = Registers.PC;
        }




        //fetch instruction from memory
        //increment Registers.PC by one

        Registers.PC++;

        byte instr = Memory.MemRead(CrntPC);
        byte AdjInstruction = instr;
        Instructions.CBprefixed = false;
        if (instr == 0xCB)
        {
            Instructions.CBprefixed = true;
            AdjInstruction = Memory.MemRead((ushort)(CrntPC + 1));
        }
        int opcode = (0b11000000 & instr) >> 6;
        Instructions.yRegisterIndex = 0b00000111 & AdjInstruction;
        Instructions.xRegisterIndex = (0b00111000 & AdjInstruction) >> 3;
        Instructions.rrIndex = Instructions.xRegisterIndex >> 1;
        int yRegisterIndex = Instructions.yRegisterIndex;
        int xRegisterIndex = Instructions.xRegisterIndex;
        Instructions.n = Memory.MemRead((ushort)(CrntPC + 1));
        Instructions.e = (sbyte)Instructions.n;
        Instructions.instr = AdjInstruction;
        Instructions.nn = (ushort)((Memory.MemRead((ushort)(CrntPC + 1))) | ((Memory.MemRead((ushort)(CrntPC + 2)) << 8)));
        Instructions.cc = Instructions.xRegisterIndex & 0b011;
        if (Registers.PC > 568)
        {

        }


        switch (opcode)
        {

            case 0b00:
                if ((instr | 0b00011000) == 0b00111000)
                {
                    Instructions.JPccnn();
                }

                switch (instr)
                {
                    case 0:
                        Instructions.NOP();
                        break;

                    case 0b00000111:
                        //RLCA
                        Instructions.RLC();
                        break;
                    case 0b00001111:
                        //RRCA
                        Instructions.RRC();
                        break;
                    case 0b00010111:
                        //rla
                        Instructions.RL();
                        break;
                    case 0b00011111:
                        //rra
                        Instructions.RR();
                        break;
                    case 0b00100111:
                        //DAA
                        Instructions.DAA();
                        break;
                    case 0b00101111:
                        //CPL
                        Instructions.CPL();
                        break;
                    case 0b00110111:
                        //SCF
                        Instructions.SCF();
                        break;
                    case 0b00111111:
                        //CCF
                        Instructions.CCF();
                        break;
                    case 0b00001000:
                        Instructions.LDnnsp();
                        break;
                    case 0b00011000:
                        Instructions.JRe();
                        break;
                    case 0b00010000:
                        //STOP
                        break;

                }
                switch (instr & 0x0F)
                {
                    case 0b0001:
                        //ld rn
                        Instructions.LDrrnn();
                        break;
                    case 0b0010:
                        //ld rmem a
                        //WRONG
                        Instructions.LDamem();
                        break;
                    case 0b1010:
                        //ld a rmem
                        Instructions.LDmema();
                        break;
                    case 0b0011:
                        //inc r16
                        Instructions.INCrr();
                        break;
                    case 0b1011:
                        //dec r16
                        Instructions.DECrr();
                        break;
                    case 0b1001:
                        //add hl r16
                        Instructions.ADDhlrr();
                        break;
                }
                switch (0b00000111 & instr)
                {
                    case 0b100:
                        //inc r8
                        Instructions.INCr();
                        break;
                    case 0b101:
                        //dec r8
                        Instructions.DECr();
                        break;
                    case 0b110:
                        //ld r8 imm8
                        Instructions.LDrn();
                        break;
                }

                break;

            case 0b01:
                //LD instruction
                //moves data from the 2nd register to the 1st or an immediate value from the next memory byte
                Instructions.LDgeneral();
                break;

            case 0b11:
                switch (instr & 0x0F)
                {
                    case 1:
                        Instructions.pop();
                        break;
                    case 0b101:
                        Instructions.push();
                        break;
                }
                switch (instr & 0b00000111)
                {
                    case 0b000:
                        if (instr == 0b11100000)
                        {
                            Instructions.LDHna();
                        }
                        else
                        if (instr != 0b11101000 && instr != 0b11110000 && instr != 0b1111001 && instr != 0b11110010 && instr != 0b11111000)
                            Instructions.RETcc();
                        break;
                    case 0b010:
                        if (instr != 0b11100010 && instr != 0b11101010 && instr != 0b11110010 && instr != 0b11111010)
                            Instructions.JPccnnreal();
                        break;
                    case 0b100:
                        Instructions.CALLccnn();
                        break;
                    case 0b111:
                        Instructions.RST();
                        break;
                }
                switch (instr)
                {
                    case 0b11000110:
                        Instructions.ADD();
                        break;
                    case 0b11001110:
                        Instructions.ADC();
                        break;
                    case 0b11010110:
                        Instructions.SUBn();
                        break;
                    case 0b11011110:
                        Instructions.SBC();
                        break;
                    case 0b11100110:
                        Instructions.AND();
                        break;
                    case 0b11101110:
                        Instructions.XOR();
                        break;
                    case 0b11110110:
                        Instructions.OR();
                        break;
                    case 0b11111110:
                        Instructions.CPn();
                        break;
                    case 0b11000011:
                        Instructions.JP();
                        break;
                    case 0b11101001:
                        Instructions.JPhl();
                        break;
                    case 0b11110011:
                        Instructions.DI();
                        break;
                    case 0b11001001:
                        Instructions.RET();
                        break;
                    case 0b11011001:
                        Instructions.RETi();
                        break;
                    case 0b11001101:
                        Instructions.CALLnn();
                        break;
                    case 0b11100010:
                        Instructions.LDHca();
                        break;
                    case 0b11100000:
                        //Instructions.LDHna();
                        break;
                    case 0b11101010:
                        Instructions.LDnna();
                        break;
                    case 0b11110010:
                        Instructions.LDHac();
                        break;
                    case 0b11110000:
                        Instructions.LDHan();
                        break;
                    case 0b11111010:
                        Instructions.ldann();
                        break;
                    case 0b11101000:
                        Instructions.ADDspe();
                        break;
                    case 0b11111000:
                        Instructions.AdjustedStack();
                        break;
                    case 0b11111001:
                        Instructions.LDsphl();
                        break;
                    case 0b11111011:
                        Instructions.EI();
                        break;

                    case 0xCB:
                        Registers.PC++;
                        switch (AdjInstruction >> 6)
                        {
                            case 0b01:
                                Instructions.BITur();
                                break;
                            case 0b10:
                                Instructions.RESETur();
                                break;
                            case 0b11:
                                Instructions.SETur();
                                break;
                        }
                        switch (Instructions.n >> 3)
                        {
                            case 0b00010:

                                Instructions.RL();
                                break;
                            case 0b00011:

                                Instructions.RR();
                                break;
                            case 0b00000:

                                Instructions.RLC();
                                break;
                            case 0b00001:

                                Instructions.RRC();
                                break;
                            case 0b00100:
                                Instructions.SLA();
                                break;
                            case 0b00101:
                                Instructions.SRA();
                                break;
                            case 0b00110:
                                Instructions.SWAPr();
                                break;
                            case 0b00111:
                                Instructions.SRL();
                                break;
                        }

                        break;

                }
                break;
            case 0b10:


                switch (instr >> 3)
                {
                    case 0b10000:
                        Instructions.ADD();
                        break;
                    case 0b10001:
                        Instructions.ADC();
                        break;
                    case 0b10010:
                        if (yRegisterIndex == 6) Instructions.SUBhl();
                        else
                            Instructions.SUBr();
                        break;
                    case 0b10011:
                        Instructions.SBC();
                        break;
                    case 0b10100:
                        Instructions.AND();
                        break;
                    case 0b10101:
                        Instructions.XOR();
                        break;
                    case 0b10110:
                        Instructions.OR();
                        break;
                    case 0b10111:
                        if (yRegisterIndex == 6) Instructions.CPhl();
                        else Instructions.CPr();
                        break;
                }
                break;
        }

    }
}
