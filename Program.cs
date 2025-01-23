
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

public static class Program
{





    static void Main(string[] args)
    {
        Tests.RunTests();





    }
    public static void Execute(ushort CrntPC)
    {
        //fetch instruction from memory
        //increment Registers.PC by one
        Registers.PC++;
        byte instr = Memory.MemRead(CrntPC);
        int opcode = (0b11000000 & instr) >> 6;
        Instructions.yRegisterIndex = 0b00000111 & instr;
        Instructions.xRegisterIndex = (0b00111000 & instr) >> 3;
        Instructions.rrIndex = Instructions.xRegisterIndex >> 1;
        int yRegisterIndex = Instructions.yRegisterIndex;
        int xRegisterIndex = Instructions.xRegisterIndex;
        Instructions.n = Memory.MemRead((ushort)(CrntPC + 1));
        Instructions.e = (sbyte)Instructions.n;
        Instructions.instr = instr;
        Instructions.nn = (ushort)((Memory.MemRead((ushort)(CrntPC + 1))) | ((Memory.MemRead((ushort)(CrntPC + 2)) << 8)));
        Instructions.cc = Instructions.xRegisterIndex & 0b011;
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
                    case 0xCB:
                        switch (Instructions.n >> 3)
                        {
                            case 0b00010:
                                Instructions.instr = Instructions.n;
                                Instructions.RL();
                                break;
                            case 0b00011:
                                Instructions.instr = Instructions.n;
                                Instructions.RR();
                                break;
                        }
                        Instructions.RL();
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
