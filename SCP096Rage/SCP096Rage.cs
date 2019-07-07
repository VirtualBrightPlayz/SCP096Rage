using Smod2;
using Smod2.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualBrightPlayz.SCPSL.SCP096Rage
{
    [PluginDetails(author = "VirtualBrightPlayz",
        description = "Better 096",
        id = "virtualbrightplayz.scpsl.scp096rage",
        name = "096 Rage",
        configPrefix = "r049_2",
        version = "1.0",
        SmodMajor = 3,
        SmodMinor = 0,
        SmodRevision = 0)]
    public class SCP096Rage : Plugin
    {
        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
        }

        public override void Register()
        {
            this.AddEventHandlers(new SCP096RageEventHandler(this));
        }
    }
}
