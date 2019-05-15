using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ThetaNetCore.Util
{
	class JsonUtil
	{
		/// <summary>
		/// Convert JSON Object to string.
		/// </summary>
		/// <param name="serializer"></param>
		/// <returns></returns>
		internal static string ToSring<T>(T dataToSend)
		{
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
			using (MemoryStream memStream = new MemoryStream())
			{
				serializer.WriteObject(memStream, dataToSend);
				byte[] bytes = memStream.ToArray();
				return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			}
		}

		/// <summary>
		/// Convert JSON string to object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strJson"></param>
		/// <returns></returns>
		internal static T ToObject<T>(String strJson) where T : class
		{
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
			MemoryStream ms = new MemoryStream();
			var bytes = Encoding.UTF8.GetBytes(strJson);
			ms.Write(bytes, 0, bytes.Length);
			ms.Position = 0;
			return serializer.ReadObject(ms) as T;
		}

		/// <summary>
		/// Convert JSON stream to object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strJson"></param>
		/// <returns></returns>
		internal static T ToObject<T>(Stream stream) where T : class
		{
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
			bool b = false;
			if (b)
			{
				var reader = new StreamReader(stream);
				var str = reader.ReadToEnd();
			}
			var res = serializer.ReadObject(stream) as T;
			stream.Dispose();
			return res;
		}
	}
}
