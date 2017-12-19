using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ConsoleFramework;
using System.Globalization;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.UI.Core.Connection;

namespace Swisscard.MS.Helpers
{
    public class LocalizationHelper
    {
        string displayStringPrefix = "";
        Guid managementPackId;
        ManagementPack mp = null;
        IManagementGroupSession session = null;

        public LocalizationHelper(string displayStringPrefix, Guid managementPackId)
        {
            this.displayStringPrefix = displayStringPrefix;
            this.managementPackId = managementPackId;
            this.session = (IManagementGroupSession)FrameworkServices.GetService<IManagementGroupSession>();
            if (session != null)
            {
                this.mp = session.ManagementGroup.ManagementPacks.GetManagementPack(this.managementPackId);
            }
        }

        public string this[string index]
        {
            get
            {

                if (this.mp != null)
                {
                    string resourceId = string.Format("{0}.{1}", this.displayStringPrefix, index);

                    try
                    {
                        ManagementPackStringResource res = mp.GetStringResource(resourceId);
                        return res.GetDisplayString(CultureInfo.CurrentUICulture).Name;
                    }
                    catch
                    {
                        try
                        {
                            return mp.GetStringResource(resourceId).DisplayName;
                        }
                        catch (ObjectNotFoundException er)
                        {
                            throw new ObjectNotFoundException(resourceId + " (" + mp.DefaultLanguageCode + ")", er);
                        }
                    }
                }
                else
                {
                    return string.Format("[{0}]", index);
                }

            }

        }

        public string this[string index, CultureInfo cultureInfo]
        {
            get
            {

                if (this.mp != null)
                {
                    string resourceId = string.Format("{0}.{1}", this.displayStringPrefix, index);

                    ManagementPackStringResource res = mp.GetStringResource(resourceId);
                    try
                    {
                        return res.GetDisplayString(cultureInfo).Name;
                    }
                    catch
                    {
                        try
                        {
                            return mp.GetStringResource(resourceId).DisplayName;
                        }
                        catch (ObjectNotFoundException er)
                        {
                            throw new ObjectNotFoundException(resourceId + " (" + mp.DefaultLanguageCode + ")", er);
                        }
                    }
                }
                else
                {
                    return string.Format("[{0}]", index);
                }

            }

        }
    }

}

