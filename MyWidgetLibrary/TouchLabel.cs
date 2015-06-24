﻿using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchLabel : EventBox
    {
        public MyText render;

        public string text {
            get {
                return render.text;
            }
            set {
                render.text = value;
            }
        }

        public MyColor textColor {
            get {
                return render.font.color;
            }
            set {
                render.font.color = value;
            }
        }

        public int textSize {
            get {
                return render.font.size;
            }
            set {
                render.font.size = value;
            }
        }

        public MyAlignment textAlignment {
            get {
                return render.alignment;
            }
            set {
                render.alignment = value;
            }
        }

        public TouchLabel () {
            this.Visible = true;
            this.VisibleWindow = false;

            render = new MyText (string.Empty);

            HeightRequest = 30;
            WidthRequest = 200;

            this.ExposeEvent += OnExpose;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            render.Render (this, Allocation.Left, Allocation.Top, Allocation.Width);
        }
    }
}

