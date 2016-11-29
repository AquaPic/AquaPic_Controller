﻿using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Runtime;
using AquaPic.Modules;
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.Sensors;

namespace AquaPic.UserInterface
{
    public class WaterLevelWindow : WindowBase
    {
        uint timerId;
        TouchLabel atoStateTextBox;
        TouchLabel reservoirLevelTextBox;
        TouchLabel analogLevelTextBox;
        TouchLabel switchStateTextBox;
        TouchLabel switchTypeLabel;
        TouchComboBox switchCombo;
        TouchButton atoClearFailBtn;
        string switchName;

        public WaterLevelWindow (params object[] options) : base () {
            screenTitle = "Water Level";

            ExposeEvent += OnExpose;

            /**************************************************************************************************************/
            /* ATO                                                                                                        */
            /**************************************************************************************************************/
            var label = new TouchLabel ();
            label.text = "Auto Top Off";
            label.WidthRequest = 329;
            label.textColor = "seca";
            label.textSize = 12;
            label.textAlignment = TouchAlignment.Right;
            Put (label, 60, 80);
            label.Show ();

            var stateLabel = new TouchLabel ();
            stateLabel.text = "ATO State";
            stateLabel.textColor = "grey3"; 
            stateLabel.WidthRequest = 329;
            stateLabel.textAlignment = TouchAlignment.Center;
            Put (stateLabel, 60, 155);
            stateLabel.Show ();

            atoStateTextBox = new TouchLabel ();
            atoStateTextBox.WidthRequest = 329;
            if (WaterLevel.atoEnabled) {
                atoStateTextBox.text = string.Format ("{0} : {1}", 
                    WaterLevel.atoState, 
                    WaterLevel.atoTime.SecondsToString ());
            } else {
                atoStateTextBox.text = "ATO Disabled";
            }
            atoStateTextBox.textSize = 20;
            atoStateTextBox.textAlignment = TouchAlignment.Center;
            Put (atoStateTextBox, 60, 120);
            atoStateTextBox.Show ();

            var reservoirLevelLabel = new TouchLabel ();
            reservoirLevelLabel.WidthRequest = 329;
            reservoirLevelLabel.text = "Reservoir Level";
            reservoirLevelLabel.textColor = "grey3";
            reservoirLevelLabel.textAlignment = TouchAlignment.Center;
            Put (reservoirLevelLabel, 60, 230);
            reservoirLevelLabel.Show ();

            reservoirLevelTextBox = new TouchLabel ();
            reservoirLevelTextBox.SetSizeRequest (329, 50);
            reservoirLevelTextBox.textSize = 20;
            reservoirLevelTextBox.textAlignment = TouchAlignment.Center;
            if (WaterLevel.atoReservoirLevelEnabled) {
                float wl = WaterLevel.atoReservoirLevel;
                if (wl < 0.0f) {
                    reservoirLevelTextBox.text = "Probe Disconnected";
                } else {
                    reservoirLevelTextBox.text = wl.ToString ("F2");
                    reservoirLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
                }
            } else {
                reservoirLevelTextBox.text = "Sensor disabled";
            }
            Put (reservoirLevelTextBox, 60, 195);
            reservoirLevelTextBox.Show ();

            var atoSettingsBtn = new TouchButton ();
            atoSettingsBtn.text = "Settings";
            atoSettingsBtn.SetSizeRequest (100, 60);
            atoSettingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new AtoSettings ();
                s.Run ();
                s.Destroy ();
            };
            Put (atoSettingsBtn, 290, 405);
            atoSettingsBtn.Show ();

            var b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 60);
            b.ButtonReleaseEvent += (o, args) => {
                if (WaterLevel.atoReservoirLevelEnabled) {
                    var cal = new CalibrationDialog (
                        "ATO Reservoir Level Sensor",
                        () => {
                            return AquaPicDrivers.AnalogInput.GetChannelValue (WaterLevel.atoReservoirLevelChannel);
                        });

                    cal.CalibrationCompleteEvent += (aa) => {
                        WaterLevel.SetAtoReservoirCalibrationData (
                            (float)aa.zeroValue,
                            (float)aa.fullScaleActual,
                            (float)aa.fullScaleValue);
                    };

                    cal.calArgs.zeroValue = WaterLevel.atoReservoirLevelSensorZeroCalibrationValue;
                    cal.calArgs.fullScaleActual = WaterLevel.atoReservoirLevelSensorFullScaleCalibrationActual;
                    cal.calArgs.fullScaleValue = WaterLevel.atoReservoirLevelSensorFullScaleCalibrationValue;

                    cal.Run ();
                    cal.Destroy ();
                } else {
                    MessageBox.Show ("ATO reservoir level sensor is disabled\n" +
                                    "Can't perfom a calibration");
                }
            };
            Put (b, 180, 405);
            b.Show ();

            atoClearFailBtn = new TouchButton ();
            atoClearFailBtn.SetSizeRequest (100, 60);
            atoClearFailBtn.text = "Reset ATO";
            atoClearFailBtn.buttonColor = "compl";
            atoClearFailBtn.ButtonReleaseEvent += (o, args) => {
                if (!WaterLevel.ClearAtoAlarm ())
                    MessageBox.Show ("Please acknowledge alarms first");
            };
            Put (atoClearFailBtn, 70, 405);
            if (Alarm.CheckAlarming (WaterLevel.atoFailedAlarmIndex)) {
                atoClearFailBtn.Visible = true;
                atoClearFailBtn.Show ();
            } else {
                atoClearFailBtn.Visible = false;
            }

            Alarm.AddAlarmHandler (WaterLevel.atoFailedAlarmIndex, OnAtoFailedAlarmEvent);

            /**************************************************************************************************************/
            /* Analog water sensor                                                                                        */
            /**************************************************************************************************************/
            label = new TouchLabel ();
            label.text = "Water Level Sensor";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 80);
            label.Show ();

            label = new TouchLabel ();
            label.WidthRequest = 370;
            label.text = "Water Level";
            label.textColor = "grey3";
            label.textAlignment = TouchAlignment.Center;
            Put (label, 415, 155);
            label.Show ();

            analogLevelTextBox = new TouchLabel ();
            analogLevelTextBox.WidthRequest = 370;
            if (WaterLevel.analogSensorEnabled) {
                float wl = WaterLevel.analogWaterLevel;
                if (wl < 0.0f)
                    analogLevelTextBox.text = "Probe Disconnected";
                else {
                    analogLevelTextBox.text = wl.ToString ("F2");
                    analogLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
                }
            } else {
                analogLevelTextBox.text = "Sensor disabled";
            }
            analogLevelTextBox.textSize = 20;
            analogLevelTextBox.textAlignment = TouchAlignment.Center;
            Put (analogLevelTextBox, 415, 120);

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 60);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new AnalogSensorSettings ();
                s.Run ();
                s.Destroy ();
            };
            Put (settingsBtn, 415, 195);
            settingsBtn.Show ();

            b = new TouchButton ();
            b.text = "Calibrate";
            b.SetSizeRequest (100, 60);
            b.ButtonReleaseEvent += (o, args) => {
                if (WaterLevel.analogSensorEnabled) {
                    var cal = new CalibrationDialog (
                        "Water Level Sensor", 
                        () => {
                            return AquaPicDrivers.AnalogInput.GetChannelValue (WaterLevel.analogSensorChannel);
                        });

                    cal.CalibrationCompleteEvent += (aa) => {
                        WaterLevel.SetCalibrationData (
                            (float)aa.zeroValue, 
                            (float)aa.fullScaleActual, 
                            (float)aa.fullScaleValue);
                    };

                    cal.calArgs.zeroValue = WaterLevel.analogSensorZeroCalibrationValue;
                    cal.calArgs.fullScaleActual = WaterLevel.analogSensorFullScaleCalibrationActual;
                    cal.calArgs.fullScaleValue = WaterLevel.analogSensorFullScaleCalibrationValue;

                    cal.Run ();
                    cal.Destroy ();
                } else {
                    MessageBox.Show ("Analog water level sensor is disabled\n" +
                                    "Can't perfom a calibration");
                }
            };
            Put (b, 525, 195);
            b.Show ();

            /**************************************************************************************************************/
            /* Float Switches                                                                                             */
            /**************************************************************************************************************/
            switchName = WaterLevel.defaultFloatSwitch;

            label = new TouchLabel ();
            label.text = "Probes";
            label.textColor = "seca";
            label.textSize = 12;
            Put (label, 415, 280);
            label.Show ();

            var sLabel = new TouchLabel ();
            sLabel.text = "Current Switch State";
            sLabel.textAlignment = TouchAlignment.Center;
            sLabel.textColor = "grey3";
            sLabel.WidthRequest = 370;
            Put (sLabel, 415, 355);
            sLabel.Show ();

            switchStateTextBox = new TouchLabel ();
            switchStateTextBox.WidthRequest = 370;
            switchStateTextBox.textSize = 20;
            switchStateTextBox.textAlignment = TouchAlignment.Center;
            Put (switchStateTextBox, 415, 320);
            switchStateTextBox.Show ();

            //Type
            switchTypeLabel = new TouchLabel ();
            switchTypeLabel.WidthRequest = 370;
            switchTypeLabel.textAlignment = TouchAlignment.Center;
            switchTypeLabel.textColor = "grey3";
            Put (switchTypeLabel, 415, 370);
            switchTypeLabel.Show ();

            var switchSetupBtn = new TouchButton ();
            switchSetupBtn.text = "Probe Setup";
            switchSetupBtn.SetSizeRequest (100, 60);
            switchSetupBtn.ButtonReleaseEvent += (o, args) => {
                var s = new SwitchSettings (switchName, switchName.IsNotEmpty ());
                s.Run ();
                var newSwitchName = s.newOrUpdatedFloatSwitchName;
                var outcome = s.outcome;
                s.Destroy ();

                if ((outcome == TouchSettingsOutcome.Modified) && (newSwitchName != switchName)) {
                    var index = switchCombo.comboList.IndexOf (switchName);
                    switchCombo.comboList[index] = newSwitchName;
                    switchName = newSwitchName;
                } else if (outcome == TouchSettingsOutcome.Added) {
                    switchCombo.comboList.Insert (switchCombo.comboList.Count - 1, newSwitchName);
                    switchCombo.activeText = newSwitchName;
                    switchName = newSwitchName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    switchCombo.comboList.Remove (switchName);
                    switchName = WaterLevel.defaultFloatSwitch;
                    switchCombo.activeText = switchName;
                }

                switchCombo.QueueDraw ();
                GetSwitchData ();
            };
            Put (switchSetupBtn, 415, 405);
            switchSetupBtn.Show ();

            string[] sNames = WaterLevel.GetAllFloatSwitches ();
            switchCombo = new TouchComboBox (sNames);
            if (switchName.IsNotEmpty ()) {
                switchCombo.activeText = switchName;
            } else {
                switchCombo.activeIndex = 0;
            }
            switchCombo.WidthRequest = 235;
            switchCombo.comboList.Add ("New switch...");
            switchCombo.ComboChangedEvent += OnSwitchComboChanged;
            Put (switchCombo, 550, 277);
            switchCombo.Show ();

            GetSwitchData ();

            timerId = GLib.Timeout.Add (1000, OnUpdateTimer);

            Show ();
        }

        public override void Dispose () {
            Alarm.RemoveAlarmHandler (WaterLevel.atoFailedAlarmIndex, OnAtoFailedAlarmEvent);
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                TouchColor.SetSource (cr, "grey3", 0.75);
                cr.LineWidth = 3;

                cr.MoveTo (402.5, 70);
                cr.LineTo (402.5, 460);
                cr.ClosePath ();
                cr.Stroke ();

                cr.MoveTo (417.5, 267.5);
                cr.LineTo (780, 267.5);
                cr.ClosePath ();
                cr.Stroke ();
            }
        }

        public bool OnUpdateTimer () {
            if (WaterLevel.analogSensorEnabled) {
                float wl = WaterLevel.analogWaterLevel;
                if (wl < 0.0f) {
                    analogLevelTextBox.text = "Probe Disconnected";
                    analogLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                } else {
                    analogLevelTextBox.text = wl.ToString ("F2");
                    analogLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
                }
            } else {
                analogLevelTextBox.text = "Sensor disabled";
                reservoirLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
            }
            analogLevelTextBox.QueueDraw ();

            if (WaterLevel.atoEnabled) {
                atoStateTextBox.text = string.Format ("{0} : {1}", 
                    WaterLevel.atoState, 
                    WaterLevel.atoTime.SecondsToString ());

                if (WaterLevel.atoReservoirLevelEnabled) {
                    float wl = WaterLevel.atoReservoirLevel;
                    if (wl < 0.0f) {
                        reservoirLevelTextBox.text = "Probe Disconnected";
                        reservoirLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                    } else {
                        reservoirLevelTextBox.text = wl.ToString ("F2");
                        reservoirLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Inches;
                    }
                } else {
                    reservoirLevelTextBox.text = "Sensor disabled";
                    reservoirLevelTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.None;
                }
                reservoirLevelTextBox.QueueDraw ();

            } else {
                atoStateTextBox.text = "ATO Disabled";
            }
            atoStateTextBox.QueueDraw ();

            GetSwitchData ();

            return true;
        }

        protected void OnSwitchComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New switch...") {
                var s = new SwitchSettings (string.Empty, false);
                s.Run ();
                var newSwitchName = s.newOrUpdatedFloatSwitchName;
                var outcome = s.outcome;
                s.Destroy ();

                if (outcome == TouchSettingsOutcome.Added) {
                    switchCombo.comboList.Insert (switchCombo.comboList.Count - 1, newSwitchName);
                    switchCombo.activeText = newSwitchName;
                    switchName = newSwitchName;
                } else {
                    switchCombo.activeText = switchName;
                }

                switchCombo.QueueDraw ();
                GetSwitchData ();
            } else {
                switchName = e.activeText;
            }
        }

        protected void GetSwitchData () {
            if (switchName.IsNotEmpty ()) {
                bool state = WaterLevel.GetFloatSwitchState (switchName);

                if (state) {
                    switchStateTextBox.text = "Activated";
                    switchStateTextBox.textColor = "pri";
                } else {
                    switchStateTextBox.text = "Normal";
                    switchStateTextBox.textColor = "seca";
                }

                SwitchType type = WaterLevel.GetFloatSwitchType (switchName);
                switchTypeLabel.text = Utilites.Utils.GetDescription (type);
            } else {
                switchTypeLabel.Visible = false;
                switchStateTextBox.text = "Switch not available";
                switchStateTextBox.textColor = "white";
            }

            switchTypeLabel.QueueDraw ();
            switchStateTextBox.QueueDraw ();
        }

        protected double GetCalibrationValue () {
            return AquaPicDrivers.AnalogInput.GetChannelValue (WaterLevel.analogSensorChannel);
        }

        protected void OnAtoFailedAlarmEvent (object sender, AlarmEventArgs args) {
            Console.WriteLine ("Ato failed alarm event handler called");

            if (args.type == AlarmEventType.Cleared)
                atoClearFailBtn.Visible = false;
            else if (args.type == AlarmEventType.Posted)
                atoClearFailBtn.Visible = true;
            else
                return;

            atoClearFailBtn.QueueDraw ();
        }
    }
}

