using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace مكتبة_الوقفية
{
	[Serializable]
	internal class Settings
	{
        [Serializable]
        public class DownloadListItem
        {
            public string Book;
            public string Category;
            public string Author;
            public DateTime AddDate;
            public string Status;
            public string[] Urls;
            public long Id;
            public long Order;
        }

		private string saveFolder_backingField;

		private bool downloadRestriction;

		private int maxMegabytesDownload;

        private short numThreads;

        private List<DownloadListItem> downloadList;

        private long id;

        public long IdentityId
        {
            get { return id; }
            set { id = value; }
        }

		public bool DownloadRestriction
		{
			get
			{
				return this.downloadRestriction;
			}
			set
			{
				this.downloadRestriction = value;
			}
		}

		public int MaxMegaBytesDownload
		{
			get
			{
				return this.maxMegabytesDownload;
			}
			set
			{
				this.maxMegabytesDownload = value;
			}
		}

        public short NumThreads
        {
            get { return numThreads; }
            set { numThreads = value; }
        }

		public string SaveFolder
		{
			get
			{
				return this.saveFolder_backingField;
			}
			set
			{
				this.saveFolder_backingField = value;
			}
		}

        public List<DownloadListItem> DownloadList
        {
            get { return downloadList; }
        }

		public Settings(string folder)
            : this()
		{
			this.saveFolder_backingField = folder;
		}

		public static Settings Load()
		{
			Settings nullable;
			try
			{
				FileStream fileStream = new FileStream("الاعدادات", FileMode.Open);
				try
				{
					nullable = (Settings)(new BinaryFormatter()).Deserialize(fileStream);
                    if (nullable.downloadList == null) // old setting files
                        nullable.downloadList = new List<DownloadListItem>();
				}
				finally
				{
					if (fileStream != null)
					{
						((IDisposable)fileStream).Dispose();
					}
				}
			}
			catch
			{
				nullable = null;
			}
			return nullable;
		}

		public void Save()
		{
			FileStream fileStream = new FileStream("الاعدادات", FileMode.Create);
			try
			{
				(new BinaryFormatter()).Serialize(fileStream, this);
			}
			finally
			{
				if (fileStream != null)
				{
					((IDisposable)fileStream).Dispose();
				}
			}
		}

        public Settings()
        {
            this.downloadList = new List<DownloadListItem>();
            this.downloadRestriction = false;
            this.maxMegabytesDownload = 0;
            this.numThreads = 2;
            this.saveFolder_backingField = "";
        }
	}
}