﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Data;
using HandyControl.Expression.Drawing;
using HandyControl.Tools;

namespace HandyControl.Controls
{
    public class UniformSpacingPanel : Panel
    {
        private Orientation _orientation;

        public UniformSpacingPanel()
        {
            _orientation = Orientation.Horizontal;
        }

        public static readonly DependencyProperty OrientationProperty =
            StackPanel.OrientationProperty.AddOwner(typeof(UniformSpacingPanel),
                new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (UniformSpacingPanel) d;
            p._orientation = (Orientation) e.NewValue;
        }

        public Orientation Orientation
        {
            get => (Orientation) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty ChildWrappingProperty = DependencyProperty.Register(
            "ChildWrapping", typeof(VisualWrapping), typeof(UniformSpacingPanel),
            new FrameworkPropertyMetadata(default(VisualWrapping), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public VisualWrapping ChildWrapping
        {
            get => (VisualWrapping) GetValue(ChildWrappingProperty);
            set => SetValue(ChildWrappingProperty, value);
        }

        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
            "Spacing", typeof(double), typeof(UniformSpacingPanel),
            new FrameworkPropertyMetadata(ValueBoxes.Double0Box, FrameworkPropertyMetadataOptions.AffectsMeasure),
            ValidateHelper.IsInRangeOfPosDoubleIncludeZero);

        public double Spacing
        {
            get => (double) GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            "ItemWidth", typeof(double), typeof(UniformSpacingPanel),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure),
            IsWidthHeightValid);

        [TypeConverter(typeof(LengthConverter))]
        public double ItemWidth
        {
            get => (double) GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight", typeof(double), typeof(UniformSpacingPanel),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure),
            IsWidthHeightValid);

        [TypeConverter(typeof(LengthConverter))]
        public double ItemHeight
        {
            get => (double) GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        private static bool IsWidthHeightValid(object value)
        {
            var v = (double) value;
            return double.IsNaN(v) || v >= 0.0d && !double.IsPositiveInfinity(v);
        }

        public static readonly DependencyProperty ItemHorizontalAlignmentProperty = DependencyProperty.Register(
            "ItemHorizontalAlignment", typeof(HorizontalAlignment?), typeof(UniformSpacingPanel),
            new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public HorizontalAlignment? ItemHorizontalAlignment
        {
            get => (HorizontalAlignment?) GetValue(ItemHorizontalAlignmentProperty);
            set => SetValue(ItemHorizontalAlignmentProperty, value);
        }

        public static readonly DependencyProperty ItemVerticalAlignmentProperty = DependencyProperty.Register(
            "ItemVerticalAlignment", typeof(VerticalAlignment?), typeof(UniformSpacingPanel),
            new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public VerticalAlignment? ItemVerticalAlignment
        {
            get => (VerticalAlignment?) GetValue(ItemVerticalAlignmentProperty);
            set => SetValue(ItemVerticalAlignmentProperty, value);
        }

        private void ArrangeWrapLine(double v, double lineV, int start, int end, bool useItemU, double itemU, double spacing)
        {
            double u = 0;
            var isHorizontal = _orientation == Orientation.Horizontal;

            var children = InternalChildren;
            for (var i = start; i < end; i++)
            {
                var child = children[i];
                if (child == null) continue;

                var childSize = new UVSize(_orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                var layoutSlotU = useItemU ? itemU : childSize.U;

                child.Arrange(isHorizontal ? new Rect(u, v, layoutSlotU, lineV) : new Rect(v, u, lineV, layoutSlotU));

                if (layoutSlotU > 0)
                {
                    u += layoutSlotU + spacing;
                }
            }
        }

        private void ArrangeLine(double lineV, bool useItemU, double itemU, double spacing)
        {
            double u = 0;
            var isHorizontal = _orientation == Orientation.Horizontal;

            var children = InternalChildren;
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child == null) continue;

                var childSize = new UVSize(_orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                var layoutSlotU = useItemU ? itemU : childSize.U;

                child.Arrange(isHorizontal ? new Rect(u, 0, layoutSlotU, lineV) : new Rect(0, u, lineV, layoutSlotU));

                if (layoutSlotU > 0)
                {
                    u += layoutSlotU + spacing;
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var curLineSize = new UVSize(_orientation);
            var panelSize = new UVSize(_orientation);
            var uvConstraint = new UVSize(_orientation, constraint.Width, constraint.Height);
            var itemWidth = ItemWidth;
            var itemHeight = ItemHeight;
            var itemWidthSet = !double.IsNaN(itemWidth);
            var itemHeightSet = !double.IsNaN(itemHeight);
            var spacing = Spacing;
            var childWrapping = ChildWrapping;
            var itemHorizontalAlignment = ItemHorizontalAlignment;
            var itemVerticalAlignment = ItemVerticalAlignment;
            var itemHorizontalAlignmentSet = itemHorizontalAlignment != null;
            var itemVerticalAlignmentSet = ItemVerticalAlignment != null;

            var childConstraint = new Size(
                itemWidthSet ? itemWidth : constraint.Width,
                itemHeightSet ? itemHeight : constraint.Height);

            var children = InternalChildren;
            var isFirst = true;

            if (childWrapping == VisualWrapping.Wrap)
            {
                for (int i = 0, count = children.Count; i < count; i++)
                {
                    var child = children[i];
                    if (child == null) continue;

                    if (itemHorizontalAlignmentSet)
                    {
                        child.SetCurrentValue(HorizontalAlignmentProperty, itemHorizontalAlignment);
                    }

                    if (itemVerticalAlignmentSet)
                    {
                        child.SetCurrentValue(VerticalAlignmentProperty, itemVerticalAlignment);
                    }

                    child.Measure(childConstraint);

                    var sz = new UVSize(
                        _orientation,
                        itemWidthSet ? itemWidth : child.DesiredSize.Width,
                        itemHeightSet ? itemHeight : child.DesiredSize.Height);

                    if (MathHelper.GreaterThan(curLineSize.U + sz.U + spacing, uvConstraint.U))
                    {
                        panelSize.U = Math.Max(curLineSize.U, panelSize.U);
                        panelSize.V += curLineSize.V + spacing;
                        curLineSize = sz;

                        if (MathHelper.GreaterThan(sz.U, uvConstraint.U))
                        {
                            panelSize.U = Math.Max(sz.U, panelSize.U);
                            panelSize.V += sz.V + spacing;
                            curLineSize = new UVSize(_orientation);
                        }

                        isFirst = true;
                    }
                    else
                    {
                        curLineSize.U += isFirst ? sz.U : sz.U + spacing;
                        curLineSize.V = Math.Max(sz.V, curLineSize.V);

                        isFirst = false;
                    }
                }
            }
            else
            {
                var layoutSlotSize = constraint;

                if (_orientation == Orientation.Horizontal)
                {
                    layoutSlotSize.Width = double.PositiveInfinity;
                }
                else
                {
                    layoutSlotSize.Height = double.PositiveInfinity;
                }

                for (int i = 0, count = children.Count; i < count; ++i)
                {
                    var child = children[i];
                    if (child == null) continue;

                    if (itemHorizontalAlignmentSet)
                    {
                        child.SetCurrentValue(HorizontalAlignmentProperty, itemHorizontalAlignment);
                    }

                    if (itemVerticalAlignmentSet)
                    {
                        child.SetCurrentValue(VerticalAlignmentProperty, itemVerticalAlignment);
                    }

                    child.Measure(layoutSlotSize);

                    var sz = new UVSize(
                        _orientation,
                        itemWidthSet ? itemWidth : child.DesiredSize.Width,
                        itemHeightSet ? itemHeight : child.DesiredSize.Height);

                    curLineSize.U += isFirst ? sz.U : sz.U + spacing;
                    curLineSize.V = Math.Max(sz.V, curLineSize.V);

                    isFirst = false;
                }
            }

            panelSize.U = Math.Max(curLineSize.U, panelSize.U);
            panelSize.V += curLineSize.V;

            return new Size(panelSize.Width, panelSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var firstInLine = 0;
            var itemWidth = ItemWidth;
            var itemHeight = ItemHeight;
            double accumulatedV = 0;
            var itemU = _orientation == Orientation.Horizontal ? itemWidth : itemHeight;
            var curLineSize = new UVSize(_orientation);
            var uvFinalSize = new UVSize(_orientation, finalSize.Width, finalSize.Height);
            var itemWidthSet = !double.IsNaN(itemWidth);
            var itemHeightSet = !double.IsNaN(itemHeight);
            var useItemU = _orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet;
            var spacing = Spacing;
            var childWrapping = ChildWrapping;

            var children = InternalChildren;
            var isFirst = true;

            if (childWrapping == VisualWrapping.Wrap)
            {
                for (int i = 0, count = children.Count; i < count; i++)
                {
                    var child = children[i];
                    if (child == null) continue;

                    var sz = new UVSize(
                        _orientation,
                        itemWidthSet ? itemWidth : child.DesiredSize.Width,
                        itemHeightSet ? itemHeight : child.DesiredSize.Height);

                    if (MathHelper.GreaterThan(curLineSize.U + sz.U + spacing, uvFinalSize.U))
                    {
                        ArrangeWrapLine(accumulatedV, curLineSize.V, firstInLine, i, useItemU, itemU, spacing);

                        accumulatedV += curLineSize.V + spacing;
                        curLineSize = sz;

                        if (MathHelper.GreaterThan(sz.U, uvFinalSize.U))
                        {
                            ArrangeWrapLine(accumulatedV, sz.V, i, ++i, useItemU, itemU, spacing);

                            accumulatedV += sz.V + spacing;
                            curLineSize = new UVSize(_orientation);
                        }

                        firstInLine = i;
                        isFirst = true;
                    }
                    else
                    {
                        curLineSize.U += isFirst ? sz.U : sz.U + spacing;
                        curLineSize.V = Math.Max(sz.V, curLineSize.V);

                        isFirst = false;
                    }
                }

                if (firstInLine < children.Count)
                {
                    ArrangeWrapLine(accumulatedV, curLineSize.V, firstInLine, children.Count, useItemU, itemU, spacing);
                }
            }
            else
            {
                ArrangeLine(uvFinalSize.V, useItemU, itemU, spacing);
            }

            return finalSize;
        }

        private struct UVSize
        {
            internal UVSize(Orientation orientation, double width, double height)
            {
                U = V = 0d;
                _orientation = orientation;
                Width = width;
                Height = height;
            }

            internal UVSize(Orientation orientation)
            {
                U = V = 0d;
                _orientation = orientation;
            }

            public double U { get; set; }

            public double V { get; set; }

            private readonly Orientation _orientation;

            public double Width
            {
                get => _orientation == Orientation.Horizontal ? U : V;
                private set
                {
                    if (_orientation == Orientation.Horizontal)
                    {
                        U = value;
                    }
                    else
                    {
                        V = value;
                    }
                }
            }

            public double Height
            {
                get => _orientation == Orientation.Horizontal ? V : U;
                private set
                {
                    if (_orientation == Orientation.Horizontal)
                    {
                        V = value;
                    }
                    else
                    {
                        U = value;
                    }
                }
            }
        }
    }
}
