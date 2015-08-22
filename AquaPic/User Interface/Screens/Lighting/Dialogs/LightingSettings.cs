﻿using System;
using System.IO;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class LightingSettings : TouchSettingsDialog
    {
        public LightingSettings () : base ("Lighting") {
            SaveEvent += OnSave;

            var t = new SettingTextBox ();
            t.text = "Latitude";
            t.textBox.text = Lighting.latitude.ToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToDouble (args.text);
                } catch {
                    TouchMessageBox.Show ("Improper latitude format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Longitude";
            t.textBox.text = Lighting.longitude.ToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToDouble (args.text);
                } catch {
                    TouchMessageBox.Show ("Improper Longitude format");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Default Rise";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.defaultSunRise.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = ToTime (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    TouchMessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Default Set";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.defaultSunSet.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = ToTime (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    TouchMessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Min Sunrise";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.minSunRise.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = ToTime (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    TouchMessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Max Sunrise";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.maxSunRise.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = ToTime (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    TouchMessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Min Sunset";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.minSunSet.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = ToTime (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    TouchMessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            t = new SettingTextBox ();
            t.text = "Max Sunset";
            t.textBox.includeTimeFunctions = true;
            t.textBox.text = Lighting.maxSunSet.TimeToString ();
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Time time = ToTime (args.text);
                    args.text = time.TimeToString ();
                } catch {
                    TouchMessageBox.Show ("Improper time format, ##:##");
                    args.keepText = false;
                }
            };
            AddSetting (t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            Lighting.latitude = Convert.ToDouble (((SettingTextBox)settings ["Latitude"]).textBox.text);
            Lighting.longitude = Convert.ToDouble (((SettingTextBox)settings ["Longitude"]).textBox.text);
            Lighting.defaultSunRise = ToTime (((SettingTextBox)settings ["Default Sunrise"]).textBox.text);
            Lighting.defaultSunSet = ToTime (((SettingTextBox)settings ["Default Sunset"]).textBox.text);
            Lighting.minSunRise = ToTime (((SettingTextBox)settings ["Min Sunrise"]).textBox.text);
            Lighting.maxSunRise = ToTime (((SettingTextBox)settings ["Max Sunrise"]).textBox.text);
            Lighting.minSunSet = ToTime (((SettingTextBox)settings ["Min Sunset"]).textBox.text);
            Lighting.maxSunSet = ToTime (((SettingTextBox)settings ["Max Sunset"]).textBox.text);

            JObject jo = new JObject ();

            jo.Add (new JProperty ("latitude", Lighting.latitude.ToString ()));
            jo.Add (new JProperty ("longitude", Lighting.longitude.ToString ()));

            jo.Add (new JProperty ("defaultSunRise", 
                new JObject (
                    new JProperty ("hour", Lighting.defaultSunRise.hour.ToString ()), 
                    new JProperty ("minute", Lighting.defaultSunRise.min.ToString ()))));

            jo.Add (new JProperty ("defaultSunSet", 
                new JObject (
                    new JProperty ("hour", Lighting.defaultSunSet.hour.ToString ()), 
                    new JProperty ("minute", Lighting.defaultSunSet.min.ToString ()))));

            jo.Add (new JProperty ("minSunRise", 
                new JObject (
                    new JProperty ("hour", Lighting.minSunRise.hour.ToString ()), 
                    new JProperty ("minute", Lighting.minSunRise.min.ToString ()))));

            jo.Add (new JProperty ("maxSunRise", 
                new JObject (
                    new JProperty ("hour", Lighting.maxSunRise.hour.ToString ()), 
                    new JProperty ("minute", Lighting.maxSunRise.min.ToString ()))));

            jo.Add (new JProperty ("minSunSet", 
                new JObject (
                    new JProperty ("hour", Lighting.minSunSet.hour.ToString ()), 
                    new JProperty ("minute", Lighting.minSunSet.min.ToString ()))));

            jo.Add (new JProperty ("maxSunSet", 
                new JObject (
                    new JProperty ("hour", Lighting.maxSunSet.hour.ToString ()), 
                    new JProperty ("minute", Lighting.maxSunSet.min.ToString ()))));

            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "lightingProperties.json");

            File.WriteAllText (path, jo.ToString ());

            Lighting.UpdateRiseSetTimes ();

            return true;
        }

        protected Time ToTime (string value) {
            int pos = value.IndexOf (":");

            if ((pos != 1) && (pos != 2))
                throw new Exception ();

            string hourString = value.Substring (0, pos);
            int hour = Convert.ToInt32 (hourString);

            if ((hour < 0) || (hour > 23))
                throw new Exception ();
            
            string minString = value.Substring (pos + 1, 2);
            int min = Convert.ToInt32 (minString);

            if ((min < 0) || (min > 59))
                throw new Exception ();

            pos = value.Length;
            if (pos > 3) {
                string last = value.Substring (pos - 2);
                if (last == "pm") {
                    if ((hour >= 1) && (hour <= 12))
                        hour = (hour + 12) % 24;
                }
            }

            return new Time ((byte)hour, (byte)min);
        }
    }
}

