// Code Auther: Flithor (Mr.Squirrel.Downy)
// License: MIT
// =======WARNING=======
// Use this code at your own risk
// The newest code may on the https://stackoverflow.com/a/77603913/6859121

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

using Expression = System.Linq.Expressions.Expression;

namespace Flithor_ReusableCodes
{
    /// <summary>
    /// A DataGrid Column work for binding to Checked property for item element, and can Check All
    /// </summary>
    public class DataGridCheckAllColumn : DataGridBoundColumn
    {
        #region Private Fields
        //CheckBox in header
        private readonly CheckBox checkAllCheckBox;
        //owner DataGrid control for this column
        private DataGrid ownerDatagrid;
        //owner DataGrid current delegate get current list version
        //if version changed then change bindings
        private Func<int> getInnerEnumeratorVersion;
        //cached list version
        private int cachedInnerVersion;
        //default style for CheckBox
        private static Style _defaultElementStyle;
        #endregion

        #region Initialize Control
        public static Style DefaultElementStyle
        {
            get
            {
                if (_defaultElementStyle == null)
                {
                    var style = new Style(typeof(CheckBox))
                    {
                        Setters =
                        {
                            new Setter(UIElement.FocusableProperty, false),
                            new Setter(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center),
                            new Setter(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center)
                        }
                    };

                    style.Seal();
                    _defaultElementStyle = style;
                }

                return _defaultElementStyle;
            }
        }

        static DataGridCheckAllColumn()
        {
            //override default element style
            ElementStyleProperty.OverrideMetadata(typeof(DataGridCheckAllColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
            //make column readonly by default
            IsReadOnlyProperty.OverrideMetadata(typeof(DataGridCheckAllColumn), new FrameworkPropertyMetadata(true));
            //not allows move column
            CanUserReorderProperty.OverrideMetadata(typeof(DataGridCheckAllColumn), new FrameworkPropertyMetadata(false));
            //not allows resize column
            CanUserResizeProperty.OverrideMetadata(typeof(DataGridCheckAllColumn), new FrameworkPropertyMetadata(false));
            //not allows order items by click header
            CanUserSortProperty.OverrideMetadata(typeof(DataGridCheckAllColumn), new FrameworkPropertyMetadata(false));
        }

        public DataGridCheckAllColumn()
        {
            //override header
            Header = checkAllCheckBox = new CheckBox();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ownerDatagrid != null) return;

            ownerDatagrid = GetParentDataGrid();
            if (ownerDatagrid == null) return;

            InitInnerVersionDetect(ownerDatagrid.Items);

            ((INotifyPropertyChanged)ownerDatagrid.Items).PropertyChanged += OnPropertyChanged;

            //if DataGrid has items now, init bindings
            checkAllCheckBox.IsEnabled = ownerDatagrid.Items.Count > 0;
            if (checkAllCheckBox.IsEnabled)
                ResetCheckCurrentAllBinding();
        }
        //find parent DataGrid(if not end initialize, may return null)
        private DataGrid GetParentDataGrid()
        {
            DependencyObject elment = checkAllCheckBox;
            do
            {
                elment = VisualTreeHelper.GetParent(elment);
            }
            while (elment != null && !(elment is DataGrid));
            return elment as DataGrid;
        }
        #endregion

        #region Generate Element
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            return GenerateCheckBox(false, cell, dataItem);
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateCheckBox(true, cell, dataItem);
        }

        private CheckBox GenerateCheckBox(bool isEditing, DataGridCell cell, object dataItem)
        {
            var checkBox = new CheckBox();
            ApplyStyle(isEditing, checkBox);
            ApplyBinding(dataItem, checkBox);
            return checkBox;
        }

        private void ApplyBinding(object dataItem, CheckBox checkBox)
        {
            var binding = CloneBinding(Binding, dataItem);
            if (binding is Binding newBinding)
            {
                newBinding.Mode = BindingMode.TwoWay;
                newBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }
            BindingOperations.ClearBinding(checkBox, CheckBox.IsCheckedProperty);
            checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);
        }

        internal void ApplyStyle(bool isEditing, FrameworkElement element)
        {
            Style style = PickStyle(isEditing);
            if (style != null)
            {
                element.Style = style;
            }
        }

        private Style PickStyle(bool isEditing)
        {
            Style style = isEditing ? EditingElementStyle : ElementStyle;
            if (isEditing && (style == null))
            {
                style = ElementStyle;
            }

            return style;
        }
        #endregion

        #region Update Binding
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ownerDatagrid == null || e.PropertyName != nameof(ownerDatagrid.Items.Count))
                return;
            //if items count changed then means the collection may changed
            if (ownerDatagrid.Items.Count == 0)
            {
                //if Items goes empty then clear the check binding and disable check all
                BindingOperations.ClearBinding(checkAllCheckBox, CheckBox.IsCheckedProperty);
                checkAllCheckBox.IsEnabled = false;
            }
            else
            {
                //else update the binding to current displayed items
                ResetCheckCurrentAllBinding();
                checkAllCheckBox.IsEnabled = true;
            }
        }

        private void ResetCheckCurrentAllBinding()
        {
            //If version changed then update binding by current items
            if (ownerDatagrid == null || !InnerVersionChanged()) return;

            var checkAllBinding = new MultiBinding
            {
                Converter = AllBoolStatusConverter.Default,
                Mode = BindingMode.TwoWay
            };
            //binding items by current displayed items
            var currentItems = ownerDatagrid.Items.OfType<object>().ToList();
            foreach (var item in currentItems)
            {
                checkAllBinding.Bindings.Add(CloneBinding((Binding)Binding, item));
            }

            //clear old binding if exists
            BindingOperations.ClearBinding(checkAllCheckBox, CheckBox.IsCheckedProperty);

            checkAllCheckBox.SetBinding(CheckBox.IsCheckedProperty, checkAllBinding);
        }

        //generate DataGrid.Items version get delegate
        private void InitInnerVersionDetect(ItemCollection itemCollection)
        {
            //Timestamp property is the version mark of ItemCollection to tell us is it changed
            var collectionTimestampProerty = itemCollection.GetType()
                .GetProperty("Timestamp", BindingFlags.Instance | BindingFlags.NonPublic);
            //use Linq Expression build a simple delegate to access Timestamp property
            getInnerEnumeratorVersion = Expression.Lambda<Func<int>>(Expression.Property(
                Expression.Constant(itemCollection),
                collectionTimestampProerty)).Compile();
        }
        //get the inner collection version to detect is it changed
        private bool InnerVersionChanged()
        {
            var currentInnerVersion = getInnerEnumeratorVersion.Invoke();
            if (currentInnerVersion != cachedInnerVersion)
            {
                cachedInnerVersion = currentInnerVersion;
                return true;
            }

            return false;
        }
        //create a new binding instance by existed binding
        private static BindingBase CloneBinding(BindingBase bindingBase, object source)
        {
            switch (bindingBase)
            {
                case Binding binding:
                    var resultBinding = new Binding
                    {
                        Source = source,
                        AsyncState = binding.AsyncState,
                        BindingGroupName = binding.BindingGroupName,
                        BindsDirectlyToSource = binding.BindsDirectlyToSource,
                        Converter = binding.Converter,
                        ConverterCulture = binding.ConverterCulture,
                        ConverterParameter = binding.ConverterParameter,
                        //ElementName = binding.ElementName,
                        FallbackValue = binding.FallbackValue,
                        IsAsync = binding.IsAsync,
                        Mode = binding.Mode,
                        NotifyOnSourceUpdated = binding.NotifyOnSourceUpdated,
                        NotifyOnTargetUpdated = binding.NotifyOnTargetUpdated,
                        NotifyOnValidationError = binding.NotifyOnValidationError,
                        Path = binding.Path,
                        //RelativeSource = binding.RelativeSource,
                        StringFormat = binding.StringFormat,
                        TargetNullValue = binding.TargetNullValue,
                        UpdateSourceExceptionFilter = binding.UpdateSourceExceptionFilter,
                        UpdateSourceTrigger = binding.UpdateSourceTrigger,
                        ValidatesOnDataErrors = binding.ValidatesOnDataErrors,
                        ValidatesOnExceptions = binding.ValidatesOnExceptions,
                        XPath = binding.XPath,
                    };

                    foreach (var validationRule in binding.ValidationRules)
                    {
                        resultBinding.ValidationRules.Add(validationRule);
                    }

                    return resultBinding;
                case MultiBinding multiBinding:
                    var resultMultiBinding = new MultiBinding
                    {
                        BindingGroupName = multiBinding.BindingGroupName,
                        Converter = multiBinding.Converter,
                        ConverterCulture = multiBinding.ConverterCulture,
                        ConverterParameter = multiBinding.ConverterParameter,
                        FallbackValue = multiBinding.FallbackValue,
                        Mode = multiBinding.Mode,
                        NotifyOnSourceUpdated = multiBinding.NotifyOnSourceUpdated,
                        NotifyOnTargetUpdated = multiBinding.NotifyOnTargetUpdated,
                        NotifyOnValidationError = multiBinding.NotifyOnValidationError,
                        StringFormat = multiBinding.StringFormat,
                        TargetNullValue = multiBinding.TargetNullValue,
                        UpdateSourceExceptionFilter = multiBinding.UpdateSourceExceptionFilter,
                        UpdateSourceTrigger = multiBinding.UpdateSourceTrigger,
                        ValidatesOnDataErrors = multiBinding.ValidatesOnDataErrors,
                        ValidatesOnExceptions = multiBinding.ValidatesOnDataErrors,
                    };

                    foreach (var validationRule in multiBinding.ValidationRules)
                    {
                        resultMultiBinding.ValidationRules.Add(validationRule);
                    }

                    foreach (var childBinding in multiBinding.Bindings)
                    {
                        resultMultiBinding.Bindings.Add(CloneBinding(childBinding, source));
                    }

                    return resultMultiBinding;
                case PriorityBinding priorityBinding:
                    var resultPriorityBinding = new PriorityBinding
                    {
                        BindingGroupName = priorityBinding.BindingGroupName,
                        FallbackValue = priorityBinding.FallbackValue,
                        StringFormat = priorityBinding.StringFormat,
                        TargetNullValue = priorityBinding.TargetNullValue,
                    };

                    foreach (var childBinding in priorityBinding.Bindings)
                    {
                        resultPriorityBinding.Bindings.Add(CloneBinding(childBinding, source));
                    }

                    return resultPriorityBinding;
                default:
                    throw new NotSupportedException("Failed to clone binding");
            }
        }
        /// <summary>
        /// A MultiValueConverter to merge all items bound bool value into one
        /// </summary>
        private class AllBoolStatusConverter : IMultiValueConverter
        {
            public static readonly AllBoolStatusConverter Default = new AllBoolStatusConverter();

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values.Length == 0 || values.OfType<bool>().Count() != values.Length)
                    return false;
                // detect all items are equals the first
                var firstStatus = values.First();

                foreach (var value in values)
                {
                    //any one not equals to first then return null
                    if (!Equals(value, firstStatus))
                        return null;
                }

                return firstStatus;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                //if the check all CheckBox checked or unchecked then update all items bound value
                var res = new object[targetTypes.Length];
                for (int i = 0; i < res.Length; i++)
                    res[i] = value;
                return res;
            }
        }
        #endregion

    }
}
