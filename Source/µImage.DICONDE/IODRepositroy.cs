using System.Collections.Generic;
using System.Linq;

namespace µ.DICONDE
{
	// The repository holds a list of all IOD's (one entry for each physical DICONDE file).
	// The repository provides helper methods in order to build up the IOD model.
	public class IODRepository
	{
		IList<IOD> myIODRepository;

		public IODRepository()
		{
			myIODRepository = new List<IOD>();
		}

		public void Add(IOD theIODInfo)
		{
			myIODRepository.Add(theIODInfo);
		}

		//DICONDE:Component
		public List<string> GetComponents()
		{
			var aComponentQuery = (from IODElement in myIODRepository
								 orderby IODElement.ComponentName ascending
								 select IODElement.ComponentName).Distinct();

			return aComponentQuery.ToList();
		}

		public List<string> GetSOPClassNames(string theComponentName)
		{
			var aSOPClassQuery = (from IODElement in myIODRepository
								  where IODElement.ComponentName.Equals(theComponentName)
								  orderby IODElement.SOPClassName ascending
								  select IODElement.SOPClassName).Distinct();

			return aSOPClassQuery.ToList();
		}

		public List<string> GetStudies(string theComponentName, string theSOPClassName)
		{
			var aStudyQuery = (from IODElement in myIODRepository
							   where IODElement.ComponentName.Equals(theComponentName)
							   where IODElement.SOPClassName.Equals(theSOPClassName)
							   orderby IODElement.StudyInstanceUID ascending
							   select IODElement.StudyInstanceUID).Distinct();

			return aStudyQuery.ToList();
		}

		public List<string> GetSeries(string theComponentName, string theSOPClassName, string theStudyInstanceUID)
		{
			var aSeriesQuery = (from IODElement in myIODRepository
								where IODElement.ComponentName.Equals(theComponentName)
								where IODElement.SOPClassName.Equals(theSOPClassName)
								where IODElement.StudyInstanceUID.Equals(theStudyInstanceUID)
								orderby IODElement.SeriesInstanceUID ascending
								select IODElement.SeriesInstanceUID).Distinct();

			return aSeriesQuery.ToList();
		}

		public List<IOD> GetIODs(string theComponentName, string theSOPClassName, string theStudyInstanceUID, string theSeriesInstanceUID)
		{
			var aIODQuery = from IODElement in myIODRepository
							where IODElement.ComponentName.Equals(theComponentName)
							where IODElement.SOPClassName.Equals(theSOPClassName)
							where IODElement.StudyInstanceUID.Equals(theStudyInstanceUID)
							where IODElement.SeriesInstanceUID.Equals(theSeriesInstanceUID)
							orderby IODElement.SortOrder ascending
							select IODElement;

			return aIODQuery.ToList();
		}
	}
}
