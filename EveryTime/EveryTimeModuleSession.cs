using Celeste;
using Celeste.Mod;
using System.Collections.Generic;

namespace EveryTime
{
    public class EveryTimeModuleSession : EverestModuleSession
    {
        public float TimeDilation = 1.0f;
        public int ExtraHair = 0;
        public int ChaserCount = 0;
        public int OshiroCount = 0;
        public float AnxietyBonus = 0;
        public float AnxietyStutter = 0;

        public List<AngryOshiro> SpawnedOshiros = new List<AngryOshiro>();
        public List<EveryTimeCustomChaser> SpawnedBadelineChasers = new List<EveryTimeCustomChaser>();

        public void Reset()
        {
            TimeDilation = 1.0f;
            ExtraHair = 0;
            ChaserCount = 0;
            OshiroCount = 0;
            AnxietyBonus = 0;
            AnxietyStutter = 0;

            SpawnedOshiros.Clear();
            SpawnedBadelineChasers.Clear();
        }
    }
}
