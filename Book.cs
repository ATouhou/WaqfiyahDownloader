using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;

namespace مكتبة_الوقفية
{
	internal class Book
	{
        private static int idCounter;
        private static List<Book> objs = new List<Book>();

        public readonly int ID;

        public string Author { get; set; }

		public string Title
		{
			get;
			private set;
		}

		public List<string> URLs
		{
			get;
			private set;
		}

		public Book(string title, List<string> urls)
		{
            ID = ++idCounter;
            objs.Add(this);
			this.URLs = urls;
			this.Title = title;
		}

        public static Book GetById(int id)
        {
            return objs.First(k => k.ID == id);
        }
	}
}