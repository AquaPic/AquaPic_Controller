﻿using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public delegate void TextChangedHandler (object sender, TextChangedEventArgs args);

    public class TextChangedEventArgs : EventArgs {
        public string text;

        public TextChangedEventArgs (string text) {
            this.text = text;
        }
    }

    public class TouchTextBox : EventBox
    {
        public string text;
        public MyColor textColor;
        public int textSize;
        public MyAlignment textAlignment;
        public bool enableTouch;
        public TextChangedHandler TextChangedEvent;

        public TouchTextBox () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.text = null;
            this.textColor = new MyColor ("black");
            this.textSize = 11;
            this.textAlignment = MyAlignment.Left;

            this.WidthRequest = 100;
            this.HeightRequest = 30;

            this.ExposeEvent += OnExpose;
            ButtonReleaseEvent += OnTouchButtonRelease;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                WidgetGlobal.DrawRoundedRectangle (cr, left, top, width, height, 3);
                cr.SetSourceRGB (0.85, 0.85, 0.85);
                cr.FillPreserve ();
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.LineWidth = 0.75;
                cr.Stroke ();

//                Pango.Layout l = new Pango.Layout (PangoContext);
//                l.Width = Pango.Units.FromPixels (width);
//                l.Wrap = Pango.WrapMode.Word;
//                if (textAlignment == MyAlignment.Left) {
//                    l.Alignment = Pango.Alignment.Left;
//                    left += 5;
//                } else if (textAlignment == MyAlignment.Right) {
//                    l.Alignment = Pango.Alignment.Right;
//                    l.Width = Pango.Units.FromPixels (width - 5);
//                } else if (textAlignment == MyAlignment.Center)
//                    l.Alignment = Pango.Alignment.Center;
//                l.SetMarkup ("<span color=\"" + textColor.ToHTML () + "\">" 
//                    + text 
//                    + "</span>");
//                l.FontDescription = Pango.FontDescription.FromString ("Courier New " + textSize.ToString ());
//                GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), left, top + 6, l);
//                l.Dispose ();

                MyText t = text;
                t.font.color = textColor;
                t.font.size = textSize;
                t.alignment = textAlignment;
                t.textWrap = MyTextWrap.Shrink;
                t.Render (this, left + 3, top, width - 6, height);
            }
        }

        protected void OnTouchButtonRelease (object o, ButtonReleaseEventArgs args) {
            if (enableTouch) {
                TouchNumberInput t = new TouchNumberInput ();
                t.NumberSetEvent += (value) => {
                    if (!string.IsNullOrWhiteSpace (value)) {
                        this.text = value;

                        if (TextChangedEvent != null)
                            TextChangedEvent (this, new TextChangedEventArgs (text));
                    }
                };
                
                t.Run ();
                t.Destroy ();
                QueueDraw ();
            }
        }
    }
}

