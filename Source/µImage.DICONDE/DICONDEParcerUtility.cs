using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace µ.DICONDE
{
	// The DICONDEParserUtility provides easy access of the DICONDE attributes via LINQ queries.

	class DICONDEParserUtility
	{
		public static string GetDICONDEAttributeAsString(XDocument theXDocument, string theAttribute)
		{
			if (theXDocument == null) return "???";
			if (theAttribute == null) return "???";

			var aQuery = from Element in theXDocument.Descendants("DataElement")
						 where Element.Attribute("Tag").Value.Equals(theAttribute)
						 select Element;

			if (aQuery.Count() > 0) return aQuery.Last().Attribute("Data").Value.Trim();
			return "???";
		}

		public static bool DoesDICONDEAttributeExist(XDocument theXDocument, string theAttribute)
		{
			if (theXDocument == null) return false;
			if (theAttribute == null) return false;

			var aQuery = from Element in theXDocument.Descendants("DataElement")
						 where Element.Attribute("Tag").Value.Equals(theAttribute)
						 select Element;

			return (aQuery.Count() > 0);
		}

		public static Int32 GetDICONDEAttributeAsInt(XDocument theXDocument, string theAttribute)
		{
			if (theXDocument == null) return -1;
			if (theAttribute == null) return -1;

			var aQuery = from Element in theXDocument.Descendants("DataElement").First().ElementsAfterSelf()
						 where Element.Attribute("Tag").Value.Equals(theAttribute)
						 select Element;

			if (aQuery.Count() > 0){
				string s = aQuery.Last().Attribute("Data").Value.Trim();
				string[] split = s.Split(new Char[] { '\\' });
				return Convert.ToInt32(split[split.Length - 1], CultureInfo.InvariantCulture);
			}
			return -1;
		}

		public static double GetDICONDEAttributeAsDouble(XDocument theXDocument, string theAttribute)
		{
			if (theXDocument == null) return -1;

			if (theAttribute == null) return -1;

			var aQuery = from Element in theXDocument.Descendants("DataElement").First().ElementsAfterSelf()
						 where Element.Attribute("Tag").Value.Equals(theAttribute)
						 select Element;

			if (aQuery.Count() > 0){
				string s = aQuery.Last().Attribute("Data").Value.Trim();
				string[] split = s.Split(new Char[] { '\\' });
				return Convert.ToDouble(split[split.Length - 1], CultureInfo.InvariantCulture);
			}
			return -1;
		}
	}
}
