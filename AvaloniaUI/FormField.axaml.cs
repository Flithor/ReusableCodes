using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;

using JetBrains.Annotations;

using ReactiveUI;

namespace AvaloniaUILib.Controls
{
    [PseudoClasses(":content-right", ":content-bottom")]
    public partial class FormField : HeaderedContentControl, IHeadered, IStyleable
    {
        public FormField()
        {
            PseudoClasses.Add(":content-right");
            PseudoClasses.Add(":content-bottom");

            PseudoClasses.Set(":content-right",
                this.WhenAny(_this => _this.ContainerPosition, p => p.Value == FieldContainerPosition.Right));
            PseudoClasses.Set(":content-bottom",
                this.WhenAny(_this => _this.ContainerPosition, p => p.Value == FieldContainerPosition.Bottom));
        }

        private Grid? contentGrid;

        private ColumnDefinition PART_FieldWidth;
        private ColumnDefinition HorizontalGap;
        private IDisposable HorizontalGapBinding;
        private ColumnDefinition HorizontalContainer;
        private RowDefinition PART_FieldHigth;
        private RowDefinition VerticalGap;
        private IDisposable VerticalGapBinding;
        private RowDefinition VerticalContainer;

        static FormField()
        {
            IsTabStopProperty.OverrideMetadata(typeof(FormField), new StyledPropertyMetadata<bool>(false));
        }

        public Type StyleKey { get; } = typeof(FormField);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            contentGrid = e.NameScope.Get<Grid>("Grid_Content");
            if (contentGrid?.ColumnDefinitions.Count == 3)
            {
                PART_FieldWidth = contentGrid.ColumnDefinitions[0];
                HorizontalGap = contentGrid.ColumnDefinitions[1];
                HorizontalContainer = contentGrid.ColumnDefinitions[2];
            }
            if (contentGrid?.RowDefinitions.Count == 3)
            {
                PART_FieldHigth = contentGrid.RowDefinitions[0];
                VerticalGap = contentGrid.RowDefinitions[1];
                VerticalContainer = contentGrid.RowDefinitions[2];
            }
            UpdateSharedSizeGroup();
        }

        /// <summary>
        /// Reset shared size group ID
        /// </summary>
        private void UpdateSharedSizeGroup()
        {
            if (contentGrid == null) return;

            var shareSizeParent = InnerShareSizeParent;
            if (shareSizeParent == null) return;

            if (!shareSizeParent.GetValue(Grid.IsSharedSizeScopeProperty))
                shareSizeParent.SetValue(Grid.IsSharedSizeScopeProperty, true);

            if (ContainerPosition == FieldContainerPosition.Right && PART_FieldWidth != null)
            {
                if (NotAlignWidth)
                    PART_FieldWidth.SharedSizeGroup = null;
                else
                {
                    var columnNoTemp = GetValue(Grid.ColumnProperty);
                    var columnNo = Equals(columnNoTemp, AvaloniaProperty.UnsetValue) ? 0 : columnNoTemp;

                    if (GetShareSizeColumns(shareSizeParent) is { } shareSizeColumns)
                        columnNo = shareSizeColumns.GetShareIndex(columnNo);

                    PART_FieldWidth.SharedSizeGroup = $"{shareSizeParent.GetType().Name}{shareSizeParent.GetHashCode()}_C{columnNo}";
                }

                VerticalGapBinding?.Dispose();
                HorizontalGapBinding =
                    HorizontalGap.Bind(ColumnDefinition.WidthProperty, this.GetBindingObservable(GapProperty));

                //Reset columns for refresh layout
                contentGrid.ColumnDefinitions.Clear();
                contentGrid.ColumnDefinitions.AddRange(new[] { PART_FieldWidth, HorizontalGap, HorizontalContainer });
            }
            else if (ContainerPosition == FieldContainerPosition.Bottom && PART_FieldHigth != null)
            {
                if (NotAlignWidth)
                    PART_FieldHigth.SharedSizeGroup = null;
                else
                {
                    var rowNoTemp = GetValue(Grid.RowProperty);
                    var rowNo = Equals(rowNoTemp, AvaloniaProperty.UnsetValue) ? 0 : rowNoTemp;

                    if (GetShareSizeRows(shareSizeParent) is { } shareSizeRows)
                        rowNo = shareSizeRows.GetShareIndex(rowNo);

                    PART_FieldHigth.SharedSizeGroup = $"{shareSizeParent.GetType().Name}{shareSizeParent.GetHashCode()}_R{rowNo}";
                }
                HorizontalGapBinding?.Dispose();
                VerticalGapBinding =
                    VerticalGap.Bind(RowDefinition.HeightProperty, this.GetBindingObservable(GapProperty));

                //Reset columns for refresh layout
                contentGrid.RowDefinitions.Clear();
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
        public Control ShareSizeParent
        {
            get => (Control)GetShareSizeParent(this);
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

        public static readonly AttachedProperty<Control> ShareSizeParentProperty =
            AvaloniaProperty.RegisterAttached<FormField, Control, Control>(nameof(ShareSizeParent));


        private Control? InnerShareSizeParent =>
            ShareSizeParent ??
            Parent?.GetValue(ShareSizeParentProperty) ??
            GetPanelParent(Parent);

        private Control? GetPanelParent(IControl? control)
        {
            if (control == null) return null;

            if (control is ItemsControl || control is Panel)
                return (Control)control;
            return control.GetLogicalAncestors().FirstOrDefault(x => x is ItemsControl || x is Panel) as Control;
        }

        #endregion

        #region Specifies the rows and columns that shared width

        public class ShareSizeGroup
        {
            private static readonly char[] splitGroup = { ';' };
            private static readonly char[] splitMember = { ',' };

            private ShareSizeGroup(string shareDefine)
            {
                if (shareDefine == null) return;
                shareGroups = shareDefine.Split(splitGroup, StringSplitOptions.RemoveEmptyEntries).Select(str =>
                        str.Split(splitMember, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray())
                    .ToArray();
            }
            private int[][] shareGroups;

            public int GetShareIndex(int index) => shareGroups.FirstOrDefault(g => g.Contains(index))?.Min() ?? index;

            public static ShareSizeGroup Parse(string s) => new ShareSizeGroup(s);

            public static implicit operator string(ShareSizeGroup group)
            {
                return string.Join(";", group.shareGroups.Select(g => string.Join(",", g)));
            }

            public static implicit operator ShareSizeGroup(string str)
            {
                if (string.IsNullOrEmpty(str)) return null;
                return new ShareSizeGroup(str);
            }
        }
        /// <summary>
        /// Specifies the columns that shared width
        /// </summary>
        public static readonly AttachedProperty<ShareSizeGroup?> ShareSizeColumnsProperty = AvaloniaProperty.RegisterAttached<FormField, IControl, ShareSizeGroup?>(
            "ShareSizeColumns", default, true);
        /// <summary>
        /// Accessor for Attached property <see cref="ShareSizeColumnsProperty"/>.
        /// </summary>
        public static void SetShareSizeColumns(AvaloniaObject element, [CanBeNull] ShareSizeGroup value)
        {
            element.SetValue(ShareSizeColumnsProperty, value);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="ShareSizeColumnsProperty"/>.
        /// </summary>
        [CanBeNull]
        public static ShareSizeGroup GetShareSizeColumns(AvaloniaObject element)
        {
            return element.GetValue(ShareSizeColumnsProperty);
        }

        /// <summary>
        /// Specifies the rows that shared width
        /// </summary>
        public static readonly AttachedProperty<ShareSizeGroup> ShareSizeRowsProperty = AvaloniaProperty.RegisterAttached<FormField, IControl, ShareSizeGroup>(
            "ShareSizeRows", default, true);

        /// <summary>
        /// Accessor for Attached property <see cref="ShareSizeRowsProperty"/>.
        /// </summary>
        public static void SetShareSizeRows(AvaloniaObject element, ShareSizeGroup value)
        {
            element.SetValue(ShareSizeRowsProperty, value);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="ShareSizeRowsProperty"/>.
        /// </summary>
        public static ShareSizeGroup GetShareSizeRows(AvaloniaObject element)
        {
            return element.GetValue(ShareSizeRowsProperty);
        }

        /// <summary>
        /// The margin of header(for string header only)
        /// </summary>
        public Thickness HeaderMargin
        {
            get => GetHeaderMargin(this);
            set => SetHeaderMargin(this, value);
        }

        public static readonly AttachedProperty<Thickness> HeaderMarginProperty =
            AvaloniaProperty.RegisterAttached<IAvaloniaObject, Thickness>("HeaderMargin", typeof(FormField),
                inherits: true);

        public static Thickness GetHeaderMargin(AvaloniaObject element)
        {
            return (Thickness)element.GetValue(HeaderMarginProperty);
        }

        public static void SetHeaderMargin(AvaloniaObject element, Thickness value)
        {
            element.SetValue(HeaderMarginProperty, value);
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
