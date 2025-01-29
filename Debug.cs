using EmuMemory;
namespace Debug
{
    public static class Dumping
    {
        public static void DumpRange(ushort start, ushort end, string title)
        {
            //prints out the bytes in range in a readable format
            Console.WriteLine("\n-----START " + title + "-------");
            for (var i = 0; i < (end - start) + 1; i++)
            {


                //Console.Write(i.ToString("X5") + " ");
                if (i % 0x10 == 0)
                {
                    Console.WriteLine();
                    Console.Write((start + i).ToString("X5") + " ");
                }
                Console.Write(Memory.MemRead((ushort)(start + i)).ToString("X2") + " ");
            }

            Console.WriteLine("\n-------END " + title.ToUpper() + "--------");
        }
    }
}