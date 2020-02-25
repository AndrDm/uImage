using System.Collections.Generic;

namespace µ.DICONDE
{
	// Dictionary for private DICONDE attributes.
	// Please note: Group Number must be odd for all private DICONDE attributes!

	public class PrivateDICONDEDictionary
	{
		static private readonly List<PrivateDICONDETagInfo> mPrivateDICONDEDictionary = new List<PrivateDICONDETagInfo>
		{
			// In case you want to support private DICONDE attribute names, the information has to be provided here.
			// Private attribute information has to be provided in form:
			// "Private Creator Code", "Group Number", "Element ID", "Attribute Name", "VR (Value Representation)"

			// Sample private DICONDE attribute:
			{ new PrivateDICONDETagInfo("YOUR PRIVATE CREATOR CODE", "300B", "ED", "My private DICONDE attribute", "OB") },
		};

		static public List<PrivateDICONDETagInfo> GetPrivateDICONDEDictionary()
		{
			return mPrivateDICONDEDictionary;
		}
	}

	public class PrivateDICONDETagInfo
	{
		public PrivateDICONDETagInfo(string thePrivateCreatorCode, string theGroupNumber, string theElementID, string theName, string theVR)
		{
			PrivateCreatorCode = thePrivateCreatorCode;
			GroupNumber = theGroupNumber;
			ElementID = theElementID;
			Name = theName;
			VR = theVR;
		}

		public string PrivateCreatorCode { get; set; }
		public string GroupNumber { get; set; }
		public string ElementID { get; set; }
		public string Name { get; set; }
		public string VR { get; set; }
	}
}
