using System;
using System.IO;
using System.Linq;
using µ.Vision;

namespace µ.DICONDE
{
	public class µDICONDE
	{


		public µDICONDE(){}

		public static void µReadDICONDEFile(µImage destination, string fileName)
		{
			string aFileName = fileName;

			var mIOD = (new IOD(aFileName));
			
			int mColumnCount = DICONDEParserUtility.GetDICONDEAttributeAsInt(mIOD.XDocument, "(0028,0011)"); //width
			int mRowCount = DICONDEParserUtility.GetDICONDEAttributeAsInt(mIOD.XDocument, "(0028,0010)"); //height

			bool aPixelPaddingValueExist = DICONDEParserUtility.DoesDICONDEAttributeExist(mIOD.XDocument, "(0028,0120)");
			int aPixelPaddingValue = DICONDEParserUtility.GetDICONDEAttributeAsInt(mIOD.XDocument, "(0028,0120)");

			int aPixelBitsAllocated = DICONDEParserUtility.GetDICONDEAttributeAsInt(mIOD.XDocument, "(0028,0100)");

			ushort aPixelPaddingValue16;
			aPixelPaddingValue16 = 0;

			int aWindowCenter = DICONDEParserUtility.GetDICONDEAttributeAsInt(mIOD.XDocument, "(0028,1050)");
			int aWindowWidth = DICONDEParserUtility.GetDICONDEAttributeAsInt(mIOD.XDocument, "(0028,1051)");

			var aPixelDataQuery = from Element in mIOD.XDocument.Descendants("DataElement")
								  where Element.Attribute("Tag").Value.Equals("(7FE0,0010)")
								  select Element;

			// Get the start position of the stream for the pixel data attribute 
			long aStreamPosition = Convert.ToInt64(aPixelDataQuery.Last().Attribute("StreamPosition").Value);

			BinaryReader aBinaryReader = new BinaryReader(File.Open(aFileName, FileMode.Open, FileAccess.Read, FileShare.Read));
				
			// Set the stream position of the binary reader to first pixel
			aBinaryReader.BaseStream.Position = aStreamPosition;

			OpenCvSharp.Mat image = null;

			var size = new OpenCvSharp.Size(); 
			size.Width = mColumnCount;
			size.Height = mRowCount;

			long length = aBinaryReader.BaseStream.Length; //! Important to get length outside of the cycle, otherwise too sloooow

			if (16==aPixelBitsAllocated){
				image = new OpenCvSharp.Mat(size, OpenCvSharp.MatType.CV_16U);
				var mat16 = new OpenCvSharp.Mat<ushort>(image);
				var indexer = mat16.GetIndexer();

				for (int aRowIndex = 0; aRowIndex < mRowCount; aRowIndex++){
					for (int aColumnIndex = 0; aColumnIndex < mColumnCount; aColumnIndex++){
						// For some images, the pixel buffer is smaller than '2Byte * RowCount * ColumnCount'
						// That's why we need the check...
						if(aBinaryReader.BaseStream.Position - 2 < length){
							byte aByte0 = aBinaryReader.ReadByte();
							byte aByte1 = aBinaryReader.ReadByte();
							ushort aPixelValue = Convert.ToUInt16((aByte1 << 8) + aByte0);
							// Check for Pixel Padding Value  
							if ((aPixelPaddingValueExist) && (aPixelValue == aPixelPaddingValue16)) aPixelValue = UInt16.MinValue;
							indexer[aRowIndex, aColumnIndex] = aPixelValue;
							// Rescale handling
							//aPixelValue = aPixelValue * aRescaleSlope + aRescaleIntercept;
							// Value of the voxel is stored in Hounsfield Units
							//mHounsfieldPixelBuffer[aRowIndex, aColumnIndex] = aPixelValue;
						}
					}
				}
				OpenCvSharp.Cv2.CopyTo(mat16, destination._image);
			} 
			else{
				image = new OpenCvSharp.Mat(size, OpenCvSharp.MatType.CV_8U);
				var mat8 = new OpenCvSharp.Mat<byte>(image);
				var indexer = mat8.GetIndexer();
				for (int aRowIndex = 0; aRowIndex < mRowCount; aRowIndex++){
					for (int aColumnIndex = 0; aColumnIndex < mColumnCount; aColumnIndex++){
						if(aBinaryReader.BaseStream.Position - 2 < length){
							byte aPixelValue = aBinaryReader.ReadByte();
							if ((aPixelPaddingValueExist) && (aPixelValue == aPixelPaddingValue16)) aPixelValue = 0;
							indexer[aRowIndex, aColumnIndex] = aPixelValue;
						}
					}
				}
				OpenCvSharp.Cv2.CopyTo(mat8, destination._image);
			} 
		}
	}
}
