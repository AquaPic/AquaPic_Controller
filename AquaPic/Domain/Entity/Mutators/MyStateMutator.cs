﻿#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2019 Goodtime Development

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
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Globals
{
    public class MyStateMutator : ISettingMutator<MyState>
    {
        public MyState Read (JObject jobj, string[] keys) {
            if (keys.Length < 1) {
                throw new ArgumentException ("keys can not be empty", nameof (keys));
            }

            var state = Default ();
            var text = (string)jobj[keys[0]];
            if (text.IsNotEmpty ()) {
                try {
                    state = (MyState)Enum.Parse (typeof (MyState), text);
                } catch {
                    //
                }
            }
            return state;
        }

        public void Write (MyState value, JObject jobj, string[] keys) {
            if (keys.Length < 1) {
                throw new ArgumentException ("keys can not be empty", nameof (keys));
            }
            jobj[keys[0]] = value.ToString ();
        }

        public bool Valid (MyState value) {
            return value != MyState.Invalid;
        }

        public MyState Default () {
            return MyState.Off;
        }
    }
}
