using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Swisscard.MS.Helpers
{
    public class ImageDataToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            Microsoft.EnterpriseManagement.UI.Core.Shared.ImageData imageData = null;
            if (value is IList<Microsoft.EnterpriseManagement.UI.Core.Shared.ImageData>)
            {
                IList<Microsoft.EnterpriseManagement.UI.Core.Shared.ImageData> imagesList = (IList<Microsoft.EnterpriseManagement.UI.Core.Shared.ImageData>)value;

                imageData = imagesList.Where(i => i.Category == 16).FirstOrDefault();
            }
            if (value is Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters.ImageList)
            {
                Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters.ImageList imagesList = (Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters.ImageList)value;

                imageData = imagesList.Where(i => i.Category == 16).FirstOrDefault();
            }
            else if (value is Microsoft.EnterpriseManagement.UI.Core.Shared.ImageData)
            {
                imageData = (Microsoft.EnterpriseManagement.UI.Core.Shared.ImageData)value;
            }

            if (imageData != null)
            {
                BitmapImage bmp = new BitmapImage();

                bmp.BeginInit();
                try
                {
                    bmp.StreamSource = new MemoryStream(imageData.Bytes);
                }
                catch (Exception er)
                {

                }
                finally
                {
                    bmp.EndInit();
                }

                return bmp;
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
