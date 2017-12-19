using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Swisscard.MS.Controls
{
    /// <summary>
    /// Interaction logic for SwisscardMSService.xaml
    /// </summary>
    public partial class SwisscardMSService : UserControl
    {
        public SwisscardMSService()
        {
            InitializeComponent();
            this.RelatedItemsTab.Content = new Microsoft.EnterpriseManagement.ServiceManager.Application.Common.RelatedItemsPane(new Helpers.ConfigItemRelatedItemsConfiguration());
        }
    }
}
