namespace SEToolbox.Services
{
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    internal class ListBoxDropAdorner : Adorner
    {
        private AdornerLayer adornerLayer;

        public bool IsAboveElement { get; set; }

        public ListBoxDropAdorner(UIElement adornedElement, AdornerLayer adornerLayer)
            : base(adornedElement)
        {
            this.adornerLayer = adornerLayer;
            this.adornerLayer.Add(this);
        }

        /// <summary>
        /// Update UI
        /// </summary>
        internal void Update()
        {
            this.adornerLayer.Update(this.AdornedElement);
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public void Remove()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            //double width = this.AdornedElement.DesiredSize.Width;
            //double height = this.AdornedElement.DesiredSize.Height;

            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Red);
            renderBrush.Opacity = 0.5;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.White), 1.5);
            double renderRadius = 5.0;

            if (this.IsAboveElement)
            {
                drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
                drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            }
            else
            {
                drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
                drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
            }
        }
    }
}
