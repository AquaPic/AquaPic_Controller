#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using Gtk;
using GoodtimeDevelopment.Utilites;
using AquaPic.DataLogging;

namespace GoodtimeDevelopment.TouchWidget
{
    public class TouchLinePlot : EventBox
    {
        uint timerId;
        const int graphWidth = 240;

        private CircularBuffer<LogEntry> _dataPoints;
        public CircularBuffer<LogEntry> dataPoints {
            get {
                return _dataPoints;
            }
        }

        private CircularBuffer<LogEntry> _eventPoints;
        public CircularBuffer<LogEntry> eventPoints {
            get {
                return _eventPoints;
            }
        }

        public Dictionary<string, TouchColor> eventColors;

        private TouchLinePlotPointPixelDifference _pointSpacing;
        public TouchLinePlotPointPixelDifference pointSpacing {
            get {
                return _pointSpacing;
            }
            set {
                _pointSpacing = value;

                _dataPoints.maxSize = maxDataPoints;
                _eventPoints.maxSize = maxDataPoints;
            }
        }

        public int maxDataPoints {
            get {
                return graphWidth / (int)_pointSpacing;
            }
        }

        public double rangeMargin;
        public TouchLinePlotPointTimeDifference timeSpan;
        public TouchLinePlotStartingPoint startingPoint;

        public TouchLinePlot () {
            Visible = true;
            VisibleWindow = false;
            SetSizeRequest (graphWidth + 8, 60);

            _pointSpacing = TouchLinePlotPointPixelDifference.One;
            _dataPoints = new CircularBuffer<LogEntry> (maxDataPoints);
            _eventPoints = new CircularBuffer<LogEntry> (maxDataPoints);
            eventColors = new Dictionary<string, TouchColor> ();

            rangeMargin = 3;
            timeSpan = TouchLinePlotPointTimeDifference.Seconds1;
            startingPoint = new TouchLinePlotStartingPoint ();

            ExposeEvent += OnExpose;

            timerId = GLib.Timeout.Add (1000, OnTimer);
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int top = Allocation.Top;
                int left = Allocation.Left;
                int height = Allocation.Height;
                int bottom = Allocation.Bottom;
                int width = Allocation.Width;
                var now = DateTime.Now;

                cr.Rectangle (left + 8, top, graphWidth, height);
                TouchColor.SetSource (cr, "grey3", 0.15f);
                cr.Fill ();

                //Value points
                if (_dataPoints.count > 0) {
                    var valueBuffer = _dataPoints.ToArray ();

                    var min = valueBuffer.Min (entry => entry.value);
                    var max = valueBuffer.Max (entry => entry.value);

                    if ((max - min) < rangeMargin) {
                        max += (rangeMargin / 2);
                        min -= (rangeMargin / 2);
                    }

                    Array.Reverse (valueBuffer);

                    TouchColor.SetSource (cr, "pri");

                    var y = valueBuffer[0].value.Map (min, max, bottom - 4, top + 4);
                    double x = left + 8;
                    var pointDifference = now.Subtract (valueBuffer[0].dateTime).TotalSeconds / (double)PointTimeDifferenceToSeconds ();
                    if (pointDifference > 2) {
                        x += pointDifference * (double)_pointSpacing;
                    }
                    cr.MoveTo (x, y);

                    for (int i = 1; i < valueBuffer.Length; ++i) {
                        y = valueBuffer[i].value.Map (min, max, bottom - 4, top + 4);
                        x = left + 8;

                        pointDifference = now.Subtract (valueBuffer[i].dateTime).TotalSeconds / (double)PointTimeDifferenceToSeconds ();
                        x += pointDifference * (double)_pointSpacing;

                        var previousDifference = valueBuffer[i - 1].dateTime.Subtract (valueBuffer[i].dateTime).TotalSeconds / (double)PointTimeDifferenceToSeconds ();
                        if (previousDifference > 2) {
                            cr.Stroke ();

                            if (x > (left + width)) {
                                break;
                            } else {
                                cr.MoveTo (x, y);
                            }
                        } else {
                            if (x > (left + width)) {
                                x = left + width;
                                cr.LineTo (x, y);
                                break;
                            } else {
                                cr.LineTo (x, y);
                            }
                        }
                    }

                    cr.Stroke ();

                    var textRender = new TouchText ();
                    textRender.textWrap = TouchTextWrap.Shrink;
                    textRender.alignment = TouchAlignment.Right;
                    textRender.font.color = "white";

                    textRender.text = Math.Ceiling (max).ToString ();
                    textRender.Render (this, left - 9, top - 2, 16);

                    textRender.text = Math.Floor (min).ToString ();
                    textRender.Render (this, left - 9, bottom - 16, 16);
                }

                //Event points
                if (_eventPoints.count > 0) {
                    var eventBuffer = _eventPoints.ToArray ();
                    Array.Reverse (eventBuffer);
                    for (int i = 0; i < eventBuffer.Length; i++) {
                        double x = left + 8;
                        x += (now.Subtract (eventBuffer[i].dateTime).TotalSeconds / (double)PointTimeDifferenceToSeconds ()) * (double)_pointSpacing;
                        if (x > (left + width)) {
                            break;
                        }

                        cr.Rectangle (x, top, (int)_pointSpacing, height);

                        if (eventColors.ContainsKey (eventBuffer[i].eventType)) {
                            eventColors[eventBuffer[i].eventType].SetSource (cr);
                        } else {
                            TouchColor.SetSource (cr, "seca", .375);
                        }

                        cr.Fill ();
                    }
                }
            }
        }

        public void LinkDataLogger (IDataLogger logger) {
            var endSearchTime = DateTime.Now.Subtract (new TimeSpan (0, 0, maxDataPoints * PointTimeDifferenceToSeconds ()));

            logger.ValueLogEntryAddedEvent += OnValueLogEntryAdded;
            var valueEntries = logger.GetValueEntries (maxDataPoints, PointTimeDifferenceToSeconds (), endSearchTime);
            _dataPoints.AddRange (valueEntries);

            logger.EventLogEntryAddedEvent += OnEventLogEntryAdded;
            var eventEntries = logger.GetEventEntries (maxDataPoints, endSearchTime);
            _eventPoints.AddRange (eventEntries);
        }

        public void UnLinkDataLogger (IDataLogger logger) {
            logger.ValueLogEntryAddedEvent -= OnValueLogEntryAdded;
            logger.EventLogEntryAddedEvent -= OnEventLogEntryAdded;
        }

        public void OnValueLogEntryAdded (object sender, DataLogEntryAddedEventArgs args) {
            if (_dataPoints.count > 0) {
                var previous = _dataPoints[_dataPoints.count - 1].dateTime;
                var totalSeconds = args.entry.dateTime.Subtract (previous).TotalSeconds.ToInt ();
                var secondTimeSpan = PointTimeDifferenceToSeconds ();
                if (totalSeconds >= secondTimeSpan) {
                    _dataPoints.Add (new LogEntry (args.entry));
                }
            } else {
                _dataPoints.Add (new LogEntry (args.entry));
            }
        }

        public void OnEventLogEntryAdded (object sender, DataLogEntryAddedEventArgs args) {
            _eventPoints.Add (new LogEntry (args.entry));
        }

        public int PointTimeDifferenceToSeconds () {
            switch (timeSpan) {
            case TouchLinePlotPointTimeDifference.Seconds1:
                return 1;
            case TouchLinePlotPointTimeDifference.Seconds5:
                return 5;
            case TouchLinePlotPointTimeDifference.Seconds10:
                return 10;
            case TouchLinePlotPointTimeDifference.Minute1:
                return 60;
            case TouchLinePlotPointTimeDifference.Minute2:
                return 120;
            case TouchLinePlotPointTimeDifference.Minute5:
                return 300;
            case TouchLinePlotPointTimeDifference.Minute10:
                return 600;
            default:
                return 1;
            }
        }

        protected bool OnTimer () {
            QueueDraw ();
            return true;
        }
    }

    public enum TouchLinePlotPointTimeDifference
    {
        [Description ("One second")]
        Seconds1,

        [Description ("Five seconds")]
        Seconds5,

        [Description ("Ten seconds")]
        Seconds10,

        [Description ("One minute")]
        Minute1,

        [Description ("Two minutes")]
        Minute2,

        [Description ("Five minutes")]
        Minute5,

        [Description ("Ten minutes")]
        Minute10
    }

    //240 is a high composite number 
    //factors are 1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 16, 20, 24, 30, 40, 48, 60, 80, 120, and 240
    public enum TouchLinePlotPointPixelDifference
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Eight = 8,
        Ten = 10,
        Twelve = 12,
        Fifteen = 15,
        Sixteen = 16,
        Twenty = 20,
        TwentyFour = 24,
        Thirty = 30,
        Forty = 40,
        FortyEight = 48,
        Sixty = 60,
        Eighty = 80,
        OneTwenty = 120
    }

    public class TouchLinePlotStartingPoint
    {
        public TouchLinePlotStartTime startPoint;
        public DateTime pastTimePoint;

        public TouchLinePlotStartingPoint () {
            startPoint = TouchLinePlotStartTime.Now;
            pastTimePoint = DateTime.MinValue;
        }
    }

    public enum TouchLinePlotStartTime
    {
        Now,
        PastTimePoint
    }
}

