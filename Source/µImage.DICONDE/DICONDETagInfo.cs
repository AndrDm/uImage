namespace µ.DICONDE
{
	class DICONDETagInfo
	{        
		public DICONDETagInfo(string theTag, string theTagName, string theVR)
		{
			Tag = theTag;
			TagName = theTagName;
			VR = theVR;
		}

		public string Tag { get; set; }
		public string TagName { get; set; }
		public string VR { get; set; }
	}
}
