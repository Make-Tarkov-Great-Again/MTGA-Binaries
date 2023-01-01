using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTGA.Spawners.Grenades
{
    public class MultipleSmokeGrenadeSpawner : SmokeGrenadeSpawner
    {
        public override int Count => 10;
    }
}
