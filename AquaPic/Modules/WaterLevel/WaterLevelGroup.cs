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
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;
using AquaPic.Sensors;
using AquaPic.DataLogging;
using AquaPic.Consumers;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        class WaterLevelGroup : SensorConsumer
        {
            public string name;
            public float highAnalogAlarmSetpoint;
            public float lowAnalogAlarmSetpoint;
            public bool enableHighAnalogAlarm;
            public bool enableLowAnalogAlarm;
            public List<string> floatSwitches;
            public List<string> waterLevelSensors;

            public float level;
            public float summedLevel;
            public IDataLogger dataLogger;
            public int highAnalogAlarmIndex;
            public int lowAnalogAlarmIndex;
            public int highSwitchAlarmIndex;
            public int lowSwitchAlarmIndex;

            public WaterLevelGroup (
                string name,
                float highAnalogAlarmSetpoint,
                bool enableHighAnalogAlarm,
                float lowAnalogAlarmSetpoint,
                bool enableLowAnalogAlarm,
                IEnumerable<string> floatSwitches,
                IEnumerable<string> waterLevelSensors) 
            {
                this.name = name;
                level = 0f;
                summedLevel = 0f;
                dataLogger = Factory.GetDataLogger (string.Format ("{0}WaterLevel", this.name.RemoveWhitespace ()));

                this.highAnalogAlarmSetpoint = highAnalogAlarmSetpoint;
                this.enableHighAnalogAlarm = enableHighAnalogAlarm;
                if (this.enableHighAnalogAlarm) {
                    highAnalogAlarmIndex = Alarm.Subscribe (string.Format ("{0} High Water Level (Analog)", this.name));
                }
                Alarm.AddAlarmHandler (highAnalogAlarmIndex, OnHighAlarm);

                this.lowAnalogAlarmSetpoint = lowAnalogAlarmSetpoint;
                this.enableLowAnalogAlarm = enableLowAnalogAlarm;
                if (this.enableLowAnalogAlarm) {
                    lowAnalogAlarmIndex = Alarm.Subscribe (string.Format ("{0} Low Water Level (Analog)", this.name));
                }
                Alarm.AddAlarmHandler (lowAnalogAlarmIndex, OnLowAlarm);

                highSwitchAlarmIndex = Alarm.Subscribe (string.Format ("{0} High Water Level (Switch)", this.name));
                lowSwitchAlarmIndex = Alarm.Subscribe (string.Format ("{0} Low Water Level (Switch)", this.name));
                Alarm.AddAlarmHandler (highSwitchAlarmIndex, OnHighAlarm);
                Alarm.AddAlarmHandler (lowSwitchAlarmIndex, OnLowAlarm);

                this.floatSwitches = new List<string> (floatSwitches);
                foreach (var floatSwitch in this.floatSwitches) {
                    AquaPicSensors.FloatSwitches.SubscribeConsumer (floatSwitch, this);
                }

                this.waterLevelSensors = new List<string> (waterLevelSensors);
                foreach (var waterLevelSensor in this.waterLevelSensors) {
                    AquaPicSensors.WaterLevelSensors.SubscribeConsumer (waterLevelSensor, this);
                }
            }

            public void GroupRun () {
                if (waterLevelSensors.Count > 0) {
                    dataLogger.AddEntry (level);
                } else {
                    dataLogger.AddEntry ("probe disconnected");
                }
            }

            protected void OnHighAlarm (object sender, AlarmEventArgs args) {
                if (args.type == AlarmEventType.Posted) {
                    dataLogger.AddEntry ("high alarm");
                }
            }

            protected void OnLowAlarm (object sender, AlarmEventArgs args) {
                if (args.type == AlarmEventType.Posted) {
                    dataLogger.AddEntry ("low alarm");
                }
            }

            public override void OnSensorUpdatedEvent (object sender, SensorUpdatedEventArgs args) {
                if (sender is WaterLevelSensor) {
                    if (args.name != args.settings.name) {
                        var index = waterLevelSensors.IndexOf (args.name);
                        waterLevelSensors[index] = args.settings.name;
                    }
                } else if (sender is FloatSwitch) {
                    if (args.name != args.settings.name) {
                        var index = floatSwitches.IndexOf (args.name);
                        floatSwitches[index] = args.settings.name;
                    }
                }
            }

            public override void OnSensorRemovedEvent (object sender, SensorRemovedEventArgs args) {
                if (sender is WaterLevelSensor) {
                    waterLevelSensors.Remove (args.name);
                } else if (sender is FloatSwitch) {
                    floatSwitches.Remove (args.name);
                }
            }

            public override void OnValueChangedEvent (object sender, ValueChangedEventArgs args) {
                var waterLevelSensor = sender as WaterLevelSensor;
                if (waterLevelSensor != null) {
                    if (waterLevelSensor.connected) {
                        summedLevel -= (float)args.oldValue;
                        summedLevel += (float)args.newValue;
                        level = summedLevel / waterLevelSensors.Count;
                    }

                    if (enableHighAnalogAlarm && (level > highAnalogAlarmSetpoint)) {
                        Alarm.Post (highAnalogAlarmIndex);
                    } else {
                        Alarm.Clear (highAnalogAlarmIndex);
                    }

                    if (enableLowAnalogAlarm && (level < lowAnalogAlarmSetpoint)) {
                        Alarm.Post (lowAnalogAlarmIndex);
                    } else {
                        Alarm.Clear (lowAnalogAlarmIndex);
                    }
                } else {
                    var floatSwitch = sender as FloatSwitch;
                    if (floatSwitch != null) {
                        if (floatSwitch.switchFuntion == SwitchFunction.HighLevel) {
                            if (floatSwitch.activated)
                                Alarm.Post (highSwitchAlarmIndex);
                            else {
                                Alarm.Clear (highSwitchAlarmIndex);
                            }
                        } else if (floatSwitch.switchFuntion == SwitchFunction.LowLevel) {
                            if (floatSwitch.activated)
                                Alarm.Post (lowSwitchAlarmIndex);
                            else {
                                Alarm.Clear (lowSwitchAlarmIndex);
                            }
                        }
                    }
                }
            }
        }
    }
}
