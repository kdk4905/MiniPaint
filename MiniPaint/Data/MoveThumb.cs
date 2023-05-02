using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MiniPaint.Data
{
    public class MoveThumb : Thumb
    {

        #region 생성자 - MoveThumb

        public MoveThumb() 
        {
            DragDelta += Thumb_DragDelta;
        }
        #endregion

        #region - 썸 드래그 델타 처리하기 - Thumb_DragDelta(sender, e)

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e) 
        {
            UIElement uiElement = sender as UIElement;

            if (uiElement != null) 
            {
                double left = Canvas.GetLeft(uiElement);
                double top =  Canvas.GetTop(uiElement);

                Canvas.SetLeft(uiElement, left + e.HorizontalChange);
                Canvas.SetTop(uiElement, top + e.VerticalChange);
            }
        }
        #endregion

    }
}
