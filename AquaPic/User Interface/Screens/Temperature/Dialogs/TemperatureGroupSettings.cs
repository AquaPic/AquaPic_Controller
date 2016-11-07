﻿using System;
using System.IO;
using Cairo;
using Gtk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TouchWidgetLibrary;
using AquaPic.Modules;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class TemperatureGroupSettings : TouchSettingsDialog
    {
        string groupName;
        public string temperatureGroupName {
            get {
                return groupName;
            }
        }

        public TemperatureGroupSettings (string name, bool includeDelete) 
            : base (name + " Temperature", includeDelete) 
        {
            groupName = name;
            
            SaveEvent += OnSave;
            DeleteButtonEvent += OnDelete;

            var t = new SettingsTextBox ();
            t.text = "Name";
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = groupName;
                t.textBox.enableTouch = false;
                t.textBox.TextChangedEvent += (sender, args) => {
                    MessageBox.Show ("Can not change temperature group name during runtime");
                    args.keepText = false;
                };
            } else {
                t.textBox.text = "Enter name";
                t.textBox.TextChangedEvent += (sender, args) => {
                    if (string.IsNullOrWhiteSpace (args.text))
                        args.keepText = false;
                    else if (!Temperature.TemperatureGroupNameOk (args.text)) {
                        MessageBox.Show ("Temperature group name already exists");
                        args.keepText = false;
                    }
                };
            }
            AddSetting (t);

            t = new SettingsTextBox ();
            t.text = "Setpoint";
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = Temperature.GetTemperatureGroupTemperatureSetpoint (groupName).ToString ();
            } else {
                t.textBox.text = "0.0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToSingle (args.text);
                } catch {
                    MessageBox.Show ("Improper floating point number format");
                    args.keepText = false;
                }
            };
            settings.Add (t.label.text, t);

            t = new SettingsTextBox ();
            t.text = "Deadband";
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = Temperature.GetTemperatureGroupTemperatureDeadband (groupName).ToString ();
            } else {
                t.textBox.text = "0.5";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToSingle (args.text);
                } catch {
                    MessageBox.Show ("Improper floating point number format");
                    args.keepText = false;
                }
            };
            settings.Add (t.label.text, t);

            t = new SettingsTextBox ();
            t.text = "High Alarm";
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = Temperature.GetTemperatureGroupHighTemperatureAlarmSetpoint (groupName).ToString ();
            } else {
                t.textBox.text = "100.0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToSingle (args.text);
                } catch {
                    MessageBox.Show ("Improper floating point number format");
                    args.keepText = false;
                }
            };
            settings.Add (t.label.text, t);

            t = new SettingsTextBox ();
            t.text = "Low Alarm";
            if (groupName.IsNotEmpty ()) {
                t.textBox.text = Temperature.GetTemperatureGroupLowTemperatureAlarmSetpoint (groupName).ToString ();
            } else {
                t.textBox.text = "0.0";
            }
            t.textBox.TextChangedEvent += (sender, args) => {
                try {
                    Convert.ToSingle (args.text);
                } catch {
                    MessageBox.Show ("Improper floating point number format");
                    args.keepText = false;
                }
            };
            settings.Add (t.label.text, t);

            DrawSettings ();
        }

        protected bool OnSave (object sender) {
            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            if (groupName.IsEmpty ()) {
                var name = (settings["Name"] as SettingsTextBox).textBox.text;
                if (name == "Enter name") {
                    MessageBox.Show ("Invalid heater name");
                    return false;
                }

                var highTemperatureAlarmSetpoint = Convert.ToSingle ((settings["High Alarm"] as SettingsTextBox).textBox.text);
                var lowTemperatureAlarmSetpoint = Convert.ToSingle ((settings["Low Alarm"] as SettingsTextBox).textBox.text);
                var temperatureSetpoint = Convert.ToSingle ((settings["Setpoint"] as SettingsTextBox).textBox.text);
                var temperatureDeadband = Convert.ToSingle ((settings["Deadband"] as SettingsTextBox).textBox.text);

                Temperature.AddTemperatureGroup (
                    name,
                    highTemperatureAlarmSetpoint,
                    lowTemperatureAlarmSetpoint,
                    temperatureSetpoint,
                    temperatureDeadband);

                JObject jobj = new JObject ();

                jobj.Add (new JProperty ("name", name));
                jobj.Add (new JProperty ("highTemperatureAlarmSetpoint", highTemperatureAlarmSetpoint.ToString ()));
                jobj.Add (new JProperty ("lowTemperatureAlarmSetpoint", lowTemperatureAlarmSetpoint.ToString ()));
                jobj.Add (new JProperty ("temperatureSetpoint", temperatureSetpoint.ToString ()));
                jobj.Add (new JProperty ("temperatureDeadband", temperatureDeadband.ToString ()));

                (jo["temperatureGroups"] as JArray).Add (jobj);

                //Get new groups name
                groupName = name;
            } else {
                var highTemperatureAlarmSetpoint = Convert.ToSingle ((settings["High Alarm"] as SettingsTextBox).textBox.text);
                var lowTemperatureAlarmSetpoint = Convert.ToSingle ((settings["Low Alarm"] as SettingsTextBox).textBox.text);
                var temperatureSetpoint = Convert.ToSingle ((settings["Setpoint"] as SettingsTextBox).textBox.text);
                var temperatureDeadband = Convert.ToSingle ((settings["Deadband"] as SettingsTextBox).textBox.text);

                Temperature.SetTemperatureGroupHighTemperatureAlarmSetpoint (groupName, highTemperatureAlarmSetpoint);
                Temperature.SetTemperatureGroupLowTemperatureAlarmSetpoint (groupName, lowTemperatureAlarmSetpoint);
                Temperature.SetTemperatureGroupTemperatureSetpoint (groupName, temperatureSetpoint);
                Temperature.SetTemperatureGroupTemperatureDeadband (groupName, temperatureDeadband);
                
                JArray ja = jo["temperatureGroups"] as JArray;

                int arrIdx = -1;
                for (int i = 0; i < ja.Count; ++i) {
                    string n = (string)ja[i]["name"];
                    if (groupName == n) {
                        arrIdx = i;
                        break;
                    }
                }

                if (arrIdx == -1) {
                    MessageBox.Show ("Something went wrong");
                    return false;
                }

                ja[arrIdx]["highTemperatureAlarmSetpoint"] = highTemperatureAlarmSetpoint.ToString ();
                ja[arrIdx]["lowTemperatureAlarmSetpoint"] = lowTemperatureAlarmSetpoint.ToString ();
                ja[arrIdx]["temperatureSetpoint"] = temperatureSetpoint.ToString ();
                ja[arrIdx]["temperatureDeadband"] = temperatureDeadband.ToString ();
            }

            File.WriteAllText (path, jo.ToString ());
            
            return true;
        }

        protected bool OnDelete (object sender) {
            if (groupName == Temperature.defaultTemperatureGroup) {
                var parent = this.Toplevel as Gtk.Window;
                if (parent != null) {
                    if (!parent.IsTopLevel)
                        parent = null;
                }

                var ms = new TouchDialog (groupName + " is the default temperature group.\n" + 
                    "Are you sure you want to delete this group", parent);

                bool confirmed = false;
                ms.Response += (o, a) => {
                    if (a.ResponseId == ResponseType.Yes) {
                        confirmed = true;
                    }
                };

                ms.Run ();
                ms.Destroy ();

                if (!confirmed) {
                    return false;
                }
            }
            
            string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "tempProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);

            JArray ja = jo["temperatureGroups"] as JArray;

            int arrIdx = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja[i]["name"];
                if (groupName == n) {
                    arrIdx = i;
                    break;
                }
            }

            if (arrIdx == -1) {
                MessageBox.Show ("Something went wrong");
                return false;
            }

            ((JArray)jo["temperatureGroups"]).RemoveAt (arrIdx);

            File.WriteAllText (path, jo.ToString ());

            Temperature.RemoveTemperatureGroup (groupName);

            return true;
        }
    }
}

