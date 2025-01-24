
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

public static class Program
{





    static void Main(string[] args)
    {
        Raylib.InitWindow(256, 256, "mario for thie wii?");

        Registers.PC = 0x0;

        byte[] a = File.ReadAllBytes("../../../dmg0_boot.bin");
        for (var i = 0; i < a.Length; i++)
        {
            Memory.RAM[i] = a[i];
        }
        long cycle = 0;
        while (true)
        {

            if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.B))
            {
                if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.C))
                {
                    for (var i = 0; i < (0x97FF - 0x8000); i++)
                    {
                        Console.Write(Memory.MemRead((ushort)(0x8000 + i)).ToString("X2") + " ");
                        if (i % 0x10 == 0)
                        {
                            Console.WriteLine();
                        }





                    }
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    throw new Exception();
                }
                for (var i = 0; i < (0x9bff - 0x9800); i++)
                {
                    Console.Write(Memory.MemRead((ushort)(0x9800 + i)).ToString("X2") + " ");
                    if (i % 0x20 == 0)
                    {
                        Console.WriteLine();
                    }





                }
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                throw new Exception();
            }
            cycle++;


            Execute(Registers.PC);

            if (cycle % 10000 == 0)
            {
                Console.WriteLine(Registers.PC.ToString("X4"));
                PPU.RenderLoop();

            }


        }






    }
    public static void Execute(ushort CrntPC)
    {

        if (((Memory.MemRead(0xFF0F) & 1) == 1))
        {
            Registers.IME = 0;
            Memory.MemWrite(0xFF0F, (byte)(Memory.MemRead(0xFF0F) & 0xFE));

            Registers.setr16((ushort)(Registers.getr16(3) - 2), 3);

            Memory.MemWrite16b(Registers.getr16(3), Registers.PC);
            CrntPC = 0x40;
            Registers.PC = 0x40;


        }

        //fetch instruction from memory
        //increment Registers.PC by one
        if (CrntPC == 0x0024)
        {

        }
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
        //Console.WriteLine("executing " + instr.ToString("X4"));

        switch (opcode)
        {

            case 0b00:
                if ((instr | 0b00011000) == 0b00111000)
                {
                    Instructions.JPccnn();
                }

                switch (instr)
                {

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
