using System;
using System.Runtime.Serialization;

namespace ThetaNetCore.Wifi
{
	[DataContract]
	public class ListPluginsResponse
	{
		[DataMember(Name = "results")]
		public ListPluginResults Results;
	}

	[DataContract]
	public class ListPluginResults
	{
		[DataMember(Name = "plugins")]
		public Plugin[] Plugins { get; set; }
	}

	[DataContract]
	public class Plugin
	{
		[DataMember(Name = "pluginName")]
		public String PluginName { get; set; }

		[DataMember(Name = "packageName")]
		public String PackageName { get; set; }

		[DataMember(Name = "version")]
		public String Version { get; set; }

		[DataMember(Name = "type")]
		public String Type { get; set; }

		[DataMember(Name = "running")]
		public bool Running { get; set; }

		[DataMember(Name = "foreground")]
		public bool Foreground { get; set; }

		[DataMember(Name = "boot")]
		public bool Boot { get; set; }

		[DataMember(Name = "webServer")]
		public bool WebServer { get; set; }

		[DataMember(Name = "exitStatus")]
		public String ExitStatus { get; set; }

		[DataMember(Name = "message")]
		public String Message { get; set; }

	}
}
