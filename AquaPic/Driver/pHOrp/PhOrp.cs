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
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Drivers
{
    public partial class PhOrpBase : GenericBase
    {
        public static PhOrpBase SharedPhOrpInstance = new PhOrpBase ();

        protected PhOrpBase ()
            : base ("pH ORP") { }

        protected override void Run () {
            foreach (var card in cards.Values) {
                card.GetAllValuesCommunication ();
            }
        }

        protected override GenericCard CardCreater (string cardName, int address) {
            return new PhOrpCard (cardName, address);
        }

        public override string GetCardAcyronym () {
            return "PH";
        }

        public override CardType GetCardType () {
            return CardType.PhOrp;
        }
    }
}

