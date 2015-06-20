using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RosterManager
{
    public class ModRecords
    {
        internal ProtoCrewMember Kerbal;

        public ModRecords()
        { }

        public ModRecords(ProtoCrewMember kerbal)
        {
            Kerbal = kerbal;
        }

        internal void Load()
        {

        }

        internal void Save()
        {

        }
    }
}
