using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace مكتبة_الوقفية.Properties
{
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static مكتبة_الوقفية.Properties.Settings defaultInstance;

		public static مكتبة_الوقفية.Properties.Settings Default
		{
			get
			{
				return مكتبة_الوقفية.Properties.Settings.defaultInstance;
			}
		}

		static Settings()
		{
			مكتبة_الوقفية.Properties.Settings.defaultInstance = (مكتبة_الوقفية.Properties.Settings)SettingsBase.Synchronized(new مكتبة_الوقفية.Properties.Settings());
		}

		public Settings()
		{
		}
	}
}