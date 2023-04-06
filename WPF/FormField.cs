using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Flithor_ReusableCodes.Controls
{
    [TemplatePart(Name = "PART_FieldWidth")]
    [TemplatePart(Name = "PART_FieldHight")]
    public class FormField : HeaderedContentControl
    {
        private ColumnDefinition partFieldWidth;
        private RowDefinition partFieldHeight;

        static FormField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FormField), new FrameworkPropertyMetadata(typeof(FormField)));
            IsTabStopProperty.OverrideMetadata(typeof(FormField), new FrameworkPropertyMetadata(false));
        }
        /// <summary>
        /// Apply the Template
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            partFieldWidth = GetTemplateChild("PART_FieldWidth") as ColumnDefinition;
            partFieldHeight = GetTemplateChild("PART_FieldHeight") as RowDefinition;
            UpdateSharedSizeGroup();
        }
        /// <summary>
        /// Visual Parent Changed to update layout
        /// </summary>
        /// <param name="oldParent"></param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (VisualParent == null) return;
            if (partFieldWidth != null || partFieldHeight != null)
                UpdateSharedSizeGroup();
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (!IsInitialized) return;
            if (e.Property == Grid.ColumnProperty)
            {
                UpdateSharedSizeGroup();
            }
        }
        /// <summary>
        /// Reset Shared Size Group ID
        /// </summary>
        private void UpdateSharedSizeGroup()
        {
            var shareSizeParent = InnerShareSizeParent;
            if (shareSizeParent == null) return;
            //horizontal layout
            if (ContainerPosition == FieldContainerPosition.Right && partFieldWidth != null)
            {
                if (NotAlignWidth)
                    partFieldWidth.SharedSizeGroup = null;
                else
                {
                    var columnNoTemp = GetValue(Grid.ColumnProperty);
                    var columnNo = Equals(columnNoTemp, DependencyProperty.UnsetValue) ? 0 : (int)columnNoTemp;
                    partFieldWidth.SharedSizeGroup = $"{shareSizeParent.GetType().Name}{shareSizeParent.GetHashCode()}_{columnNo}";
                    shareSizeParent.SetValue(Grid.IsSharedSizeScopeProperty, true);
                }
            }
            //vertical layout
            else if (ContainerPosition == FieldContainerPosition.Bottom && partFieldHeight != null)
            {
                if (NotAlignWidth)
                    partFieldHeight.SharedSizeGroup = null;
                else
                {
                    var columnNoTemp = GetValue(Grid.RowProperty);
                    var columnNo = Equals(columnNoTemp, DependencyProperty.UnsetValue) ? 0 : (int)columnNoTemp;
                    partFieldHeight.SharedSizeGroup = $"{shareSizeParent.GetType().Name}{shareSizeParent.GetHashCode()}_{columnNo}";
                    shareSizeParent.SetValue(Grid.IsSharedSizeScopeProperty, true);
                }
            }
        }

        #region Layout properties

        #region Grid layout

        #region Row
        public int Row
        {
            get => (int)GetValue(RowProperty);
            set => SetValue(RowProperty, value);
        }

        // Using a DependencyProperty as the backing store for Row.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowProperty =
            DependencyProperty.Register(nameof(Row), typeof(int), typeof(FormField), new PropertyMetadata(OnRowValueChanged));

        private static void OnRowValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d as FormField)?.VisualParent is Grid)
                d.SetValue(Grid.RowProperty, e.NewValue);
        }

        public int RowSpan
        {
            get => (int)GetValue(RowSpanProperty);
            set => SetValue(RowSpanProperty, value);
        }

        // Using a DependencyProperty as the backing store for RowSpan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowSpanProperty =
            DependencyProperty.Register(nameof(RowSpan), typeof(int), typeof(FormField), new PropertyMetadata(OnRowSpanValueChanged));

        private static void OnRowSpanValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d as FormField)?.VisualParent is Grid)
                d.SetValue(Grid.RowSpanProperty, e.NewValue);
        }
        #endregion

        #region Column
        public int Column
        {
            get => (int)GetValue(ColumnProperty);
            set => SetValue(ColumnProperty, value);
        }

        // Using a DependencyProperty as the backing store for Column.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register(nameof(Column), typeof(int), typeof(FormField), new PropertyMetadata(OnColumnValueChanged));

        private static void OnColumnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(Grid.ColumnProperty, e.NewValue);
        }

        public int ColumnSpan
        {
            get => (int)GetValue(ColumnSpanProperty);
            set => SetValue(ColumnSpanProperty, value);
        }

        // Using a DependencyProperty as the backing store for ColumnSpan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnSpanProperty =
            DependencyProperty.Register(nameof(ColumnSpan), typeof(int), typeof(FormField), new PropertyMetadata(OnColumnSpanValueChanged));

        private static void OnColumnSpanValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is int value && value <= 1) d.SetValue(Grid.ColumnSpanProperty, 1);
            d.SetValue(Grid.ColumnSpanProperty, e.NewValue);
        }
        #endregion

        #endregion

        #region Gap
        /// <summary>
        /// Gap
        /// </summary>
        public GridLength Gap
        {
            get => GetGap(this);
            set => SetGap(this, value);
        }
        public static GridLength GetGap(DependencyObject obj)
        {
            return (GridLength)obj.GetValue(GapProperty);
        }

        public static void SetGap(DependencyObject obj, GridLength value)
        {
            obj.SetValue(GapProperty, value);
        }

        // Using a DependencyProperty as the backing store for Gap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GapProperty =
            DependencyProperty.RegisterAttached("Gap",
                typeof(GridLength),
                typeof(FormField),
                new FrameworkPropertyMetadata(new GridLength(2),
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

        #endregion

        #region Header alignment
        public HorizontalAlignment HorizontalHeaderAlignment
        {
            get => GetHorizontalHeaderAlignment(this);
            set => SetHorizontalHeaderAlignment(this, value);
        }

        public static HorizontalAlignment GetHorizontalHeaderAlignment(DependencyObject obj)
        {
            return (HorizontalAlignment)obj.GetValue(HorizontalHeaderAlignmentProperty);
        }

        public static void SetHorizontalHeaderAlignment(DependencyObject obj, HorizontalAlignment value)
        {
            obj.SetValue(HorizontalHeaderAlignmentProperty, value);
        }

        // Using a DependencyProperty as the backing store for HorizontalHeaderAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalHeaderAlignmentProperty =
            DependencyProperty.RegisterAttached("HorizontalHeaderAlignment",
                typeof(HorizontalAlignment),
                typeof(FormField),
                new FrameworkPropertyMetadata(HorizontalAlignment.Left,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

        public VerticalAlignment VerticalHeaderAlignment
        {
            get => (VerticalAlignment)GetValue(VerticalHeaderAlignmentProperty);
            set => SetValue(VerticalHeaderAlignmentProperty, value);
        }

        // Using a DependencyProperty as the backing store for VerticalHeaderAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalHeaderAlignmentProperty =
            DependencyProperty.Register(nameof(VerticalHeaderAlignment),
                typeof(VerticalAlignment),
                typeof(FormField),
                new PropertyMetadata(VerticalAlignment.Center));



        public bool NotAlignWidth
        {
            get => GetNotAlignWidth(this);
            set => SetNotAlignWidth(this, value);
        }
        public static bool GetNotAlignWidth(DependencyObject obj)
            => (bool)obj.GetValue(NotAlignWidthProperty);
        public static void SetNotAlignWidth(DependencyObject obj, bool value)
            => obj.SetValue(NotAlignWidthProperty, value);

        // Using a DependencyProperty as the backing store for NotAlignWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotAlignWidthProperty =
            DependencyProperty.RegisterAttached(nameof(NotAlignWidth), typeof(bool), typeof(FormField),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits,
                    OnNotAlignWidthChanged));

        private static void OnNotAlignWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FormField)?.UpdateSharedSizeGroup();
        }

        public FieldContainerPosition ContainerPosition
        {
            get => GetContainerPosition(this);
            set => SetContainerPosition(this, value);
        }
        public static FieldContainerPosition GetContainerPosition(DependencyObject obj)
        {
            return (FieldContainerPosition)obj.GetValue(ContainerPositionProperty);
        }

        public static void SetContainerPosition(DependencyObject obj, FieldContainerPosition value)
        {
            obj.SetValue(ContainerPositionProperty, value);
        }

        // Using a DependencyProperty as the backing store for ContainerPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContainerPositionProperty =
            DependencyProperty.RegisterAttached(nameof(ContainerPosition), typeof(FieldContainerPosition), typeof(FormField),
                new FrameworkPropertyMetadata(FieldContainerPosition.Right,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
        #endregion

        #region Shared size parent
        public DependencyObject ShareSizeParent
        {
            get => GetShareSizeParent(this);
            set => SetShareSizeParent(this, value);
        }

        public static DependencyObject GetShareSizeParent(DependencyObject obj)
        {
            return (DependencyObject)obj.GetValue(ShareSizeParentProperty);
        }

        public static void SetShareSizeParent(DependencyObject obj, DependencyObject value)
        {
            obj.SetValue(ShareSizeParentProperty, value);
        }

        // Using a DependencyProperty as the backing store for ShareSizeParent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShareSizeParentProperty =
            DependencyProperty.RegisterAttached(nameof(ShareSizeParent), typeof(DependencyObject), typeof(FormField),
                new FrameworkPropertyMetadata(null, OnShareSizeParentChanged));

        private static void OnShareSizeParentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (d)
            {
                case FormField f:
                    f.UpdateSharedSizeGroup();
                    break;
                case Panel p:
                    foreach (FormField child in p.Children)
                        child?.UpdateSharedSizeGroup();
                    break;
            }
        }

        private DependencyObject InnerShareSizeParent =>
            ShareSizeParent ??
            VisualParent?.GetValue(ShareSizeParentProperty) as DependencyObject ??
            VisualParent;

        #endregion

        #endregion
    }
    /// <summary>
    /// layout direction
    /// </summary>
    public enum FieldContainerPosition
    {
        Right,
        Bottom
    }
}
