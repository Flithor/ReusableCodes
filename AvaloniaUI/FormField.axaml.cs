using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Flithor_ReusableCodes.Controls
{
    public partial class FormField : HeaderedContentControl, IHeadered, IStyleable
    {
        private Grid contentGrid;

        private ColumnDefinition PART_FieldWidth = new ColumnDefinition(GridLength.Auto);
        private ColumnDefinition HorizontalGap = new ColumnDefinition(0, GridUnitType.Pixel);
        private IDisposable HorizontalGapBinding;
        private ColumnDefinition HorizontalContainer = new ColumnDefinition(GridLength.Star);
        private RowDefinition PART_FieldHigth = new RowDefinition(GridLength.Auto);
        private RowDefinition VerticalGap = new RowDefinition(0, GridUnitType.Pixel);
        private IDisposable VerticalGapBinding;
        private RowDefinition VerticalContainer = new RowDefinition(GridLength.Star);

        static FormField()
        {
            IsTabStopProperty.OverrideMetadata(typeof(FormField), new StyledPropertyMetadata<bool>(false));
        }

        public Type StyleKey { get; } = typeof(FormField);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            contentGrid = e.NameScope.Get<Grid>("Grid_Content");
            // clear and reset
            contentGrid?.ColumnDefinitions.Clear();
            contentGrid?.RowDefinitions.Clear();
            UpdateSharedSizeGroup();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            // 2
            if (e.Parent == null) return;
            var shareSizeParent = InnerShareSizeParent;
            if (shareSizeParent == null) return;
            Grid.SetIsSharedSizeScope(shareSizeParent as Control, true);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (!IsInitialized) return;
            if (change.Property == Grid.ColumnProperty)
            {
                UpdateSharedSizeGroup();
            }
        }

        /// <summary>
        /// Reset shared size group ID
        /// </summary>
        private void UpdateSharedSizeGroup()
        {
            var shareSizeParent = InnerShareSizeParent;
            if (shareSizeParent == null) return;

            if (ContainerPosition == FieldContainerPosition.Right && PART_FieldWidth != null)
            {
                if (NotAlignWidth)
                    PART_FieldWidth.SharedSizeGroup = null;
                else
                {
                    var columnNoTemp = GetValue(Grid.ColumnProperty);
                    var columnNo = Equals(columnNoTemp, AvaloniaProperty.UnsetValue) ? 0 : (int)columnNoTemp;
                    PART_FieldWidth.SharedSizeGroup = $"{shareSizeParent.GetType().Name}{shareSizeParent.GetHashCode()}_{columnNo}";
                }

                VerticalGapBinding?.Dispose();
                HorizontalGapBinding =
                    HorizontalGap.Bind(ColumnDefinition.WidthProperty, this.GetBindingObservable(GapProperty));

                contentGrid.RowDefinitions.Clear();
                contentGrid.ColumnDefinitions.AddRange(new[] { PART_FieldWidth, HorizontalGap, HorizontalContainer });
            }
            else if (ContainerPosition == FieldContainerPosition.Bottom && PART_FieldHigth != null)
            {
                if (NotAlignWidth)
                    PART_FieldHigth.SharedSizeGroup = null;
                else
                {
                    var columnNoTemp = GetValue(Grid.RowProperty);
                    var columnNo = Equals(columnNoTemp, AvaloniaProperty.UnsetValue) ? 0 : (int)columnNoTemp;
                    PART_FieldHigth.SharedSizeGroup = $"{shareSizeParent.GetType().Name}{shareSizeParent.GetHashCode()}_{columnNo}";
                }
                HorizontalGapBinding?.Dispose();
                VerticalGapBinding =
                    VerticalGap.Bind(RowDefinition.HeightProperty, this.GetBindingObservable(GapProperty));

                contentGrid.ColumnDefinitions.Clear();
                contentGrid.RowDefinitions.AddRange(new[] { PART_FieldHigth, VerticalGap, VerticalContainer });
            }
        }

        #region Layout properties

        #region Grid layout

        #region row
        /// <summary>
        /// Defines the <see cref="Row"/> property.
        /// </summary>
        public static readonly StyledProperty<int> RowProperty =
            AvaloniaProperty.Register<FormField, int>(nameof(Row));

        /// <summary>
        /// quick set grid row
        /// </summary>
        public int Row
        {
            get { return GetValue(Grid.RowProperty); }
            set
            {
                SetValue(Grid.RowProperty, value);
            }
        }


        public static readonly StyledProperty<int> RowSpanProperty =
            AvaloniaProperty.Register<FormField, int>(nameof(RowSpan));

        /// <summary>
        /// quick set grid row span
        /// </summary>
        public int RowSpan
        {
            get { return GetValue(Grid.RowSpanProperty); }
            set
            {
                //SetValue(RowSpanProperty, value); 
                SetValue(Grid.RowSpanProperty, value);
            }
        }


        #endregion

        #region column
        /// <summary>
        /// Defines the <see cref="Column"/> property.
        /// </summary>
        public static readonly StyledProperty<int> ColumnProperty =
            AvaloniaProperty.Register<FormField, int>(nameof(Column));

        /// <summary>
        /// quick set grid column
        /// </summary>
        public int Column
        {
            get { return GetValue(Grid.ColumnProperty); }
            set { SetValue(Grid.ColumnProperty, value); }
        }

        public static readonly StyledProperty<int> ColumnSpanProperty =
            AvaloniaProperty.Register<FormField, int>(nameof(ColumnSpan));

        /// <summary>
        /// quick set grid column span
        /// </summary>
        public int ColumnSpan
        {
            get => GetValue(Grid.ColumnSpanProperty);
            set => SetValue(Grid.ColumnSpanProperty, value);
        }
        #endregion

        #endregion

        #region gap
        /// <summary>
        /// gap
        /// </summary>
        public GridLength Gap
        {
            get => GetGap(this);
            set => SetGap(this, value);
        }
        public static GridLength GetGap(AvaloniaObject obj)
        {
            return (GridLength)obj.GetValue(GapProperty);
        }

        public static void SetGap(AvaloniaObject obj, GridLength value)
        {
            obj.SetValue(GapProperty, value);
        }

        public static readonly AttachedProperty<GridLength> GapProperty =
            AvaloniaProperty.RegisterAttached<FormField, AvaloniaObject, GridLength>(nameof(Gap), new GridLength(2), true);

        #endregion

        #region header alilgnment
        public HorizontalAlignment HorizontalHeaderAlignment
        {
            get => GetHorizontalHeaderAlignment(this);
            set => SetHorizontalHeaderAlignment(this, value);
        }

        public static HorizontalAlignment GetHorizontalHeaderAlignment(AvaloniaObject obj)
        {
            return obj.GetValue(HorizontalHeaderAlignmentProperty);
        }

        public static void SetHorizontalHeaderAlignment(AvaloniaObject obj, HorizontalAlignment value)
        {
            obj.SetValue(HorizontalHeaderAlignmentProperty, value);
        }

        public static readonly AttachedProperty<HorizontalAlignment> HorizontalHeaderAlignmentProperty =
            AvaloniaProperty.RegisterAttached<FormField, AvaloniaObject, HorizontalAlignment>(
                nameof(HorizontalHeaderAlignment), HorizontalAlignment.Left, true);

        public VerticalAlignment VerticalHeaderAlignment
        {
            get => GetVerticalHeaderAlignment(this);
            set => SetVerticalHeaderAlignment(this, value);
        }

        public static VerticalAlignment GetVerticalHeaderAlignment(AvaloniaObject obj)
        {
            return obj.GetValue(VerticalHeaderAlignmentProperty);
        }
        public static void SetVerticalHeaderAlignment(AvaloniaObject obj, VerticalAlignment value)
        {
            obj.SetValue(VerticalHeaderAlignmentProperty, value);
        }

        public static readonly AttachedProperty<VerticalAlignment> VerticalHeaderAlignmentProperty =
            AvaloniaProperty.RegisterAttached<FormField, AvaloniaObject, VerticalAlignment>(
                nameof(VerticalHeaderAlignment), VerticalAlignment.Center, true);



        public bool NotAlignWidth
        {
            get => GetNotAlignWidth(this);
            set => SetNotAlignWidth(this, value);
        }
        public static bool GetNotAlignWidth(AvaloniaObject obj)
            => obj.GetValue(NotAlignWidthProperty);
        public static void SetNotAlignWidth(AvaloniaObject obj, bool value)
        {
            obj.SetValue(NotAlignWidthProperty, value);
            (obj as FormField)?.UpdateSharedSizeGroup();
        }

        public static readonly AttachedProperty<bool> NotAlignWidthProperty =
            AvaloniaProperty.RegisterAttached<FormField, AvaloniaObject, bool>(nameof(NotAlignWidth));


        public FieldContainerPosition ContainerPosition
        {
            get => GetContainerPosition(this);
            set => SetContainerPosition(this, value);
        }
        public static FieldContainerPosition GetContainerPosition(AvaloniaObject obj)
        {
            return obj.GetValue(ContainerPositionProperty);
        }

        public static void SetContainerPosition(AvaloniaObject obj, FieldContainerPosition value)
        {
            obj.SetValue(ContainerPositionProperty, value);
        }

        public static readonly AttachedProperty<FieldContainerPosition> ContainerPositionProperty =
            AvaloniaProperty.RegisterAttached<FormField, AvaloniaObject, FieldContainerPosition>(nameof(ContainerPosition));
        #endregion

        #region set shared size parent
        public AvaloniaObject ShareSizeParent
        {
            get => GetShareSizeParent(this);
            set => SetShareSizeParent(this, value);
        }

        public static AvaloniaObject GetShareSizeParent(AvaloniaObject obj)
        {
            return obj.GetValue(ShareSizeParentProperty);
        }

        public static void SetShareSizeParent(AvaloniaObject obj, AvaloniaObject value)
        {
            obj.SetValue(ShareSizeParentProperty, value);
            switch (obj)
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

        public static readonly AttachedProperty<AvaloniaObject> ShareSizeParentProperty =
            AvaloniaProperty.RegisterAttached<FormField, AvaloniaObject, AvaloniaObject>(nameof(ShareSizeParent));


        private AvaloniaObject InnerShareSizeParent =>
            ShareSizeParent ??
            Parent?.GetValue(ShareSizeParentProperty) ??
            GetPanelParent(Parent);

        private AvaloniaObject GetPanelParent(IControl control)
        {
            return this.GetLogicalAncestors().FirstOrDefault(x => x is Panel or ItemsControl) as AvaloniaObject;
        }

        #endregion

        #endregion
    }
    public enum FieldContainerPosition
    {
        Right,
        Bottom
    }
}
