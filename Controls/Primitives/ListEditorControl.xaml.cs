using Swisscard.MS.Helpers;
using Microsoft.EnterpriseManagement.ConsoleFramework;
using Microsoft.EnterpriseManagement.GenericForm;
using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
using Microsoft.EnterpriseManagement.UI.FormsInfra;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters;
using Microsoft.EnterpriseManagement.UI.WpfControls;
using Syliance.Tracing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ItnetX.Controls.Primitives
{
    /// <summary>
    /// Interaction logic for ListEditorControl.xaml
    /// </summary>
    public partial class ListEditorControl : UserControl
    {
        public ListEditorControl()
        {
            this.GridView = new GridView();
            Loaded += ListEditorControl_Loaded;
            GridViewColumns = new GridViewColumnCollection();
            InitializeComponent();
        }

        void ListEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(CollectionPropertyName))
            {
                var headerBinding = new Binding()
                {
                    Path = new PropertyPath("[" + CollectionPropertyName + "]"),
                    Source = Constants.LocalizationHelper,
                    FallbackValue = CollectionPropertyName
                };
                BindingOperations.SetBinding(headerLabel, ContentControl.ContentProperty, headerBinding);

                var itemsBinding = new Binding(CollectionPropertyName);
                BindingOperations.SetBinding(list, ItemsControl.ItemsSourceProperty, itemsBinding);

                if (CanAddNew && String.IsNullOrEmpty(ThisEndProperty))
                    throw new InvalidOperationException("ThisEndProperty must be set in order to use New button");
            }
        }

        public GridViewColumnCollection GridViewColumns
        {
            get { return (GridViewColumnCollection)GetValue(GridViewColumnsProperty); }
            set { SetValue(GridViewColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridViewColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridViewColumnsProperty =
            DependencyProperty.Register("GridViewColumns", typeof(GridViewColumnCollection), typeof(ListEditorControl), new PropertyMetadata(null));

        #region ColumnsSpecification
        public string ColumnsSpecification
        {
            get { return (string)GetValue(ColumnsSpecificationProperty); }
            set { SetValue(ColumnsSpecificationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnsSpecification.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsSpecificationProperty =
            DependencyProperty.Register("ColumnsSpecification", typeof(string), typeof(ListEditorControl), new PropertyMetadata("DisplayName,150"));
        #endregion

        private void BuildColumns()
        {
            var gridView = list.View as GridView;

            if (gridView != null)
            {
                var colSpecArray = ColumnsSpecification.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (colSpecArray.Length == 0) return;
                foreach (string colSpec in colSpecArray)
                {
                    var columnParams = colSpec.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    Debug.Assert(columnParams.Length == 2, "Column specification must be in '<colname>,<colwidth>' format");
                    string colName = columnParams[0];
                    double colWidth = Double.Parse(columnParams[1]);

                    var objColumn = new GridViewColumn();

                    objColumn.DisplayMemberBinding = new Binding()
                    {
                        Path = new PropertyPath(colName),
                        FallbackValue = colName
                    };

                    var headerBinding = new Binding()
                    {
                        Path = new PropertyPath("[" + colName + "]"),
                        Source = Constants.LocalizationHelper,
                        FallbackValue = colName
                    };
                    BindingOperations.SetBinding(objColumn, GridViewColumn.HeaderProperty, headerBinding);

                    objColumn.Width = colWidth;
                    gridView.Columns.Add(objColumn);
                }
            }
        }

        #region GridView
        public GridView GridView
        {
            get { return (GridView)GetValue(GridViewProperty); }
            set { SetValue(GridViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridViewProperty =
            DependencyProperty.Register("GridView", typeof(GridView), typeof(ListEditorControl), new PropertyMetadata(null));
        #endregion


        #region CollectionPropertyName
        public string CollectionPropertyName
        {
            get { return (string)GetValue(CollectionPropertyNameProperty); }
            set { SetValue(CollectionPropertyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollectionPropertyNameProperty =
            DependencyProperty.Register("CollectionPropertyName", typeof(string), typeof(ListEditorControl), new PropertyMetadata(String.Empty));

        #endregion



        public string ThisEndProperty
        {
            get { return (string)GetValue(ThisEndPropertyProperty); }
            set { SetValue(ThisEndPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThisEndProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThisEndPropertyProperty =
            DependencyProperty.Register("ThisEndProperty", typeof(string), typeof(ListEditorControl), new PropertyMetadata(string.Empty));




        public RelationshipMode RelationshipMode
        {
            get { return (RelationshipMode)GetValue(RelationshipModeProperty); }
            set { SetValue(RelationshipModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RelationshipMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RelationshipModeProperty =
            DependencyProperty.Register("RelationshipMode", typeof(RelationshipMode), typeof(ListEditorControl),
            new FrameworkPropertyMetadata(RelationshipMode.Reference, OnRelationshipModeChanged));

        private static void OnRelationshipModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ListEditorControl;
            self.CoerceValue(CanAddProperty);
        }




        #region ItemClassGuid
        public Guid? ItemClassGuid
        {
            get { return (Guid?)GetValue(ItemClassGuidProperty); }
            set { SetValue(ItemClassGuidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemClassGuid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemClassGuidProperty =
            DependencyProperty.Register("ItemClassGuid", typeof(Guid?), typeof(ListEditorControl), new PropertyMetadata(null));

        #endregion

        #region CanEdit


        public bool CanAdd
        {
            get { return (bool)GetValue(CanAddProperty); }
            set { SetValue(CanAddProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanAdd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanAddProperty =
            DependencyProperty.Register("CanAdd", typeof(bool), typeof(ListEditorControl),
            new FrameworkPropertyMetadata(true, OnCanAddChanged, OnCanAddCoerced));

        private static object OnCanAddCoerced(DependencyObject d, object baseValue)
        {
            var self = d as ListEditorControl;
            if (self.RelationshipMode == RelationshipMode.Membership)
                return false;
            else return baseValue;
        }

        private static void OnCanAddChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }



        public bool CanAddNew
        {
            get { return (bool)GetValue(CanAddNewProperty); }
            set { SetValue(CanAddNewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanAddNew.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanAddNewProperty =
            DependencyProperty.Register("CanAddNew", typeof(bool), typeof(ListEditorControl), new PropertyMetadata(true));




        public bool CanRemove
        {
            get { return (bool)GetValue(CanRemoveProperty); }
            set { SetValue(CanRemoveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanRemove.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanRemoveProperty =
            DependencyProperty.Register("CanRemove", typeof(bool), typeof(ListEditorControl), new PropertyMetadata(true));



        #endregion


        #region PickerDialogColumns
        public StringDictionary PickerDialogColumns
        {
            get { return (StringDictionary)GetValue(PickerDialogColumnsProperty); }
            set { SetValue(PickerDialogColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PickerDialogColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PickerDialogColumnsProperty =
            DependencyProperty.Register("PickerDialogColumns", typeof(StringDictionary), typeof(ListEditorControl), new PropertyMetadata(null));

        #endregion


        #region Button Handlers

        internal void LaunchInstancePickerDialogInternal(InstancePickerDialog instancePickerDialog, ref Collection<IDataItem> items)
        {
            try
            {
                if ((items != null) && (items.Count > 0))
                {
                    instancePickerDialog.SetPickedInstances(items);
                }
                bool? nullable = instancePickerDialog.ShowDialog();
                if (nullable.HasValue && ((nullable == true) && (items != null)))
                {
                    foreach (IDataItem item in instancePickerDialog.RemovedInstances)
                    {
                        items.Remove(item);
                    }
                    foreach (IDataItem item2 in instancePickerDialog.PickedInstances)
                    {
                        if (!items.Contains(item2))
                        {
                            items.Add(item2);
                        }
                    }
                }
            }
            finally
            {

            }
        }
        public void LaunchAddInstancePickerDialog(Collection<IDataItem> items, Guid typeId)
        {
            try
            {
                InstancePickerDialog instancePickerDialog = new InstancePickerDialog(typeId)
                {
                    SelectionMode = SelectionMode.Multiple,
                };
                if (PickerDialogColumns != null)
                    instancePickerDialog.ColumnDefinitions = PickerDialogColumns;
                this.LaunchInstancePickerDialogInternal(instancePickerDialog, ref items);
            }
            finally
            {

            }
        }

        void butAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (ItemClassGuid.HasValue)
            {
                LaunchAddInstancePickerDialog((Collection<IDataItem>)list.ItemsSource, ItemClassGuid.Value);
            }
        }

        void butRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (list.SelectedItem is IDataItem)
            {
                this.list.ItemsSource.RemoveFromCollection(list.SelectedItem);
            }
        }

        void butOpenItem_Click(object sender, RoutedEventArgs e)
        {
            var item = list.SelectedItem as IDataItem;
            if (item != null)
            {
                switch (RelationshipMode)
                {
                    case RelationshipMode.Reference:
                        FormUtilities.Instance.PopoutForm(item);
                        break;
                    case RelationshipMode.Membership:
                        popupChildObject(item);
                        break;
                }
            }

        }


        void butAddFullDependence_Click(object sender, RoutedEventArgs e)
        {
            if (ItemClassGuid.HasValue)
            {
                Dictionary<string, string> dictColumns = new Dictionary<string, string>();
                dictColumns.Add("DisplayName", "Display Name");
                dictColumns.Add("Status.DisplayName", "Status");
                dictColumns.Add("$Class$.DisplayName", "Class");

                InstancePickerDialog ciPicker = new InstancePickerDialog();
                ciPicker.ClassId = ItemClassGuid.Value;
                ciPicker.SelectionMode = SelectionMode.Multiple;
                ciPicker.ColumnDefinitions = dictColumns;

                if (list.Items.Count > 0)
                    ciPicker.SetPickedInstances((Collection<IDataItem>)list.ItemsSource);

                bool? result = ciPicker.ShowDialog();
                if (result != null && result == true)
                {
                    Collection<IDataItem> items = (Collection<IDataItem>)list.ItemsSource;
                    foreach (IDataItem item in ciPicker.RemovedInstances)
                        items.Remove(item);

                    foreach (IDataItem item in ciPicker.PickedInstances)
                        if (!items.Contains(item))
                            items.Add(item);
                }

                //FormUtilities.Instance.LaunchAddInstancePickerDialog((Collection<IDataItem>)list.ItemsSource, ItemClassGuid.Value);
            }
        }

        void butRemoveFullDependence_Click(object sender, RoutedEventArgs e)
        {
            if (list.SelectedItem is IDataItem)
            {
                this.list.ItemsSource.RemoveFromCollection(list.SelectedItem);
            }
        }

        void butOpenFullDependence_Click(object sender, RoutedEventArgs e)
        {
            var item = list.SelectedItem as IDataItem;
            if (item != null)
                FormUtilities.Instance.PopoutForm(item);
        }



        private void listItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listItem = sender as ListViewItem;
            if (listItem != null)
            {
                var item = listItem.DataContext as IDataItem;
                if (item != null)
                    FormUtilities.Instance.PopoutForm(item);
            }
        }

        #endregion



        private void addNewItem_ReferenceMode()
        {
            using (TraceHelper tracer = new TraceHelper())
            {
                if (!ItemClassGuid.HasValue)
                    throw new InvalidOperationException("ItemClassGuid must be set before adding new item");

                IDataItem clItem = ConsoleContextHelper.Instance.GetClassType(ItemClassGuid.Value);
                NavigationModelNodeBase base5;
                NavigationModelNodeTask task = NavigationTasksHelper.CreateNewInstanceLink(clItem);

                IAsyncResult formResult = GenericCommon.MonitorCreatedForm(null, task, out base5, true);
                SdkNavigationModelNodeBase createdNode = FormUtilities.Instance.GetCreatedLinkNode(task) as SdkNavigationModelNodeBase;

                // !! THIS IS REQUIRED to open form as child. This is must be USerConteol itself, !!! not the Parent !!!!
                createdNode.NodeContext["ParentNavigationNode"] = FormUtilities.Instance.GetNavigationNode(this);

                // reference to root element
                createdNode.NodeContext["RootIDataItemParameter"] = this.DataContext as IDataItem;
                // Path of the collection where new element created
                createdNode.NodeContext["CollectionParameterName"] = CollectionPropertyName;

                // have no idea there this used
                createdNode.NodeContext["IsTemplateMode"] = FormUtilities.Instance.IsFormInTemplateMode(this);

                Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CallbackData callBackstate = new Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CallbackData(formResult, createdNode, null, null);

                WaitOrTimerCallback callBack = new WaitOrTimerCallback((state, timeout) =>
                {
                    Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CallbackData callBackData = state as Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CallbackData;
                    // this is required
                    NavigationModel.EndExecute(callBackData.Result);

                    IDataItem newItem = FormUtilities.Instance.GetFormDataContext(callBackData.CreatedNode);
                    FormView formView = NavigationModel.FindView(null, callBackData.CreatedNode.Location, FindViewCriteria.ViewIsAssociatedToNode) as FormView;
                    formView.Form.AddHandler(FormEvents.SubmittedEvent, new EventHandler<FormCommandExecutedEventArgs>((sender2, evnt) =>
                    {
                        this.list.Dispatcher.Invoke((Action)(() =>
                        {
                            this.list.ItemsSource.AddToCollection(newItem);
                        }));
                    }), true);

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        if (newItem.HasProperty(ThisEndProperty))
                            newItem[ThisEndProperty] = this.DataContext;
                    }));
                });

                ThreadPool.RegisterWaitForSingleObject(formResult.AsyncWaitHandle, callBack, callBackstate, -1, true);
                // wait until process completed
                while ((formResult != null) && !formResult.IsCompleted)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void addNewItem_MembershipMode()
        {
            using (TraceHelper tracer = new TraceHelper())
            {
                if (!ItemClassGuid.HasValue)
                    throw new InvalidOperationException("ItemClassGuid must be set before adding new item");

                createNewObject();
            }
        }

        private void butNewAddItem_Click(object sender, RoutedEventArgs e)
        {
            switch (RelationshipMode)
            {
                case RelationshipMode.Reference:
                    addNewItem_ReferenceMode();
                    break;
                case RelationshipMode.Membership:
                    addNewItem_MembershipMode();
                    break;
            }
        }

        void createNewObject()
        {
            // to use popupChildObject we must use type projection and add it to list before popup
            IDataItem newObjectItem = ConsoleContextHelper.Instance.CreateNewProjectionInstanceBindableItem(ItemClassGuid.Value);
            this.list.ItemsSource.AddToCollection(newObjectItem);
            popupChildObject(newObjectItem);
        }

        void popupChildObject(IDataItem item)
        {
            // child instance MUST BE created using $EMO$, not the projection object
            NavigationModelNodeTask task = NavigationTasksHelper.CreateChildInstanceLink(item["$EMO$"] as IDataItem, null);
            SdkNavigationModelNodeBase createdNode = FormUtilities.Instance.GetCreatedLinkNode(task) as SdkNavigationModelNodeBase;
            if (createdNode != null)
            {

                // !! THIS IS REQUIRED to open form as child. This is must be USerConteol itself, !!! not the Parent !!!!
                createdNode.NodeContext["ParentNavigationNode"] = FormUtilities.Instance.GetNavigationNode(this);
                // THIS IS ALSO REQUIRED. This parameter used to copy the object
                createdNode.NodeContext["IDataItemParameter"] = item;

                // reference to root element
                createdNode.NodeContext["RootIDataItemParameter"] = this.DataContext as IDataItem;
                // Path of the collection where new element created
                createdNode.NodeContext["CollectionParameterName"] = CollectionPropertyName;

                // have no idea there this used
                createdNode.NodeContext["IsTemplateMode"] = FormUtilities.Instance.IsFormInTemplateMode(this);

                FormView formView = FormUtilities.Instance.GetFormView(this);
                if (formView != null)
                {

                    //GenericCommon.MonitorCreatedForm(formView, createdNode, true);

                    // also , we can do some job after form displayed
                    IAsyncResult formResult = GenericCommon.MonitorCreatedForm(formView, createdNode, true);

                    Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CallbackData callBackstate = new Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CallbackData(formResult, createdNode, null, null);

                    WaitOrTimerCallback callBack = new WaitOrTimerCallback((state, timeout) => {
                        Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CallbackData data = state as Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CallbackData;
                        // this is required
                        NavigationModel.EndExecute(data.Result);

                        FormView formViewPopuped = NavigationModel.FindView(null, data.CreatedNode.Location, FindViewCriteria.ViewIsAssociatedToNode) as FormView;

                        // invoke in Render priority to ensure that formViewPopuped.Form is set
                        formViewPopuped.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                            (EventHandler)((senderPoped, ePoped) =>
                            {
                                if ((bool)item["$IsNew$"])
                                {
                                    formViewPopuped.Form.AddHandler(FormEvents.CanceledEvent, new EventHandler<FormCommandExecutedEventArgs>((sender2, evnt) =>
                                    {
                                        this.list.Dispatcher.Invoke((Action)(() =>
                                        {
                                            this.list.ItemsSource.RemoveFromCollection(item);

                                        }));
                                    }), true);
                                }

                            }), formViewPopuped, null);


                        //    IDataItem newItem = FormUtilities.Instance.GetFormDataContext(data.CreatedNode);
                        //    newItem["DisplayName"] = "Test";

                    });

                    ThreadPool.RegisterWaitForSingleObject(formResult.AsyncWaitHandle, callBack, callBackstate, -1, true);
                    //// wait until process completed
                    //while ((formResult != null) && !formResult.IsCompleted)
                    //{
                    //    Thread.Sleep(100);
                    //}

                }
            }
        }


    }

    public class StringDictionary : Dictionary<string, string>
    {

    }

    public enum RelationshipMode
    {
        Reference,
        Membership
    }
}
