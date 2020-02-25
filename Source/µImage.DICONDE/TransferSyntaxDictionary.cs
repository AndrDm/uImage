using System.Collections.Generic;

namespace µ.DICONDE
{
	public static class TransferSyntaxDictionary
	{
		// DICONDE Transfer Syntax Dictionary
		// Reference: DCIOM Standard 2009, Part 5: Data Structures and Encoding

		static private readonly Dictionary<string, string> d = new Dictionary<string, string>
		{
			{ "1.2.840.10008.1.2", "Implicit VR Little Endian: Default Transfer Syntax for DICONDE" },
			{ "1.2.840.10008.1.2.1", "Explicit VR Little Endian" },
			{ "1.2.840.10008.1.2.1.99", "Deflated Explicit VR Little Endian" },
			{ "1.2.840.10008.1.2.2", "Explicit VR Big Endian" },
			{ "1.2.840.10008.1.2.4.50", "JPEG Baseline (Process 1): Default Transfer Syntax for Lossy JPEG 8 Bit Image Compression" },
			{ "1.2.840.10008.1.2.4.51", "JPEG Extended (Process 2 & 4): Default Transfer Syntax for Lossy JPEG 12 Bit Image Compression (Process 4 only)" },
			{ "1.2.840.10008.1.2.4.52", "JPEG Extended (Process 3 & 5)" },
			{ "1.2.840.10008.1.2.4.53", "JPEG Spectral Selection, Non-Hierarchical (Process 6 & 8)" }, 
			{ "1.2.840.10008.1.2.4.54", "JPEG Spectral Selection, Non-Hierarchical (Process 7 & 9)" },
			{ "1.2.840.10008.1.2.4.55", "JPEG Full Progression, Non-Hierarchical (Process 10 & 12)" },
			{ "1.2.840.10008.1.2.4.56", "JPEG Full Progression, Non-Hierarchical (Process 11 & 13)" },
			{ "1.2.840.10008.1.2.4.57", "JPEG Lossless, Non-Hierarchical (Process 14)" }, 
			{ "1.2.840.10008.1.2.4.58", "JPEG Lossless, Non-Hierarchical (Process 15)" },
			{ "1.2.840.10008.1.2.4.59", "JPEG Extended, Hierarchical (Process 16 & 18)" },
			{ "1.2.840.10008.1.2.4.60", "JPEG Extended, Hierarchical (Process 17 & 19)" },
			{ "1.2.840.10008.1.2.4.61", "JPEG Spectral Selection, Hierarchical (Process 20 & 22)" },
			{ "1.2.840.10008.1.2.4.62", "JPEG Spectral Selection, Hierarchical (Process 21 & 23)" },
			{ "1.2.840.10008.1.2.4.63", "JPEG Full Progression, Hierarchical (Process 24 & 26)" }, 
			{ "1.2.840.10008.1.2.4.64", "JPEG Full Progression, Hierarchical (Process 25 & 27)" },
			{ "1.2.840.10008.1.2.4.65", "JPEG Lossless, Hierarchical (Process 28)" },
			{ "1.2.840.10008.1.2.4.66", "JPEG Lossless, Hierarchical (Process 29)" },
			{ "1.2.840.10008.1.2.4.70", "JPEG Lossless, Non-Hierarchical, First-Order Prediction (Process 14 [Selection Value 1]): Default Transfer Syntax for Lossless JPEG Image Compression" }, 
			{ "1.2.840.10008.1.2.4.80", "JPEG-LS Lossless Image Compression" },
			{ "1.2.840.10008.1.2.4.81", "JPEG-LS Lossy (Near-Lossless) Image Compression" },
			{ "1.2.840.10008.1.2.4.90", "JPEG 2000 Image Compression (Lossless Only)" },
			{ "1.2.840.10008.1.2.4.91", "JPEG 2000 Image Compression" },
			{ "1.2.840.10008.1.2.4.92", "JPEG 2000 Part 2 Multi-component Image Compression (Lossless Only)" },
			{ "1.2.840.10008.1.2.4.93", "JPEG 2000 Part 2 Multi-component Image Compression" },
			{ "1.2.840.10008.1.2.4.94", "JPIP Referenced" },
			{ "1.2.840.10008.1.2.4.95", "JPIP Referenced Deflate" },
			{ "1.2.840.10008.1.2.4.100", "MPEG2 Main Profile @ Main Level" },
			{ "1.2.840.10008.1.2.5", "RLE Lossless" },
			{ "1.2.840.10008.1.2.6.1", "RFC 2557 MIME encapsulation" }
		};

		static public string GetTransferSyntaxName(string theTransferSyntaxUID)
		{
			return d.ContainsKey(theTransferSyntaxUID) ? d[theTransferSyntaxUID] : "???";
		}
	}
}