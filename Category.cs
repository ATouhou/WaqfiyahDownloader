using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace مكتبة_الوقفية
{
	internal class Category
	{
		public readonly string Id;

		public List<Book> Books
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public List<string> Pages
		{
			get;
			private set;
		}

		public Category(string name, string id)
		{
			this.Id = id;
			List<string> strs = new List<string>()
			{
				string.Concat("http://www.waqfeya.com/category.php?cid=", id)
			};
			this.Pages = strs;
			this.Name = name;
			this.Books = new List<Book>();
		}
	}
}