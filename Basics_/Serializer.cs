
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Basics
{
	public static class Serializer
	{
		public static void Serialize<T>(T obj, TextWriter writer,
			bool? indent = null, bool? omitXmlDeclaration = null, Dictionary<string, string> namespaces = null)
		{
			XmlWriterSettings settings = new XmlWriterSettings { Indent = indent ?? true };
			if (omitXmlDeclaration.HasValue)
			{ settings.OmitXmlDeclaration = omitXmlDeclaration.Value; }

			using (XmlWriter xw = XmlWriter.Create(writer, settings))
			{
				XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
				if (namespaces != null)
				{
					foreach (var pair in namespaces)
					{
						ns.Add(pair.Key, pair.Value);
					}
				}
				else
				{
					ns.Add(string.Empty, string.Empty);
				}
				XmlSerializer ser = new XmlSerializer(typeof(T));
				ser.Serialize(xw, obj, ns);
			}
		}

		public static bool TryDeserialize<T>(out T obj, TextReader reader)
		{
			try
			{
				Deserialize(out obj, reader);
				return true;
			}
			catch (System.Exception e)
			{
				obj = default(T);
				System.Diagnostics.Trace.Write(e);
				return false;
			}
		}
		public static void Deserialize<T>(out T obj, TextReader reader)
		{
			XmlSerializer ser = new XmlSerializer(typeof(T));

			object o = ser.Deserialize(reader);
			obj = (T)o;
		}
	}
}
