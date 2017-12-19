using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess.Common;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters;
using Syliance.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Swisscard.MS.Helpers
{
    internal class SDKHelper
    {
        public static void OpenFileFromDisk(IList<IDataItem> attachments)
        {
            using (TraceHelper tracer = new TraceHelper())
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                //fixed for bug 6, allow multiselection
                dlg.Multiselect = true;
                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    foreach (string fileName in dlg.FileNames)
                    {
                        FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        IDataItem attachment = SDKHelper.CreateAttachemntObject(stream, System.IO.Path.GetFileName(fileName));
                        attachments.Add(attachment);
                    }
                }
            }
        }

        public static IDataItem CreateAttachemntObject(Stream fileStream, string fileName)
        {
            using (TraceHelper tracer = new TraceHelper())
            {
                if (fileStream.CanSeek)
                {
                    fileStream.Seek(0L, SeekOrigin.Begin);
                }

                DateTime now = DateTime.Now;
                IDataItem item = ConsoleContextHelper.Instance.CreateProjectionInstance(Constants.TP_System_FileAttachmentProjection, Constants.Class_System_FileAttachment);
                item["FileAttachmentAddedBy"] = ConsoleContextHelper.Instance.CurrentUser;
                item["Extension"] = System.IO.Path.GetExtension(fileName);
                item["Size"] = unchecked((int)fileStream.Length);
                item["AddedDate"] = now;
                item["Id"] = Guid.NewGuid().ToString();
                item["Content"] = fileStream;
                item["DisplayName"] = fileName;

                return item;
            }
        }

        public static void OpenAttachment(IDataItem attachment)
        {
            using (TraceHelper tracer = new TraceHelper())
            {
                string filePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), (string)attachment["DisplayName"]);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    SDKHelper.CopyStream((Stream)attachment["Content"], fs);
                    fs.Flush();
                    fs.Close();
                }

                Process.Start(filePath);
            }
        }
        public static void CopyStream(Stream input, Stream output)
        {
            using (TraceHelper tracer = new TraceHelper())
            {
                if (input.CanSeek)
                    input.Position = 0;
                byte[] buffer = new byte[32768];
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, read);
                }
                output.Position = 0;
            }
        }

        public static IList<IDataItem> GetInstances(string criteria, Guid classId)
        {
            Dictionary<string, object> parameterList = new Dictionary<string, object>();
            parameterList.Add("ManagementPackClassId", classId);
            parameterList.Add(EnterpriseManagementObjectAdapter.InstanceCriteria, criteria);

            IList<IDataItem> list = (new DataAccessQuery()).QueryAdapter(parameterList, null, typeof(EnterpriseManagementObjectAdapter), EnterpriseManagementObjectAdapter.AdapterName);
            return list;
        }

    }
}
