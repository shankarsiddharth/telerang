using System;

namespace telerang
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (var game = new TeleRangGame())
                game.Run();
        }
    }
}