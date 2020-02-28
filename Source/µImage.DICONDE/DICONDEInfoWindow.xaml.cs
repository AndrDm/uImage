using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Collections.Generic;
 
namespace µ.DICONDE
{
	public partial class DICONDEInfoWindow : Window
	{
		public DICONDEInfoWindow(string infoFileName)
		{
			InitializeComponent();

			List<IOD> aIODList = new List<IOD>();

			IODRepository mIODRepository = new IODRepository();
			mIODTree.Items.Clear();
				
			if (infoFileName == "") return;

			mIODRepository.Add(new IOD(infoFileName));
			//changed to component:
			foreach (string aComponentName in mIODRepository.GetComponents()){
				TreeViewItem aComponentItem = new TreeViewItem() { Header = aComponentName };
				this.mIODTree.Items.Add(aComponentItem);
				foreach (string aSOPClass in mIODRepository.GetSOPClassNames(aComponentName)){
					TreeViewItem aSOPClassItem = new TreeViewItem() { Header = aSOPClass };
					aComponentItem.Items.Add(aSOPClassItem);
					foreach (string aStudy in mIODRepository.GetStudies(aComponentName, aSOPClass)){
						TreeViewItem aStudyItem = new TreeViewItem() { Header = string.Format(@"Study: '{0}'", aStudy) };
						aSOPClassItem.Items.Add(aStudyItem);
						foreach (string aSeries in mIODRepository.GetSeries(aComponentName, aSOPClass, aStudy)){
							TreeViewItem aSeriesItem = new TreeViewItem() { Header = string.Format(@"Series: '{0}'", aSeries) };
							aStudyItem.Items.Add(aSeriesItem);
							foreach (IOD aIOD in mIODRepository.GetIODs(aComponentName, aSOPClass, aStudy, aSeries)){
									TreeViewItem anIOD = new TreeViewItem() { Header = string.Format(@"{0}", aIOD.SOPInstanceUID) };
									anIOD.Tag = aIOD;
									aSeriesItem.Items.Add(anIOD);
							}
						}
					}
				}
			}
		}
 
		private void Accept_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
 
 		private void mIODTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			TreeViewItem aSelectedNode = this.mIODTree.SelectedItem as TreeViewItem;
			if (aSelectedNode == null) return;

			// Clear old content
			this.mDICONDETagTree.Items.Clear();
			mGrid.RowDefinitions.First().Height = new GridLength(0);
			mGrid.RowDefinitions.Last().Height = new GridLength(0);

			IOD anIOD = aSelectedNode.Tag as IOD;
			if (anIOD == null) return;

			// Set the FileName as root node
			string[] split = anIOD.FileName.Split(new Char[] { '\\' });
			string aFileName = split[split.Length-1];

			TreeViewItem aRootNode = new TreeViewItem() { Header = string.Format("File: {0}", aFileName) };
			this.mDICONDETagTree.Items.Add(aRootNode);

			// Expand the root node
			aRootNode.IsExpanded = true;

			// Add all DICONDE attributes to the tree
			foreach (XElement xe in anIOD.XDocument.Descendants("DataSet").First().Elements("DataElement"))
				AddDICONDEAttributeToTree(aRootNode, xe);

			// In case the IOD does have a processable pixel data, the ImageFlow button, the volume buttons and the bitmap is shown.
			// Otherwise, only the DICONDE attributes are shown and the first and last grid row is hided.
		}

		 private void AddDICONDEAttributeToTree(TreeViewItem theParentNode, XElement theXElement)
		{
			string aTag = theXElement.Attribute("Tag").Value;
			string aTagName = theXElement.Attribute("TagName").Value;
			string aTagData = theXElement.Attribute("Data").Value;

			// Enrich the Transfer Syntax attribute (0002,0010) with human-readable string from dictionary
			if (aTag.Equals("(0002,0010)"))
				aTagData = string.Format("{0} ({1})", aTagData, TransferSyntaxDictionary.GetTransferSyntaxName(aTagData));

			// Enrich the SOP Class UID attribute (0008,0016) with human-readable string from dictionary
			if (aTag.Equals("(0008,0016)"))
				aTagData = string.Format("{0} ({1})", aTagData, SOPClassDictionary.GetSOPClassName(aTagData));

			string s = string.Format("{0} {1}", aTag, aTagName);

			// Do some cut-off in order to allign the TagData
			if (s.Length > 50) s = s.Remove(50);
			else s = s.PadRight(50);

			s = string.Format("{0} {1}", s, aTagData); 

			TreeViewItem aNewItem = new TreeViewItem() { Header = s };
			theParentNode.Items.Add(aNewItem);

			// In case the DICOM attributes has childrens (= Sequence), call the helper method recursively.
			if (theXElement.HasElements)
				foreach (XElement xe in theXElement.Elements("DataElement"))
					AddDICONDEAttributeToTree(aNewItem, xe); 
		}
	}
}