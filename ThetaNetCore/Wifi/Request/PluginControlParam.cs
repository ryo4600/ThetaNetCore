using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	public enum PLUGIN_ACTION { START, STOP };

	[DataContract]
	public class PluginControlParam
	{
		public const string ACTION_START = "boot";
		public const string ACTION_STOP = "finish";

		[IgnoreDataMember]
		public PLUGIN_ACTION PluginAction { get; set; } = PLUGIN_ACTION.STOP;

		[DataMember(Name = "action")]
		public String Action {
			get
			{
				switch (PluginAction)
				{
					case PLUGIN_ACTION.START:
						return ACTION_START;
					case PLUGIN_ACTION.STOP:
						return ACTION_STOP;
					default:
						return "";
				}
			}
			set
			{
				switch(value)
				{
					case ACTION_START:
						PluginAction = PLUGIN_ACTION.START;
						break;
					case ACTION_STOP:
						PluginAction = PLUGIN_ACTION.STOP;
						break;
				}
			}
		}

		/// <summary>
		/// Z1 or later only <br />
		/// Target plugin package name <br />
		/// If no target is specified, then Plugin 1 will start. This parameter is ignored when action parameter is "finish".
		/// </summary>
		[DataMember(Name = "plugin")]
		public string Plugin { get; set; }
	}
}
