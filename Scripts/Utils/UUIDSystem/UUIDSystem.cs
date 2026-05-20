using System.Collections.Generic;
using System.Linq;

namespace NBodySimulation.Utils
{
    public static class UUIDSystem
    {
        private static List<int> existed = [];

        public static int GetUUID()
        {
            for (int id = 1;; id++)
            {
                if (!existed.Contains(id))
                {
                    existed.Add(id);
                    return id;
                }
            }
        }

        public static void ReleaseUUID(int id)
        {
            existed.Remove(id);
        }
    }
}