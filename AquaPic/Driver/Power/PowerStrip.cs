﻿using System;
using System.Collections.Generic;
using AquaPic;
using AquaPic.SerialBus;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public partial class Power
    {
        private class PowerStrip {
            public AquaPicBus.Slave slave;
            public byte powerID;
            //public int commsAlarmIdx;
            public int powerLossAlarmIndex;
            public bool AcPowerAvailable;
            public string name;
            public OutletData[] outlets;

            public PowerStrip (byte address, byte powerID, string name, bool alarmOnLossOfPower, int powerLossAlarmIndex) {
                this.slave = new AquaPicBus.Slave (AquaPicBus.Bus1, address, name + " (Power Strip)");
                //this.slave.OnStatusUpdate += OnSlaveStatusUpdate;

                this.powerID = powerID;

                //this.commsAlarmIdx = Alarm.Subscribe (this.slave.Address.ToString () + " communication fault");

                if (alarmOnLossOfPower && (powerLossAlarmIndex == -1))
                    this.powerLossAlarmIndex = Alarm.Subscribe (
                        "Loss of power");
                else
                    this.powerLossAlarmIndex = powerLossAlarmIndex;

                this.name = name;
                this.AcPowerAvailable = false;

                this.outlets = new OutletData[8];
                for (int i = 0; i < 8; ++i) {
                    int plugID = i;
                    string plugName = this.name + "." + "p" + plugID.ToString() ;
                        
                    this.outlets [plugID] = new OutletData (
                        plugName,
                        () => SetOutletState ((byte)plugID, MyState.On),
                        () => SetOutletState ((byte)plugID, MyState.Off));
                }
            }

            public unsafe void SetupOutlet (byte outletID, MyState fallback) {
                const int messageLength = 2; 

                byte[] message = new byte[messageLength];

                message [0] = outletID;

                if (fallback == MyState.On)
                    message [1] = 0xFF;
                else
                    message [1] = 0x00;

                unsafe {
                    fixed (byte* ptr = message) {
                        slave.Write (2, ptr, sizeof(byte) * messageLength);
                    }
                }
            }

            public unsafe void GetStatus () {
                slave.Read (20, sizeof(PowerComms), GetStatusCallback);
            }

            protected void GetStatusCallback (CallbackArgs callArgs) {
                if (slave.Status != AquaPicBusStatus.communicationSuccess)
                    return;

                PowerComms status = new PowerComms ();

                unsafe {
                    callArgs.copyBuffer (&status, sizeof(PowerComms));
                }

                AcPowerAvailable = status.acPowerAvailable;
                if (!AcPowerAvailable)
                    Alarm.Post (powerLossAlarmIndex);

                for (int i = 0; i < outlets.Length; ++i) {
                    if (Utils.mtob (status.currentAvailableMask, i))
                        ReadOutletCurrent ((byte)i);
                }
            }

            public void ReadOutletCurrent (byte outletID) {
                unsafe {
                    slave.ReadWrite (10, &outletID, sizeof (byte), sizeof (AmpComms), ReadOutletCurrentCallback);
                }
            }

            protected void ReadOutletCurrentCallback (CallbackArgs callArgs) {
                if (slave.Status != AquaPicBusStatus.communicationSuccess)
                    return;

                AmpComms message;

                unsafe {
                    callArgs.copyBuffer (&message, sizeof(AmpComms));
                }

                outlets [message.outletID].SetAmpCurrent (message.current);
            }

            public void SetOutletState (byte outletID, MyState state) {
                const int messageLength = 2;

                outlets [outletID].manualState = state;

                byte[] message = new byte[messageLength];
                message [0] = outletID;

                if (state == MyState.On)
                    message [1] = 0xFF;
                else
                    message [1] = 0x00;

                unsafe {
                    fixed (byte* ptr = message) {
                        slave.ReadWrite (
                            30, 
                            ptr, 
                            sizeof(byte) * messageLength, 
                            0, 
                            (args) => {
                                //outlets [outletID].OnChangeState (new StateChangeEventArgs (outletID, powerID, state)));
                                outlets [outletID].currentState = state;
                                OnStateChange (outlets [outletID], new StateChangeEventArgs (outletID, powerID, state));
                            });
                    }
                }

                //<TEST> this is here only because the slave never responds so the callback never happens
                //outlets [outletID].OnChangeState (new StateChangeEventArgs (outletID, powerID, state));
                //outlets [outletID].currentState = state;
                //OnStateChange (outlets [outletID], new StateChangeEventArgs (outletID, powerID, state));
            }

            public void SetPlugMode (byte outletID, Mode mode) {
                //outlets [outletID].OnModeChange (new ModeChangeEventArgs (outletID, powerID, outlets [outletID].mode));
                outlets [outletID].mode = mode;
                OnModeChange (outlets [outletID], new ModeChangeEventArgs (outletID, powerID, outlets [outletID].mode));
            }

//            commented out because alarm handling is done in the serial slave object
//            protected void OnSlaveStatusUpdate (object sender) {
//                if ((slave.Status != AquaPicBusStatus.communicationSuccess) ||
//                    (slave.Status != AquaPicBusStatus.communicationStart) ||
//                    (slave.Status != AquaPicBusStatus.open))
//                    Alarm.Post (commsAlarmIdx);
//                else {
//                    if (Alarm.CheckAlarming (commsAlarmIdx))
//                        Alarm.Clear (commsAlarmIdx);
//                }
//            }
    	}
    }
}