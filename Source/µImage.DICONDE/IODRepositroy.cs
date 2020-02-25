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

		public List<string> GetPatients()
		{
			var aPatientQuery = (from IODElement in myIODRepository
								 orderby IODElement.PatientName ascending
								 select IODElement.PatientName).Distinct();

			return aPatientQuery.ToList();
		}

		public List<string> GetSOPClassNames(string thePatientName)
		{
			var aSOPClassQuery = (from IODElement in myIODRepository
								  where IODElement.PatientName.Equals(thePatientName)
								  orderby IODElement.SOPClassName ascending
								  select IODElement.SOPClassName).Distinct();

			return aSOPClassQuery.ToList();
		}

		public List<string> GetStudies(string thePatientName, string theSOPClassName)
		{
			var aStudyQuery = (from IODElement in myIODRepository
							   where IODElement.PatientName.Equals(thePatientName)
							   where IODElement.SOPClassName.Equals(theSOPClassName)
							   orderby IODElement.StudyInstanceUID ascending
							   select IODElement.StudyInstanceUID).Distinct();

			return aStudyQuery.ToList();
		}

		public List<string> GetSeries(string thePatientName, string theSOPClassName, string theStudyInstanceUID)
		{
			var aSeriesQuery = (from IODElement in myIODRepository
								where IODElement.PatientName.Equals(thePatientName)
								where IODElement.SOPClassName.Equals(theSOPClassName)
								where IODElement.StudyInstanceUID.Equals(theStudyInstanceUID)
								orderby IODElement.SeriesInstanceUID ascending
								select IODElement.SeriesInstanceUID).Distinct();

			return aSeriesQuery.ToList();
		}

		public List<IOD> GetIODs(string thePatientName, string theSOPClassName, string theStudyInstanceUID, string theSeriesInstanceUID)
		{
			var aIODQuery = from IODElement in myIODRepository
							where IODElement.PatientName.Equals(thePatientName)
							where IODElement.SOPClassName.Equals(theSOPClassName)
							where IODElement.StudyInstanceUID.Equals(theStudyInstanceUID)
							where IODElement.SeriesInstanceUID.Equals(theSeriesInstanceUID)
							orderby IODElement.SortOrder ascending
							select IODElement;

			return aIODQuery.ToList();
		}
	}
}
