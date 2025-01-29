using Newtonsoft.Json;
// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
using QuickType;
using EmuMemory;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.IO;
using Raylib_cs;

public static class Tests
{

    public static void RunTests()
    {
        //improvised testing, very ugly 11011001
        int total = 0;
        int passed = 0;
        foreach (string s in Directory.GetFiles("../../../../../Downloads/sm83-main/sm83-main/v1"))
        {

            string txt = "a";



            txt = File.ReadAllText(s);




            var test = Test.FromJson(txt);

            foreach (Test item in test)
            {
                total++;
                Registers.IME = (byte)item.Initial.Ime;
                Registers.r8[0] = (byte)item.Initial.B;
                Registers.r8[1] = (byte)item.Initial.C;
                Registers.r8[2] = (byte)item.Initial.D;
                Registers.r8[3] = (byte)item.Initial.E;
                Registers.r8[4] = (byte)item.Initial.H;
                Registers.r8[5] = (byte)item.Initial.L;
                Registers.Flags = (byte)item.Initial.F;
                Registers.r8[7] = (byte)item.Initial.A;
                Registers.PC = (ushort)item.Initial.Pc;
                Registers.setr16((ushort)item.Initial.Sp, 3);
                foreach (long[] written in item.Initial.Ram)
                {
                    Memory.RAM[written[0]] = (byte)written[1];

                }
                for (int j = 0; j < 1; j++)
                {
                    Program.Execute(Registers.PC);
                }
                int b = 6;
                if (Registers.IME == (byte)item.Final.Ime &&
                    Registers.r8[0] == (byte)item.Final.B &&
                    Registers.r8[1] == (byte)item.Final.C &&
                    Registers.r8[2] == (byte)item.Final.D &&
                    Registers.r8[3] == (byte)item.Final.E &&
                    Registers.r8[4] == (byte)item.Final.H &&
                    Registers.r8[5] == (byte)item.Final.L &&
                    Registers.getr16(3) == (ushort)item.Final.Sp &&
                    Registers.PC == (ushort)item.Final.Pc &&
                    Registers.Flags == (byte)item.Final.F &&
                    Registers.r8[7] == (byte)item.Final.A)
                {
                    int a = 5;
                    foreach (long[] written in item.Final.Ram)
                    {
                        if (Memory.RAM[written[0]] != (byte)written[1])
                        {
                            a = 2;
                        }

                    }
                    if (a == 5)
                        passed++;
                    else b = 5;
                }
                else b = 5;
                if (b == 5)
                {

                    //Console.WriteLine("F Difference: " + (Registers.Flags - (byte)item.Final.F));
                    //Console.WriteLine("nopüe");

                }



            }
            Console.WriteLine((float)passed / (float)total);
            Console.WriteLine("passed: " + passed);
            Console.WriteLine("total: " + total);
        }

    }
}