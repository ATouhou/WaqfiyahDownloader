using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
	internal static class Program
	{
		private readonly static List<char> invalidPathChars;
        public static readonly Settings Setting;

        public static bool Updated { get; private set; }


		static Program()
		{
            Setting = Settings.Load() ?? new Settings();
			Program.invalidPathChars = Path.GetInvalidFileNameChars().ToList<char>();
		}

        public static void Exit()
        {
            Setting.Save(); 
            Application.ExitThread();
            Environment.Exit(0);
        }

		[STAThread]
		private static void Main(string[] args)
		{
            foreach (var item in args)
                if (item == "--updated")
                    Updated = true;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormWelcome());
		}

		public static string PreparePath(string path)
		{
			for (int i = path.Length - 1; i >= 0; i--)
			{
				if (Program.invalidPathChars.Contains(path[i]))
				{
					path = path.Remove(i, 1);
				}
			}
			return path;
		}
	}
}