using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace µ.DICONDE
{
	enum VREncodingMode { Undefined, ExplicitVR, ImplicitVR };
	enum EndianEncodingMode { Undefined, LittleEndian, BigEndian };

	public class DICONDEParser
	{
		private string mFileName = null;
		private BinaryReader mBinaryReader = null;
		private string mTransferSyntax = "???";
		private bool mFileMetaInfoReadingOngoingFlag = true;

		public DICONDEParser(string theFileName)
		{
			mFileName = theFileName;
		}

		// The method converts the binary encoded DICONDE file into a XDocument tree.
		// The XDocument can be queried afterwars via LINQ to access the detailed information for individual DICONDE attributes.
		//
		// In order to understand the implementation details, please refer to 'DCIOM Standard 2009, Part 5: Data Structures and Encoding'

		public XDocument GetXDocument()
		{
			mBinaryReader = new BinaryReader(File.Open(mFileName, FileMode.Open, FileAccess.Read, FileShare.Read));

			XDocument aXDocument = new XDocument(
				new XDeclaration("1.0", System.Text.Encoding.Default.EncodingName, "yes"),
				new XElement("DICONDEFile"));

			XElement aRootNode = new XElement("DataSet");
			aXDocument.Root.Add(aRootNode);

			// Check, if file is following the DICONDE standard (DICONDE Specification Part 10: Media Storage and File Format for Media Interchange)
			//
			// In this case, the file will contain the following:
			//  a) A fixed file preamble (128 bytes) not to be used
			//  b) DICONDE Prefix (four bytes containing string "DICM")
			//  c) File Meta Information (sequence of File Meta Attributes)
			//     All File Meta Attributes are of type (0002,xxxx) and shall be encoded using the 'Explicit VR Little Endian' Transfer Syntax

			// Skip file preable (128 bytes)
			mBinaryReader.BaseStream.Position = 128;

			// Read in the "DICM" string
			byte[] aDICMString = new byte[4];
			mBinaryReader.Read(aDICMString, 0, 4);
			Encoding myStringEncoder = System.Text.Encoding.Default;
			string DICMString = myStringEncoder.GetString(aDICMString).Trim();

			if (DICMString.Equals("DICM")){
				// DICONDE file, start parsing after preamble (128 byte) and DICM string (4 bytes)
				mBinaryReader.BaseStream.Position = 128 + 4;
			}
			else{
				// No standard DICONDE file, set offset to '0'
				mBinaryReader.BaseStream.Position = 0;
			}

			ParseDataSet(mBinaryReader.BaseStream.Length, aRootNode);

			mBinaryReader.Close();
			
			return aXDocument;
		}

		private void ParseDataSet(long StreamPositionEnd, XElement theParentNode)
		{
			// Each Sequence does have it's own 'Private Code Dictionary'
			PrivateCodeDictionary aPrivateCodeDictionary = new PrivateCodeDictionary();

			while (mBinaryReader.BaseStream.Position < StreamPositionEnd){
				DICONDEDataElement aTag = GetNextTag(aPrivateCodeDictionary);
				if (aTag != null){
					// Check for 'Transfer Syntax UID (0002,0010)'
					// ===========================================
					if (aTag.Tag.Equals("(0002,0010)")) mTransferSyntax = aTag.ValueField;

					// Private Creator Code Handling
					// =============================
					// Reset Private Creator Code List if current Tag is 'Item'
					if (aTag.Tag.Equals("(FFFE,E000)")) aPrivateCodeDictionary.ClearPrivateCreatorCode();

					// Add new Private Creator Code
					if ((aTag.GroupNumber % 2) == 1 && (aTag.ElementNumber <= 0xFF)){
						string aGroupNumberString = aTag.GroupNumber.ToString("X4");
						string aElementNumberString = aTag.ElementNumber.ToString("X2");
						aPrivateCodeDictionary.LoadPrivateCreatorCode(aGroupNumberString, aElementNumberString, aTag.ValueField.Trim());
					}

					// Add new Tag to XDocument
					// ========================
					XElement newXDataElement = new XElement("DataElement");
					theParentNode.Add(newXDataElement);

					newXDataElement.Add(new XAttribute("Tag", aTag.Tag));
					newXDataElement.Add(new XAttribute("TagName", aTag.TagName));
					newXDataElement.Add(new XAttribute("VR", aTag.VR));
					newXDataElement.Add(new XAttribute("VM", aTag.VM));
					newXDataElement.Add(new XAttribute("Data", aTag.ValueField));
					newXDataElement.Add(new XAttribute("Length", aTag.ValueLength.ToString()));
					newXDataElement.Add(new XAttribute("StreamPosition", aTag.StreamPosition.ToString()));

					// Call 'ParseDataSet' recursively for nested datasets
					// ===================================================
					if (aTag.VR.Equals("SQ")){
						if (aTag.ValueLength == -1) ParseDataSet(mBinaryReader.BaseStream.Length, newXDataElement);
						else ParseDataSet(mBinaryReader.BaseStream.Position + aTag.ValueLength, newXDataElement);
					}

					// End criteria for sequence reached?
					// ==================================
					// Break if current Tag is 'Sequence Delimitation Item'
					if (aTag.Tag.Equals("(FFFE,E0DD)")) break;
				}
			}
		}

		private DICONDEDataElement GetNextTag(PrivateCodeDictionary thePrivateCodeDictionary)
		{
			DICONDEDataElement aTag = new DICONDEDataElement();

			// Check, if we still read the 'File Meta Information' (attributes of type '(0002,xxxx)')
			// For this purpose, we do a look-up of the GroupNumber with the default 'Little Endian' encoding mode
			// If the GroupNumer is different to 0x0002, we have left the 'File Meta Information' section
			// Don't forget to adjust the Stream Position afterwards (decrement by '2')
			// Afterwars, the GroupNumber is read again (this time with the correct encoding mode)
			ushort aGroupNumberLookup = GetUnsignedShort_16Bit(EndianEncodingMode.LittleEndian);
			if (aGroupNumberLookup != 0x0002) mFileMetaInfoReadingOngoingFlag = false;

			mBinaryReader.BaseStream.Position -= 2;

			// Set the Encoding Mode for this tag
			// ==================================
			// All 'File Meta Information Attributes (0002,xxxx)' shall be encoded as 'VR explicit, little endian'
			// All other Attributes have to be encoded according to the transfer syntax value
			VREncodingMode aTagVREncodingMode = VREncodingMode.Undefined;
			EndianEncodingMode aTagEndianEncodingMode = EndianEncodingMode.Undefined;

			if (mFileMetaInfoReadingOngoingFlag){
				aTagVREncodingMode = VREncodingMode.ExplicitVR;
				aTagEndianEncodingMode = EndianEncodingMode.LittleEndian;
			}
			else{
				switch (mTransferSyntax){
					// Explicit VR Encoding, little endian
					case "1.2.840.10008.1.2.1":
						aTagVREncodingMode = VREncodingMode.ExplicitVR;
						aTagEndianEncodingMode = EndianEncodingMode.LittleEndian;
						break;

					// Implicit VR Encoding, little endian
					case "1.2.840.10008.1.2":
						aTagVREncodingMode = VREncodingMode.ImplicitVR;
						aTagEndianEncodingMode = EndianEncodingMode.LittleEndian;
						break;

					// Explicit VR Encoding, big endian
					case "1.2.840.10008.1.2.2":
						aTagVREncodingMode = VREncodingMode.ExplicitVR;
						aTagEndianEncodingMode = EndianEncodingMode.BigEndian;
						break;

					// For every other Transfer Syntax (e.g. JPEG encoding), 'Exlicit VR, little endian' shall be used
					default:
						aTagVREncodingMode = VREncodingMode.ExplicitVR;
						aTagEndianEncodingMode = EndianEncodingMode.LittleEndian;
						break;
				}
			}

			// Read GroupNumber
			aTag.GroupNumber = GetUnsignedShort_16Bit(aTagEndianEncodingMode);

			// Read ElementNumber
			aTag.ElementNumber = GetUnsignedShort_16Bit(aTagEndianEncodingMode);

			// Format the Tag string
			aTag.Tag = string.Format("({0},{1})", aTag.GroupNumber.ToString("X4"), aTag.ElementNumber.ToString("X4"));

			// Get VR value
			aTag.VR = GetVR(aTag.GroupNumber, aTag.ElementNumber, aTag.Tag, thePrivateCodeDictionary, aTagVREncodingMode);

			// Get DICONDE attribute name
			aTag.TagName = GetTagName(aTag.GroupNumber, aTag.ElementNumber, aTag.Tag, thePrivateCodeDictionary);
			
			switch (aTag.VR){
				case "OB":  // Other Byte String
				case "OF":  // Other Float String
				case "OW":  // Other Word String
				case "UN":  // Unknown content
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-1 
						//  -------------------------------------
						// | Reserved | ValueLength | ValueField |
						//  -------------------------------------
						// | 2 Bytes  | 4 Bytes     | n Bytes    |
						//  -------------------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR){
							// Skip the two Reserved bytes
							byte ReservedByte0 = mBinaryReader.ReadByte();
							byte ReservedByte1 = mBinaryReader.ReadByte();
						}

						aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);
						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						// Parse out the data
						if (aTag.ValueLength > 10){
							for (int i = 0; i < 10; i++){
								byte b = mBinaryReader.ReadByte();
								aTag.ValueField += string.Format("{0} ", b.ToString("X2"));
							}
							aTag.ValueField += "....";
							mBinaryReader.BaseStream.Position += aTag.ValueLength - 10;
						}
						else{
							for (int i = 0; i < aTag.ValueLength; i++){
								byte b = mBinaryReader.ReadByte();
								aTag.ValueField += string.Format("{0} ", b.ToString("X2"));
							}
						}

						aTag.ValueField = aTag.ValueField.Trim();
						aTag.VM = 1;

						break;
					}

				case "UT":  // Unlimited Text
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-1 
						//  -------------------------------------
						// | Reserved | ValueLength | ValueField |
						//  -------------------------------------
						// | 2 Bytes  | 4 Bytes     | n Bytes    |
						//  -------------------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR){ // Skip the two Reserved bytes
							byte ReservedByte0 = mBinaryReader.ReadByte();
							byte ReservedByte1 = mBinaryReader.ReadByte();
						}

						aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);
						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						// Parse out the Value string using the Default Encoder
						byte[] aBuffer = new byte[aTag.ValueLength];
						mBinaryReader.Read(aBuffer, 0, (int)aTag.ValueLength);
						
						Encoding myStringEncoder = System.Text.Encoding.Default;
						aTag.ValueField = myStringEncoder.GetString(aBuffer).Trim();

						aTag.ValueField = aTag.ValueField.Replace(Environment.NewLine, " ");
						aTag.ValueField = aTag.ValueField.Replace(Convert.ToChar(0x00).ToString(), "");

						aTag.VM = 1;

						break;
					}

				case "SQ":  // Sequence of Items 
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-1 
						//  ------------------------
						// | Reserved | ValueLength |
						// | 2 Bytes  | 4 Bytes     |
						//  ------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR){// Skip the two Reserved bytes
							byte ReservedByte0 = mBinaryReader.ReadByte();
							byte ReservedByte1 = mBinaryReader.ReadByte();
						}

						aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);
						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						aTag.VM = 1;

						break;
					}

				case "AE":  // Application Entity
				case "AS":  // Age String
				case "CS":  // Code String
				case "DA":  // Date
				case "DT":  // Date Time
				case "LT":  // Long Text
				case "PN":  // Person Name
				case "SH":  // Short String
				case "ST":  // Short Text
				case "TM":  // Time
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  --------------------------
						// | ValueLength | ValueField |
						//  --------------------------
						// | 2 Bytes     | n Bytes    |
						//  --------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;
						
						// Parse out the Value string using the Default Encoder
						byte[] aBuffer = new byte[aTag.ValueLength];
						mBinaryReader.Read(aBuffer, 0, (int)aTag.ValueLength);

						Encoding myStringEncoder = System.Text.Encoding.Default;
						aTag.ValueField = myStringEncoder.GetString(aBuffer).Trim();

						aTag.ValueField = aTag.ValueField.Replace(Environment.NewLine, " ");
						aTag.ValueField = aTag.ValueField.Replace(Convert.ToChar(0x00).ToString(), "");

						aTag.VM = 1;

						break;
					}

				case "DS":  // Decimal String
				case "IS":  // Integer String
				case "LO":  // Long String
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  --------------------------
						// | ValueLength | ValueField |
						//  --------------------------
						// | 2 Bytes     | n Bytes    |
						//  --------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						// Parse out the Value string using the Default Encoder
						byte[] aBuffer = new byte[aTag.ValueLength];
						mBinaryReader.Read(aBuffer, 0, (int)aTag.ValueLength);

						Encoding myStringEncoder = System.Text.Encoding.Default;
						aTag.ValueField = myStringEncoder.GetString(aBuffer).Trim();

						aTag.ValueField = aTag.ValueField.Replace(Environment.NewLine, " ");
						aTag.ValueField = aTag.ValueField.Replace(Convert.ToChar(0x00).ToString(), "");

						string[] split = aTag.ValueField.Split(new Char[] { '\\' });

						aTag.VM = split.Count();

						break;
					}

				case "UI":  // Unique Identifier (UID)
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  --------------------------
						// | ValueLength | ValueField |
						//  --------------------------
						// | 2 Bytes     | n Bytes    |
						//  --------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						// Parse out the Value string using the Default Encoder
						byte[] aBuffer = new byte[aTag.ValueLength];
						mBinaryReader.Read(aBuffer, 0, (int)aTag.ValueLength);
						
						Encoding myStringEncoder = System.Text.Encoding.Default;
						aTag.ValueField = myStringEncoder.GetString(aBuffer).Trim();

						aTag.ValueField = aTag.ValueField.Replace(Environment.NewLine, " ");
						aTag.ValueField = aTag.ValueField.Replace(Convert.ToChar(0x00).ToString(), "");

						aTag.VM = 1;

						break;
					}

				case "AT":  // Attribute Tag
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  -----------------------------
						// | ValueLength | ValueField    |
						//  -----------------------------
						// | 2 Bytes     | 4 Bytes fixed |
						//  -----------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						ushort aGroupNumber = GetUnsignedShort_16Bit(aTagEndianEncodingMode);
						ushort aElementNumber = GetUnsignedShort_16Bit(aTagEndianEncodingMode);

						aTag.Tag = string.Format("({0}),({1})", aGroupNumber.ToString("X4"), aElementNumber.ToString("X4"));

						aTag.VM = 1;

						break;
					}

				case "UL":  // Unsigned Long (32 Bit, 4 Bytes)
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  ----------------------------------------
						// | ValueLength | ValueField               |
						//  ----------------------------------------
						// | 2 Bytes     | ValueLength x 4 Bytes    |
						//  ----------------------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						for (int i = 0; i < aTag.ValueLength; i += 4){
							ulong Value = GetUnsignedInt_32Bit(aTagEndianEncodingMode);
							aTag.ValueField += Value.ToString() + " ";
							aTag.VM++;
						}
						break;
					}

				case "US":  // Unsigned Short
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  -------------------------------------
						// | ValueLength | ValueField            |
						//  -------------------------------------
						// | 2 Bytes     | ValueLength x 2 Bytes |
						//  -------------------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						for (int i = 0; i < aTag.ValueLength; i += 2){
							ushort Value = GetUnsignedShort_16Bit(aTagEndianEncodingMode);
							aTag.ValueField += Value.ToString() + " ";
							aTag.VM++;
						}

						break;
					}

				case "SL":  // Signed long (32 Bit, 4 Bytes)
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  -------------------------------------
						// | ValueLength | ValueField            |
						//  -------------------------------------
						// | 2 Bytes     | ValueLength x 4 Bytes |
						//  -------------------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						for (int i = 0; i < aTag.ValueLength; i += 4){
							long Value = GetSignedInt_32Bit(aTagEndianEncodingMode);
							aTag.ValueField += Value.ToString() + " ";
							aTag.VM++;
						}
						break;
					}

				case "SS":  // Signed short (16 Bit, 2 Bytes)
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  -------------------------------------
						// | ValueLength | ValueField            |
						//  -------------------------------------
						// | 2 Bytes     | ValueLength x 2 Bytes |
						//  -------------------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						for (int i = 0; i < aTag.ValueLength; i += 2){
							short Value = GetSignedShort_16Bit(aTagEndianEncodingMode);
							aTag.ValueField += Value.ToString() + " ";
							aTag.VM++;
						}
						break;
					}

				case "FL":  // Floating Point Single (32 Bit, 4 Byte)
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  -------------------------------------
						// | ValueLength | ValueField            |
						//  -------------------------------------
						// | 2 Bytes     | ValueLength x 4 Bytes |
						//  -------------------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						for (int i = 0; i < aTag.ValueLength; i += 4){
							float Value = GetFloatingPointSingle_32Bit(aTagEndianEncodingMode);
							aTag.ValueField += Value.ToString() + " ";
							aTag.VM++;
						}
						break;
					}

				case "FD":  // Floating Point Double (64 Bit, 8 Byte)
					{
						// Reference: DCIOM Standard 2009, PS 3.5: Data Structures and Encoding
						// Table 7.1-2 
						//  -------------------------------------
						// | ValueLength | ValueField            |
						//  -------------------------------------
						// | 2 Bytes     | ValueLength x 8 Bytes |
						//  -------------------------------------

						if (aTagVREncodingMode == VREncodingMode.ExplicitVR) aTag.ValueLength = GetLength_16Bit(aTagEndianEncodingMode);
						else aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);

						aTag.StreamPosition = mBinaryReader.BaseStream.Position;

						for (int i = 0; i < aTag.ValueLength; i += 8){
							double Value = GetFloatingPointDouble_64Bit(aTagEndianEncodingMode);
							aTag.ValueField += Value.ToString() + " ";
							aTag.VM++;
						}
						break;
					}

				case "DL":  // Special SQ related Data Elements Items:
							//   - (FFFE,E000) Item
							//   - (FFFE,E00D) Item Delimitation Item
							//   - (FFFE,E0DD) Sequence Delimitation Item

					aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);
					aTag.StreamPosition = mBinaryReader.BaseStream.Position;
					aTag.VM = 1;
					break;
 
				default:

					aTag.VR = "UN";
					aTag.ValueLength = GetLength_32Bit(aTagEndianEncodingMode);
					aTag.StreamPosition = mBinaryReader.BaseStream.Position;

					aTag.ValueField = "???";
					aTag.VM = 1;
					mBinaryReader.BaseStream.Position += aTag.ValueLength;
					break;
			}

			return aTag;
		}

		private string GetVR(UInt16 theGroupNumber, UInt16 theElementNumber, string theTag, PrivateCodeDictionary thePrivateCodeDictionary, VREncodingMode theTagVREncodingMode)
		{
			// For Tag 'Item (FFFE,E000)' return "Delimination"
			if (theTag.Equals("(FFFE,E000)")) return "DL";
			
			// For Tag 'Item Delimitation Item (FFFE,E00D)' return "Delimination"
			if (theTag.Equals("(FFFE,E00D)")) return "DL";

			// For Tag 'Sequence Delimitation Item (FFFE,E0DD)' return "Delimination"
			if (theTag.Equals("(FFFE,E0DD)")) return "DL";

			if (theTagVREncodingMode == VREncodingMode.ExplicitVR){
				// Explicit VR Encoding (value representation type is contained in stream)
				// =======================================================================
				byte b4 = mBinaryReader.ReadByte();
				byte b5 = mBinaryReader.ReadByte();
				return string.Format("{0}{1}", (char)b4, (char)b5);
			}
			else{
				// Implicit VR Encoding (value representation type must be retrieved from dictionary)
				// ==================================================================================

				// For Tag 'Private Creator Code' return "Long String"
				if ((theGroupNumber % 2) == 1 && (theElementNumber <= 0xFF)) return "LO";

				// For 'Even Group Number' and 'Implicit VR Encoding', VR information has to come from public dictionary
				if ((theGroupNumber % 2) == 0) return PublicDICONDEDictionary.GetVR(theTag);

				// For 'Odd Group Number' and 'Implicit VR Encoding', VR information has to come from private dictionary
				if ((theGroupNumber % 2) == 1) return thePrivateCodeDictionary.GetVR(theTag);
			}

			// Should never be reached...
			return "UN";
		}

		private string GetTagName(UInt16 theGroupNumber, UInt16 theElementNumber, string theTag, PrivateCodeDictionary thePrivateCodeDictionary)
		{
			// Tag 'Item (FFFE,E000)'
			if (theTag.Equals("(FFFE,E000)")) return "Item";

			// Tag 'Item Delimitation Item (FFFE,E00D)'
			if (theTag.Equals("(FFFE,E00D)")) return "Item Delimitation Item";

			// Tag 'Sequence Delimitation Item (FFFE,E0DD)'
			if (theTag.Equals("(FFFE,E0DD)")) return "Sequence Delimitation Item";

			// For 'Even Group Number', DICONDE attribute name has to come from public dictionary
			if ((theGroupNumber % 2) == 0) return PublicDICONDEDictionary.GetTagName(theTag);

			// For Tag 'Private Creator Code' return "Private Creator"
			if ((theGroupNumber % 2) == 1 && (theElementNumber <= 0xFF)) return "Private Creator";

			// For 'Odd Group Number', DICONDE attribute name has to come from private dictionary
			if ((theGroupNumber % 2) == 1) return thePrivateCodeDictionary.GetTagName(theTag);

			// Should never be reached...
			return "???";
		}

		private UInt16 GetLength_16Bit(EndianEncodingMode theTagEndianEncodingMode)
		{
			byte[] aBuffer = new byte[2];
			mBinaryReader.Read(aBuffer, 0, 2);

			if (theTagEndianEncodingMode == EndianEncodingMode.BigEndian) aBuffer = aBuffer.Reverse().ToArray();
			
			return BitConverter.ToUInt16(aBuffer, 0);
		}

		private UInt32 GetLength_32Bit(EndianEncodingMode theTagEndianEncodingMode)
		{
			byte[] aBuffer = new byte[4];
			mBinaryReader.Read(aBuffer, 0, 4);

			if (theTagEndianEncodingMode == EndianEncodingMode.BigEndian) aBuffer = aBuffer.Reverse().ToArray();

			return BitConverter.ToUInt32(aBuffer, 0);
		}

		private UInt16 GetUnsignedShort_16Bit(EndianEncodingMode theTagEndianEncodingMode)
		{
			byte[] aBuffer = new byte[2];
			mBinaryReader.Read(aBuffer, 0, 2);

			if (theTagEndianEncodingMode == EndianEncodingMode.BigEndian) aBuffer = aBuffer.Reverse().ToArray();

			return BitConverter.ToUInt16(aBuffer, 0);
		}

		private Int16 GetSignedShort_16Bit(EndianEncodingMode theTagEndianEncodingMode)
		{
			byte[] aBuffer = new byte[2];
			mBinaryReader.Read(aBuffer, 0, 2);

			if (theTagEndianEncodingMode == EndianEncodingMode.BigEndian) aBuffer = aBuffer.Reverse().ToArray();

			return BitConverter.ToInt16(aBuffer, 0);
		}

		private UInt32 GetUnsignedInt_32Bit(EndianEncodingMode theTagEndianEncodingMode)
		{
			byte[] aBuffer = new byte[4];
			mBinaryReader.Read(aBuffer, 0, 4);

			if (theTagEndianEncodingMode == EndianEncodingMode.BigEndian) aBuffer = aBuffer.Reverse().ToArray();

			return BitConverter.ToUInt32(aBuffer, 0);
		}

		private Int32 GetSignedInt_32Bit(EndianEncodingMode theTagEndianEncodingMode)
		{
			byte[] aBuffer = new byte[4];
			mBinaryReader.Read(aBuffer, 0, 4);

			if (theTagEndianEncodingMode == EndianEncodingMode.BigEndian) aBuffer = aBuffer.Reverse().ToArray();
			
			return BitConverter.ToInt32(aBuffer, 0);
		}

		private float GetFloatingPointSingle_32Bit(EndianEncodingMode theTagEndianEncodingMode)
		{
			byte[] aBuffer = new byte[4];
			mBinaryReader.Read(aBuffer, 0, 4);

			if (theTagEndianEncodingMode == EndianEncodingMode.BigEndian) aBuffer = aBuffer.Reverse().ToArray();

			return (float)BitConverter.ToSingle(aBuffer, 0);
		}

		private double GetFloatingPointDouble_64Bit(EndianEncodingMode theTagEndianEncodingMode)
		{
			byte[] aBuffer = new byte[8];
			mBinaryReader.Read(aBuffer, 0, 8);

			if (theTagEndianEncodingMode == EndianEncodingMode.BigEndian) aBuffer = aBuffer.Reverse().ToArray();
			
			return (float)BitConverter.ToDouble(aBuffer, 0);
		}
	}

	public class DICONDEDataElement
	{
		public UInt16 GroupNumber { get; set; }
		public UInt16 ElementNumber { get; set; }
		public string Tag { get; set; }
		public string TagName { get; set; }
		public string VR { get; set; }
		public Int64  VM { get; set; }
		public Int64  ValueLength { get; set; }
		public string ValueField { get; set; }
		public Int64  StreamPosition { get; set; }

		public DICONDEDataElement()
		{
			GroupNumber = 0;
			ElementNumber = 0;
			Tag = "";
			TagName = "";
			VR = "";
			VM = 0;
			ValueLength = -1;
			ValueField = "";
			StreamPosition = 0;
		}
	}
}
