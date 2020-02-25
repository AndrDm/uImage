using System.Collections.Generic;
using System.Linq;

namespace µ.DICONDE
{
	class PrivateCodeDictionary
	{
		Dictionary<string, DICONDETagInfo> mPrivateCodeDictionary = new Dictionary<string, DICONDETagInfo>();

		public void LoadPrivateCreatorCode(string theGroupNumber, string theCodeValue, string thePrivateCreatorCode)
		{
			var aPrivateCodeQuery = from entry in PrivateDICONDEDictionary.GetPrivateDICONDEDictionary()
									where entry.PrivateCreatorCode.Equals(thePrivateCreatorCode)
									where entry.GroupNumber.Equals(theGroupNumber)
									select entry;

			foreach (PrivateDICONDETagInfo aPrivateCode in aPrivateCodeQuery){ // Tag is of format '(300B,1012)'
				string aTag = string.Format("({0},{1}{2})", aPrivateCode.GroupNumber, theCodeValue, aPrivateCode.ElementID);
				mPrivateCodeDictionary[aTag] = new DICONDETagInfo(aTag, aPrivateCode.Name, aPrivateCode.VR);
			}
		}

		public void ClearPrivateCreatorCode()
		{
			mPrivateCodeDictionary.Clear();
		}

		public string GetVR(string theTagString)
		{
			// If Tag is not known, return VR value 'UN' (Unknown). 
			return mPrivateCodeDictionary.ContainsKey(theTagString) ? mPrivateCodeDictionary[theTagString].VR : "UN";
		}

		public string GetTagName(string theTagString)
		{
			// If Tag is not know, return TagName 'Not in Dictionary'. 
			return mPrivateCodeDictionary.ContainsKey(theTagString) ? mPrivateCodeDictionary[theTagString].TagName : "Not in Dictionary";
		}
	}
}
