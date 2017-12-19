using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swisscard.MS.Helpers
{
    public static class Constants
    {
        public static string Trace_ETLTracerId = "{F0D3A52C-A5EB-426A-A53A-6A5EB7E80612}";
        public static string Trace_SourceName = "Swisscard MS";

        public static readonly Guid Class_Provance_Class_Company = new Guid("3317f6ce-8752-44e5-c0ec-9439bcfb966f");
        public static readonly Guid Class_Provance_Class_OrganizationBase = new Guid("e50c422b-64cd-19aa-4607-97f51ffec430");
        public static readonly Guid Class_Swisscard_MS = new Guid("981cbf87-0416-f7ee-76ff-649400b11295");
        public static readonly Guid Class_Swisscard_MS_Service = new Guid("1aeb86db-6842-8e18-881a-6f2cccbdf40d");
        public static readonly Guid Class_Swisscard_MS_ServiceComponent = new Guid("000e3aab-0ca7-e8b8-7f86-85ba3ecbc734");
        public static readonly Guid Class_System_Domain_User = new Guid("eca3c52a-f273-5cdc-f165-3eb95a2b26cf");
        public static readonly Guid Class_System_User = new Guid("943d298f-d79a-7a29-a335-8833e582d252");
        public static readonly Guid Class_System_FileAttachment = new Guid("68A35B6D-CA3D-8D90-F93D-248CEFF935C0");
        public static readonly Guid Class_System_RequestOffering = new Guid("8fc1cd4a-b39e-2879-2ba8-b7036f9d8ee7");

        public static readonly Guid Enum_Swisscard_MS_ClassificationEnum = new Guid("90882462-4514-3e14-cb25-64c690b51ced");
        public static readonly Guid Enum_Swisscard_MS_CriticalityEnum = new Guid("ef1ec51f-d2ff-286a-7711-f1c57528215d");
        public static readonly Guid Enum_Swisscard_MS_StatusEnum = new Guid("7d1df241-09d0-c02a-b08d-4ee0af0e8b1a");

        public static readonly Guid MP_Swisscard_MS_Presentation = new Guid("9cb27e70-3732-5014-31d1-3e65a538cab4 ");

        public static readonly Guid TP_Swisscard_MS_Service = new Guid("ffaa9f57-c2e0-c52c-1a7f-82ce88d2b37a");
        public static readonly Guid TP_Swisscard_MS_Service_Component = new Guid("ad58be7f-6d73-9c6b-279a-0f580e6dacd5");
        public static readonly Guid TP_System_FileAttachmentProjection = new Guid("42561C7A-5D16-A5FE-64F9-FFA5BBC6CDB5");

        public static LocalizationHelper LocalizationHelper = new LocalizationHelper("Swisscard.MS.Presentation.FormStrings", Constants.MP_Swisscard_MS_Presentation);

    }
}
