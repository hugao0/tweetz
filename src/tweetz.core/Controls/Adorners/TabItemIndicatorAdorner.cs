﻿using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace tweetz.core.Controls.Adorners
{
    public class TabItemIndicatorAdorner : Adorner
    {
        public TabItemIndicatorAdorner(UIElement adornedElement, Brush brush)
            : base(adornedElement)
        {
            Brush = brush;
        }

        public Brush Brush { get; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            const int radius = 3;
            var size = AdornedElement.DesiredSize;

            var pen = new Pen(Brush, 1);
            pen.Freeze();

            var point = new Point(size.Width / 2 + radius * 3, radius + 1);
            drawingContext?.DrawEllipse(Brush, pen, point, radius, radius);
        }
    }
}