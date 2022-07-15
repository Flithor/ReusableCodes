using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Flithor_ReusableCodes.Controls
{
    /// <summary>
    /// A ComboBox style and behavior with any custom content
    /// Part of code from: https://referencesource.microsoft.com/
    /// </summary>
    [TemplatePart(Name = NAME_PART_DROPDOWNBUTTON, Type = typeof(ToggleButton))]
    [TemplatePart(Name = NAME_PART_POPUP, Type = typeof(Popup))]
    public class DropDownControl : ContentControl
    {
        static DropDownControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownControl), new FrameworkPropertyMetadata(typeof(DropDownControl)));
            EventManager.RegisterClassHandler(typeof(DropDownControl), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
            EventManager.RegisterClassHandler(typeof(DropDownControl), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), handledEventsToo: true);
        }

        private const string NAME_PART_POPUP = "PART_Popup";
        private const string NAME_PART_DROPDOWNBUTTON = "PART_DropDownButton";
        private Popup _popup;
        private ToggleButton _dropdownButton;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _popup = GetTemplateChild(NAME_PART_POPUP) as Popup;
            if (_popup != null)
            {
                _popup.Loaded += _popup_Loaded;

                _popup.Opened += _popup_Opened;
                _popup.Closed += _popup_Closed; ;
            }
            _dropdownButton = GetTemplateChild(NAME_PART_DROPDOWNBUTTON) as ToggleButton;
        }

        private void _popup_Closed(object sender, EventArgs e)
        {
            Mouse.Capture(null, CaptureMode.None);
        }

        private void _popup_Loaded(object sender, RoutedEventArgs e)
        {
            _popup.IsOpen = true;
            _popup.IsOpen = false;
            _popup.Loaded -= _popup_Loaded;
        }

        private void _popup_Opened(object sender, EventArgs e)
        {
            Mouse.Capture(this, CaptureMode.SubTree);
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDropDownOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(DropDownControl),
                new PropertyMetadata(false));

        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxDropDownHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(DropDownControl),
                new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 3.0));

        public object Display
        {
            get { return (object)GetValue(DisplayProperty); }
            set { SetValue(DisplayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Display.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayProperty =
            DependencyProperty.Register("Display", typeof(object), typeof(DropDownControl),
                new PropertyMetadata(null));
        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            DropDownControl ddet = (DropDownControl)sender;
            if (Mouse.Captured == ddet)
            {
                return;
            }

            if (e.OriginalSource == ddet)
            {
                if (Mouse.Captured == null || !IsDescendant(ddet, Mouse.Captured as DependencyObject))
                {
                    ddet.Close();
                }
            }
            else if (IsDescendant(ddet, e.OriginalSource as DependencyObject))
            {
                if (ddet.IsDropDownOpen && Mouse.Captured == null && GetCapture() == IntPtr.Zero)
                {
                    Mouse.Capture(ddet, CaptureMode.SubTree);
                    e.Handled = true;
                }
            }
            else
            {
                ddet.Close();
            }
        }
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetCapture();

        internal static bool IsDescendant(DependencyObject reference, DependencyObject node)
        {
            bool IsPopRoot(DependencyObject dependencyObject) => dependencyObject.GetType().Name == "PopupRoot";

            bool result = false;
            DependencyObject dependencyObject = node;
            while (dependencyObject != null)
            {
                if (dependencyObject == reference)
                {
                    result = true;
                    break;
                }

                if (IsPopRoot(dependencyObject))
                {
                    Popup popup = (dependencyObject as FrameworkElement).Parent as Popup;
                    dependencyObject = popup;
                    if (popup != null)
                    {
                        dependencyObject = popup.Parent;
                        if (dependencyObject == null)
                        {
                            dependencyObject = popup.PlacementTarget;
                        }
                    }
                }
                else
                {
                    dependencyObject = FindParent(dependencyObject);
                }
            }

            return result;
        }
        internal static DependencyObject FindParent(DependencyObject o)
        {
            // see if o is a Visual or a Visual3D
            DependencyObject v = o as Visual;
            if (v == null)
            {
                v = o as Visual3D;
            }

            ContentElement ce = (v == null) ? o as ContentElement : null;

            if (ce != null)
            {
                o = ContentOperations.GetParent(ce);
                if (o != null)
                {
                    return o;
                }
                else
                {
                    FrameworkContentElement fce = ce as FrameworkContentElement;
                    if (fce != null)
                    {
                        return fce.Parent;
                    }
                }
            }
            else if (v != null)
            {
                return VisualTreeHelper.GetParent(v);
            }

            return null;
        }
        private static DependencyObject GetVisualRoot(DependencyObject item)
        {
            var root = item;
            while (item != null)
            {
                item = VisualTreeHelper.GetParent(item);
                if (item != null)
                    root = item;
            }
            item = LogicalTreeHelper.GetParent(root);
            if (item != null)
                root = item;
            return root;
        }

        private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            DropDownControl ddet = (DropDownControl)sender;
            if (!ddet.IsDropDownOpen)
            {
                ddet.Focus();
            }

            e.Handled = true;
            if (Mouse.Captured == ddet && e.OriginalSource == ddet)
            {
                ddet.Close();
            }
        }

        private void Close()
        {
            if (IsDropDownOpen)
            {
                IsDropDownOpen = false;
                Mouse.Capture(null);
            }
        }
    }
}
