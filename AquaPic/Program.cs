﻿using System;
using Gtk;
using AquaPic.AlarmDriver;
using AquaPic.AnalogInputDriver;
using AquaPic.AnalogOutputDriver;
using AquaPic.LightingDriver;
using AquaPic.PowerDriver;
using AquaPic.SerialBus;
using AquaPic.TemperatureDriver;
using AquaPic.Utilites;
using AquaPic.Globals;

namespace AquaPic
{
	class MainClass
	{
        static int powerStrip1 = -1;
        static int powerStrip2 = -1;
        static int analogInputCard1 = -1;
        static int analogOutputCard1 = -1;

        public static void Main (string[] args)
		{
            Application.Init ();

            powerStrip1 = Power.Main.AddPowerStrip (16, "Left Power Strip");
            powerStrip2 = Power.Main.AddPowerStrip (17, "Right Power Strip");

            // Analog Input
            analogInputCard1 = AnalogInput.Main.AddCard (20, "Analog Input 1");

            // Analog Output
            analogOutputCard1 = AnalogOutput.Main.AddCard (30, "Analog Output 1");

            // Temperature
            Temperature.Main.AddHeater (powerStrip1, 6, "Bottom Heater");
            Temperature.Main.AddHeater (powerStrip1, 7, "Top Heater");
            Temperature.Main.AddTemperatureProbe (analogInputCard1, 0, "Sump Temperature");
            Temperature.Main.Init ();

            // Lighting
            Lighting.Main.AddLight (
                powerStrip1, 
                0, 
                analogOutputCard1,
                0,
                AnalogType.ZeroTen,
                "White LED", 
                0,
                0, 
                new Time (7, 30, 0), 
                new Time (8, 30, 0),
                0.0f,
                75.0f
            );
            Lighting.Main.AddLight (
                powerStrip1, 
                1, 
                analogOutputCard1,
                0,
                AnalogType.ZeroTen,
                "Actinic LED", 
                -15, 
                15, 
                new Time (7, 30, 0), 
                new Time (8, 30, 0),
                0.0f,
                75.0f
            );

            mainWindow mainScreen = new mainWindow ();
            mainScreen.Show ();
			Application.Run ();
		}
	}
}
