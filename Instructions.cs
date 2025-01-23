


using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using EmuMemory;
namespace Execution
{



    static class Instructions
    {
        public static bool CBprefixed;

        public static ushort nn;
        public static byte n;
        public static sbyte e;
        public static byte instr;
        public static int xRegisterIndex;
        public static int rrIndex;
        public static int yRegisterIndex;

        public static int cc;


        public static void NOP()
        {
            //nop, what do you expect?
        }
        public static void LoadFromBCToA()
        {
            //loads byte from [BC] to A 
            Registers.r8[7] = Memory.MemRead(Registers.getr16(0));
        }

        public static void LDrrnn()
        {
            //loads nn into the specified register
            Registers.setr16(nn, rrIndex);
            Registers.PC += 2;
        }

        public static void LDnnsp()
        {
            //loads data from SP register to [nn]
            //TODO:STACK REGISTER 
            Memory.MemWrite16b(nn, Registers.getr16(3));
            Registers.PC += 2;
        }

        public static void LDhlda()
        {
            //loads A to [hl] then decrements hl
            Memory.MemWrite(Registers.getr16(2), Registers.r8[7]);
            Registers.setr16((ushort)(Registers.getr16(2) - 1), 2);
        }

        public static void LDahld()
        {
            //loads [hl] to a then decrements hl
            Registers.r8[7] = Memory.MemRead(Registers.getr16(2));
            Registers.setr16((ushort)(Registers.getr16(2) - 1), 2);
        }
        public static void LDhlia()
        {
            //loads A to [hl] then decrements hl
            Memory.MemWrite(Registers.getr16(2), Registers.r8[7]);
            Registers.setr16((ushort)(Registers.getr16(2) + 1), 2);
        }
        public static void LDahli()
        {
            //loads [hl] to a then decrements hl
            Registers.r8[7] = Memory.MemRead(Registers.getr16(2));
            Registers.setr16((ushort)(Registers.getr16(2) + 1), 2);
        }

        public static void LDmema()
        {
            if (rrIndex == 3)
            {
                Registers.r8[7] = Memory.MemRead(Registers.getr16(2));
                //redirect to HL instead of SP
            }
            else
                Registers.r8[7] = Memory.MemRead(Registers.getr16(rrIndex));
            if (rrIndex == 3)
                Registers.setr16((ushort)(Registers.getr16(2) - 1), 2);
            if (rrIndex == 2)
                Registers.setr16((ushort)(Registers.getr16(2) + 1), 2);
        }
        public static void LDamem()
        {
            if (rrIndex == 3)
            {
                Memory.MemWrite(Registers.getr16(2), Registers.r8[7]);
                //redirect to HL instead of SP
            }
            else
                Memory.MemWrite(Registers.getr16(rrIndex), Registers.r8[7]);
            if (rrIndex == 3)
                Registers.setr16((ushort)(Registers.getr16(2) - 1), 2);
            if (rrIndex == 2)
                Registers.setr16((ushort)(Registers.getr16(2) + 1), 2);
        }

        public static void LDbca()
        {
            Memory.MemWrite(Registers.getr16(0), Registers.r8[7]);
        }

        public static void LDdea()
        {
            Memory.MemWrite(Registers.getr16(1), Registers.r8[7]);
        }


        public static void LDrn()
        {
            if (xRegisterIndex == 6)
            {
                Memory.MemWrite(Registers.getr16(2), n);
                Registers.PC++;
                return;
            }
            Registers.r8[xRegisterIndex] = n;
            Registers.PC++;
        }

        public static void LDgeneral()
        {
            if (yRegisterIndex == 6)
            {
                if (xRegisterIndex == 6)
                {
                    //HALT
                    return;
                }
                //load to register
                Registers.r8[xRegisterIndex] = Memory.MemRead(Registers.getr16(2));
            }
            else if (xRegisterIndex == 6)
            {
                //load from register
                Memory.MemWrite(Registers.getr16(2), Registers.r8[yRegisterIndex]);
            }
            else Registers.r8[xRegisterIndex] = Registers.r8[yRegisterIndex];
        }

        public static void push()
        {
            //pushes rrIndex to the stack and moves the stack pointer accordingly
            Registers.setr16((ushort)(Registers.getr16(3) - 2), 3);
            int rr = rrIndex;
            if (rr == 3)
            {
                Memory.MemWrite(Registers.getr16(3), Registers.r8[7]);
                Memory.MemWrite((ushort)(Registers.getr16(3) + 1), Registers.Flags);
            }
            else
                Memory.MemWrite16b(Registers.getr16(3), Registers.getr16(rr));

        }

        public static void pop()
        {
            int rr = rrIndex;
            if (rr == 3)
            {
                //if the register to reach is the 4th one, redirect to AF instead of the stack pointer
                Registers.r8[7] = Memory.MemRead(Registers.getr16(3));
                Registers.setr16((ushort)(Registers.getr16(3) + 1), 3);
                Registers.Flags = Memory.MemRead(Registers.getr16(3));
                Registers.setr16((ushort)(Registers.getr16(3) + 1), 3);
                return;
            }
            Registers.setr16(Memory.MemRead16b(Registers.getr16(3)), rr);
            Registers.setr16((ushort)(Registers.getr16(3) + 2), 3);

        }

        public static void LDsphl()
        {
            Registers.setr16(Registers.getr16(3), 3);
        }

        public static void ldann()
        {
            //load from address specified by 16 bit immediate into accumulator
            Registers.r8[7] = Memory.MemRead(nn);
            Registers.PC += 2;
        }

        public static void LDnna()
        {
            //load from accumulator into address specified by 16 bit immediate
            Memory.MemWrite(nn, Registers.r8[7]);
            Registers.PC += 2;
        }

        public static void LDHac()
        {
            //load from C + 0xFF into A??
            Registers.r8[7] = Memory.MemRead((ushort)(0xFF00 + Registers.r8[1]));
        }

        public static void LDHca()
        {
            //load from A into C + 0xFF00???
            Memory.MemWrite((ushort)(0xFF00 + Registers.r8[1]), Registers.r8[7]);
        }

        public static void LDHan()
        {
            //load into A from n + 0xFF00
            Registers.r8[7] = Memory.MemRead((ushort)(n + 0xFF00));
            Registers.PC++;
        }

        public static void LDHna()
        {
            //load from n + 0xFF00 into A
            Memory.MemWrite((ushort)(n + 0xFF00), Registers.r8[7]);
            Registers.PC++;
        }

        public static void ADD()
        {
            byte addvalue = Registers.r8[yRegisterIndex];
            if (yRegisterIndex == 6 && (0b01000000 & instr) < 1)
            {
                addvalue = Memory.MemRead(Registers.getr16(2));
            }
            if ((0b01000000 & instr) > 1)
            {
                addvalue = n;
                Registers.PC++;
            }
            Registers.SetFlags8bAdd(Registers.r8[7], addvalue);
            Registers.r8[7] = (byte)(Registers.r8[7] + addvalue);

        }
        public static void AdjustedStack()
        {
            //write n + sp (n being a signed integer) to HL

            Registers.SetFlags8bAdd((byte)e, (byte)(Registers.getr16(3) & 0x00FF));
            Registers.setr16((ushort)(n + Registers.getr16(3)), 2);

            Registers.PC++;
        }



        public static void ADC()
        {
            //adds r and the carry flag to A and stores it in A
            //NOT OPCODED 0b10001xxx
            byte addvalue = (byte)(Registers.r8[yRegisterIndex]);
            byte CarryFlag = (byte)Registers.ReadFlag("C");
            if (yRegisterIndex == 6)
            {
                addvalue = Memory.MemRead(Registers.getr16(2));
            }
            if ((0b01000000 & instr) > 1)
            {
                addvalue = n;
                Registers.PC++;
            }
            Registers.SetFlags8bAdd(Registers.r8[7], addvalue, CarryFlag);
            Registers.r8[7] = (byte)(Registers.r8[7] + addvalue + CarryFlag);
        }


        public static void SUBr()
        {
            //subtracts r from A and store it in A
            //NOT OPCODED 0b10010xxx
            Registers.SetFlags8bAdd(Registers.r8[7], (byte)Registers.r8[yRegisterIndex], 0, "-");
            Registers.SetFlag("N", 1);
            Registers.r8[7] = (byte)(Registers.r8[7] - Registers.r8[yRegisterIndex]);
        }

        public static void SUBhl()
        {
            //subtracts [hl] from A and stores it in A
            //NOT OPCODED 0b10010110
            Registers.SetFlags8bAdd(Registers.r8[7], (byte)Memory.MemRead(Registers.getr16(2)), 0, "-");
            Registers.SetFlag("N", 1);
            Registers.r8[7] = (byte)(Registers.r8[7] - Memory.MemRead(Registers.getr16(2)));
        }

        public static void SUBn()
        {
            //subtracts n from A and stores it in A
            //NOT OPCODED 0b11010110
            Registers.SetFlags8bAdd(Registers.r8[7], (byte)n, 0, "-");
            Registers.SetFlag("N", 1);
            Registers.r8[7] = (byte)(Registers.r8[7] - n);
            Registers.PC++;
        }

        public static void SBC()
        {
            //subtracts r and the carry from A and stores it in A
            //NOT OPCODED 0b1x011xxx
            byte subtractval = Registers.r8[yRegisterIndex];
            byte CarryFlag = (byte)Registers.ReadFlag("C");
            if (yRegisterIndex == 6)
            {
                subtractval = Memory.MemRead(Registers.getr16(2));
            }
            if ((instr & 0b01000000) > 1)
            {
                subtractval = n;
                Registers.PC++;
            }
            Registers.SetFlags8bAdd(Registers.r8[7], (byte)subtractval, (byte)CarryFlag, "-");
            Registers.SetFlag("N", 1);
            Registers.r8[7] = (byte)(Registers.r8[7] - subtractval - CarryFlag);
        }



        public static void CPr()
        {
            //subr, but it doesnt update A
            //NOT OPCODED 0b10111xxx
            byte k = Registers.r8[7];
            SUBr();
            Registers.r8[7] = k;
        }

        public static void CPhl()
        {
            //subhl, but it doesnt update A
            //NOT OPCODED 0b10011110
            byte k = Registers.r8[7];
            SUBhl();
            Registers.r8[7] = k;
        }

        public static void CPn()
        {
            //subn, but it doesnt update A
            //NOT OPCODED 0b11111110
            byte k = Registers.r8[7];
            SUBn();
            Registers.r8[7] = k;
        }

        public static void INCr()
        {
            if (xRegisterIndex == 6)
            {
                INChl();
                return;
            }
            //increments data in R
            //NOT OPCODED 0b00xxx100
            int c = Registers.ReadFlag("C");
            Registers.SetFlags8bAdd(Registers.r8[xRegisterIndex], 1);
            Registers.r8[xRegisterIndex] = (byte)(Registers.r8[xRegisterIndex] + 1);
            Registers.SetFlag("C", c);

        }

        public static void DECr()
        {
            if (xRegisterIndex == 6)
            {
                DEChl();
                return;
            }
            //increments data in R
            //NOT OPCODED 0b00xxx100
            int c = Registers.ReadFlag("C");
            Registers.SetFlags8bAdd(Registers.r8[xRegisterIndex], 1, 0, "-");
            Registers.r8[xRegisterIndex] = (byte)(Registers.r8[xRegisterIndex] - 1);
            Registers.SetFlag("C", c);
            Registers.SetFlag("N", 1);

        }

        public static void DEChl()
        {

            //increments data in R
            //NOT OPCODED 0b00xxx100
            int c = Registers.ReadFlag("C");
            Registers.SetFlags8bAdd(Memory.MemRead(Registers.getr16(2)), 1, 0, "-");
            Memory.MemWrite(Registers.getr16(2), (byte)(Memory.MemRead(Registers.getr16(2)) - 1));
            Registers.SetFlag("C", c);
            Registers.SetFlag("N", 1);

        }

        public static void INChl()
        {
            //increments data at hl
            //NOT OPCODED 0b00110100
            int c = Registers.ReadFlag("C");
            Registers.SetFlags8bAdd(Memory.MemRead(Registers.getr16(2)), 1);
            Memory.MemWrite(Registers.getr16(2), (byte)(Memory.MemRead(Registers.getr16(2)) + 1));
            Registers.SetFlag("C", c);

        }

        public static void INCI()
        {
            unchecked
            {

                //decrements data in R
                //NOT OPCODED 0b00xxx101
                int c = Registers.ReadFlag("C");
                Registers.SetFlags8bAdd(Registers.r8[xRegisterIndex], (byte)-1);
                Memory.MemWrite(Registers.getr16(2), (byte)(Registers.r8[xRegisterIndex] - 1));
                Registers.SetFlag("N", 1);
                Registers.SetFlag("C", c);
            }

        }

        public static void AND()
        {
            //does AND between A and r, then stores it in A
            //NOT OPCODED 0b10100xxx
            byte otherval = Registers.r8[yRegisterIndex];

            if (yRegisterIndex == 6)
            {
                otherval = Memory.MemRead(Registers.getr16(2));
            }

            if ((instr & 0b01000000) > 0)
            {
                otherval = n;
                Registers.PC++;
            }

            Registers.r8[7] &= otherval;
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 1);
            Registers.SetFlag("C", 0);

            if (Registers.r8[7] == 0)
            {
                Registers.SetFlag("Z", 1);
            }
            else Registers.SetFlag("Z", 0);
        }

        public static void OR()
        {
            //does OR between A and r, then stores it in A
            //NOT OPCODED bunch of stuff
            byte otherval = Registers.r8[yRegisterIndex];

            if (yRegisterIndex == 6)
            {
                otherval = Memory.MemRead(Registers.getr16(2));
            }

            if ((instr & 0b01000000) > 0)
            {
                otherval = n;
                Registers.PC++;
            }

            Registers.r8[7] |= otherval;
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            Registers.SetFlag("C", 0);

            if (Registers.r8[7] == 0)
            {
                Registers.SetFlag("Z", 1);
            }
            else Registers.SetFlag("Z", 0);
        }
        public static void XOR()
        {
            //does OR between A and r, then stores it in A
            //NOT OPCODED bunch of stuff
            byte otherval = Registers.r8[yRegisterIndex];

            if (yRegisterIndex == 6)
            {
                otherval = Memory.MemRead(Registers.getr16(2));
            }

            if ((instr & 0b01000000) > 0)
            {
                otherval = n;
                Registers.PC++;
            }

            Registers.r8[7] ^= otherval;
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            Registers.SetFlag("C", 0);

            if (Registers.r8[7] == 0)
            {
                Registers.SetFlag("Z", 1);
            }
            else Registers.SetFlag("Z", 0);
        }

        public static void CCF()
        {
            //NOT OPCODED 0b00111111
            Registers.SetFlag("C", 1 - Registers.ReadFlag("C"));
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
        }

        public static void SCF()
        {
            //NOT OPCODED 0b00110111
            Registers.SetFlag("C", 1);
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
        }

        public static void DAA()
        {
            //DAA
            //BROKEN
            int c = 0;
            int offset = 0;


            if ((Registers.ReadFlag("N") == 0 && (Registers.r8[7] & 0x0F) > 9) || Registers.ReadFlag("H") == 1)
            {
                offset += 0x06;
            }

            if ((Registers.ReadFlag("N") == 0 && Registers.r8[7] > 0x99) || Registers.ReadFlag("C") == 1)
            {
                c = 1;
                offset += 0x60;
            }



            if (Registers.ReadFlag("N") == 1)
            {
                Registers.r8[7] = (byte)(Registers.r8[7] - offset);
            }
            else Registers.r8[7] = (byte)(Registers.r8[7] + offset);


            if (Registers.r8[7] == 0)
            {
                Registers.SetFlag("Z", 1);
            }
            else Registers.SetFlag("Z", 0);
            Registers.SetFlag("H", 0);
            Registers.SetFlag("C", (byte)c);

        }

        public static void CPL()
        {
            //NOT OPCODED  0b00101111
            Registers.r8[7] = (byte)~Registers.r8[7];
            Registers.SetFlag("N", 1);
            Registers.SetFlag("H", 1);
        }

        public static void INCrr()
        {
            //increments the 16 bit register rr
            //NOT OPCODED 0b00xx0011
            Registers.setr16((ushort)(Registers.getr16(rrIndex) + 1), rrIndex);
        }

        public static void DECrr()
        {
            //decrements the 16 bit register rr
            //NOT OPCODED 0b00xx1011
            Registers.setr16((ushort)(Registers.getr16(rrIndex) - 1), rrIndex);
        }

        public static void ADDhlrr()
        {
            //adds HL to RR and stores it back in HL
            //NOT OPCODED 0b00xx1001
            int sum = (Registers.getr16(2) + Registers.getr16(rrIndex));
            //manually setting flags since i believe? its not needed anywhere else
            Registers.SetFlag("N", 0);
            if (sum > ushort.MaxValue || sum < 0)
            {
                Registers.SetFlag("C", 1);
            }
            else Registers.SetFlag("C", 0);
            int halfsum = (Registers.getr16(2) & 0x0FFF) + (Registers.getr16(rrIndex) & 0x0FFF);
            if (halfsum > 0x0FFF)
            {
                Registers.SetFlag("H", 1);
            }
            else Registers.SetFlag("H", 0);

            Registers.setr16((ushort)sum, 2);
        }

        public static void ADDspe()
        {
            //adds signed operand e to sp and stores it in sp
            //NOT OPCODED 11101000
            Registers.SetFlags8bAdd((byte)e, (byte)(Registers.getr16(3) & 0x00FF));
            Registers.setr16((ushort)(e + Registers.getr16(3)), 3);

            Registers.PC++;
        }



        public static void RLref(ref byte rot)
        {

            //rotates one bit to the left, into the carry flag and possibly beyond
            byte rotationvalue = rot;
            byte CryValue = (byte)Registers.ReadFlag("C");

            Registers.SetFlag("C", (0b10000000 & rotationvalue) >> 7);
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            rot = (byte)(rotationvalue << 1);
            rot |= CryValue;
            if (rot == 0)
                Registers.SetFlag("Z", 1);
            else Registers.SetFlag("Z", 0);


        }

        public static void RL()
        {
            if (yRegisterIndex == 6)
            {
                RLref(ref Memory.RAM[Registers.getr16(2)]);
            }
            else RLref(ref Registers.r8[yRegisterIndex]);
            if (yRegisterIndex == 7 && !CBprefixed)
            {
                Registers.SetFlag("Z", 0);
            }
        }

        public static void RLCref(ref byte rot)
        {
            //rotates one bit to the left, into the carry flag and possibly beyond
            byte rotationvalue = rot;



            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            rot = (byte)(rotationvalue << 1);
            Registers.SetFlag("C", (0b10000000 & rotationvalue) >> 7);
            rot |= (byte)Registers.ReadFlag("C");

            if (rot == 0)
                Registers.SetFlag("Z", 1);
            else Registers.SetFlag("Z", 0);
        }

        public static void RLC()
        {
            if (yRegisterIndex == 6)
            {
                RLCref(ref Memory.RAM[Registers.getr16(2)]);
            }
            else RLCref(ref Registers.r8[yRegisterIndex]);
            if (yRegisterIndex == 7 && !CBprefixed)
            {
                Registers.SetFlag("Z", 0);
            }
        }

        public static void RRref(ref byte rot)
        {

            //rotates one bit to the left, into the carry flag and possibly beyond
            byte rotationvalue = rot;
            byte CryValue = (byte)Registers.ReadFlag("C");

            Registers.SetFlag("C", (0b00000001 & rotationvalue));
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            rot = (byte)(rotationvalue >> 1);
            rot |= (byte)(CryValue << 7);
            if (rot == 0)
                Registers.SetFlag("Z", 1);
            else Registers.SetFlag("Z", 0);


        }

        public static void RR()
        {
            if (yRegisterIndex == 6)
            {
                RRref(ref Memory.RAM[Registers.getr16(2)]);
            }
            else RRref(ref Registers.r8[yRegisterIndex]);
            if (yRegisterIndex == 7 && !CBprefixed)
            {
                Registers.SetFlag("Z", 0);
            }
        }

        public static void RRCref(ref byte rot)
        {
            //rotates one bit to the left, into the carry flag and possibly beyond
            byte rotationvalue = rot;



            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            rot = (byte)(rotationvalue >> 1);
            Registers.SetFlag("C", (0b00000001 & rotationvalue));
            rot |= (byte)(Registers.ReadFlag("C") << 7);

            if (rot == 0)
                Registers.SetFlag("Z", 1);
            else Registers.SetFlag("Z", 0);
        }

        public static void RRC()
        {
            if (yRegisterIndex == 6)
            {
                RRCref(ref Memory.RAM[Registers.getr16(2)]);
            }
            else RRCref(ref Registers.r8[yRegisterIndex]);
            if (yRegisterIndex == 7 && !CBprefixed)
            {
                Registers.SetFlag("Z", 0);
            }
        }

        public static void JP()
        {
            Registers.PC += 2;
            Registers.PC = nn;
        }

        public static void JPhl()
        {
            Registers.PC = Registers.getr16(2);
        }

        public static void JRe()
        {
            Registers.PC++;
            Registers.PC = (ushort)(Registers.PC + e);
        }

        public static void DI()
        {
            Registers.IME = 0;
        }

        public static void JPccnn()
        {
            Registers.PC += 1;
            if (Registers.IfCondition(cc))
            {
                Registers.PC = (ushort)(Registers.PC + e);
            }
        }

        public static void RET()
        {
            Registers.PC = Memory.MemRead16b(Registers.getr16(3));
            Registers.setr16((ushort)(Registers.getr16(3) + 2), 3);
        }
        public static void RETi()
        {
            Registers.IME = 1;
            Registers.PC = Memory.MemRead16b(Registers.getr16(3));
            Registers.setr16((ushort)(Registers.getr16(3) + 2), 3);

        }
        public static void RETcc()
        {
            if (Registers.IfCondition(cc))
            {
                Registers.PC = Memory.MemRead16b(Registers.getr16(3));
                Registers.setr16((ushort)(Registers.getr16(3) + 2), 3);

            }

        }

        public static void JPccnnreal()
        {
            Registers.PC += 2;
            if (Registers.IfCondition(cc))
            {
                Registers.PC = nn;
            }
        }

        public static void CALLnn()
        {
            Registers.PC += 2;
            Registers.setr16((ushort)(Registers.getr16(3) - 2), 3);
            Memory.MemWrite16b(Registers.getr16(3), Registers.PC);
            Registers.PC = nn;
        }

        public static void CALLccnn()
        {
            Registers.PC += 2;
            if (Registers.IfCondition(cc))
            {
                Registers.setr16((ushort)(Registers.getr16(3) - 2), 3);
                Memory.MemWrite16b(Registers.getr16(3), Registers.PC);
                Registers.PC = nn;
            }
        }
        public static void RST()
        {

            Registers.setr16((ushort)(Registers.getr16(3) - 2), 3);
            Memory.MemWrite16b(Registers.getr16(3), Registers.PC);
            Registers.PC = (ushort)(xRegisterIndex * 8);
        }

        public static void EI()
        {
            Registers.IME = 1;
        }

        public static void SWAPr()
        {

            Registers.SetFlag("C", 0);
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            if (yRegisterIndex == 6)
            {
                if (Memory.MemRead(Registers.getr16(2)) == 0)
                {
                    Registers.SetFlag("Z", 1);
                }
                else Registers.SetFlag("Z", 0);
                Memory.MemWrite(Registers.getr16(2), (byte)(((Memory.MemRead(Registers.getr16(2)) & 0x0F) << 4) + ((Memory.MemRead(Registers.getr16(2)) & 0xF0) >> 4)));
                return;
            }
            if (Registers.r8[yRegisterIndex] == 0)
            {
                Registers.SetFlag("Z", 1);
            }
            else Registers.SetFlag("Z", 0);

            Registers.r8[yRegisterIndex] = (byte)(((Registers.r8[yRegisterIndex] & 0x0F) << 4) + ((Registers.r8[yRegisterIndex] & 0xF0) >> 4));
        }

        public static void SRL()
        {
            Registers.SetFlag("C", 0);
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            byte preshift;

            if (yRegisterIndex != 6)
            {
                preshift = Registers.r8[yRegisterIndex];

                Registers.r8[yRegisterIndex] = (byte)(Registers.r8[yRegisterIndex] >> 1);

            }
            else
            {
                preshift = Memory.MemRead(Registers.getr16(2));

                Memory.MemWrite(Registers.getr16(2), (byte)(Memory.MemRead(Registers.getr16(2)) >> 1));
            }
            if (1 == (1 & preshift))
            {
                Registers.SetFlag("C", 1);

            }
            if ((preshift >> 1) == 0)
            {
                Registers.SetFlag("Z", 1);
            }
            else Registers.SetFlag("Z", 0);
        }

        public static void SLA()
        {

            Registers.SetFlag("C", 0);
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            byte preshift;

            if (yRegisterIndex != 6)
            {
                preshift = Registers.r8[yRegisterIndex];

                Registers.r8[yRegisterIndex] = (byte)(Registers.r8[yRegisterIndex] << 1);

            }
            else
            {
                preshift = Memory.MemRead(Registers.getr16(2));

                Memory.MemWrite(Registers.getr16(2), (byte)(Memory.MemRead(Registers.getr16(2)) << 1));
            }
            if (1 == ((0b10000000 & preshift) >> 7))
            {
                Registers.SetFlag("C", 1);

            }
            if ((preshift << 1) == 0)
            {
                Registers.SetFlag("Z", 1);
            }
            else Registers.SetFlag("Z", 0);

        }

        public static void SRA()
        {
            Registers.SetFlag("C", 0);
            Registers.SetFlag("N", 0);
            Registers.SetFlag("H", 0);
            byte preshift;

            if (yRegisterIndex != 6)
            {
                preshift = Registers.r8[yRegisterIndex];

                Registers.r8[yRegisterIndex] = (byte)(Registers.r8[yRegisterIndex] >> 1);
                Registers.r8[yRegisterIndex] = (byte)(Registers.r8[yRegisterIndex] | ((Registers.r8[yRegisterIndex] & 0b01000000) << 1));

            }
            else
            {
                preshift = Memory.MemRead(Registers.getr16(2));

                Memory.MemWrite(Registers.getr16(2), (byte)(Memory.MemRead(Registers.getr16(2)) >> 1));
                Memory.MemWrite(Registers.getr16(2), (byte)(Memory.MemRead(Registers.getr16(2)) | ((Memory.MemRead(Registers.getr16(2)) & 0b01000000) << 1)));
            }
            if (1 == (1 & preshift))
            {
                Registers.SetFlag("C", 1);

            }
            if ((preshift >> 1) == 0)
            {
                Registers.SetFlag("Z", 1);
            }
            else Registers.SetFlag("Z", 0);
        }








    }
}
