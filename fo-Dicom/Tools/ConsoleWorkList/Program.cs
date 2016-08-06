using Dicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorkList
{
    class Program
    {
        static void Main(string[] args)
        {
            DicomDataset set = new DicomDataset();
            set.Add(DicomTag.SOPClassUID,DicomUID.SecondaryCaptureImageStorage);
            set.Add(DicomTag.SOPInstanceUID, "0");
            set.Add(DicomTag.ImplementationVersionName, "OFFIS_DCMTK_361");
            set.Add(DicomTag.SpecificCharacterSet, "");
            set.Add(DicomTag.PatientName, "HUAC");
            set.Add(DicomTag.PatientID, "123456");
            set.Add(DicomTag.PatientBirthDate, "20000101");
            set.Add(DicomTag.PatientSex, "1");

            DicomFile file = new DicomFile(set);
            file.Save(@"F:\TEST\TestDicomDemo\Tools\ConsoleWorkList\bin\Debug\testWorkList.wl");
        }
    }
}
