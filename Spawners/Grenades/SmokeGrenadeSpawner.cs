using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.A.Tarkov.Core.Spawners.Grenades
{
    public class SmokeGrenadeSpawner : GrenadeSpawner
    {
        public override string TemplateId { get => "617aa4dd8166f034d57de9c5"; set => base.TemplateId = value; }
        public SmokeGrenadeSpawner() : base("617aa4dd8166f034d57de9c5") { }
    }
}
