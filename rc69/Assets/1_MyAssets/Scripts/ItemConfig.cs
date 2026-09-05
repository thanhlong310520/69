
using MessagePack;
using Raccoon.EnumHolder;

namespace Raccoon.Item
{
    [MessagePackObject(keyAsPropertyName: true)]
    [System.Serializable]
    public class ItemConfig
    {
        public string id;
        public int priority;
        public int level;
        public EnvironmentType environment;
        public ItemConfig(string id, int priority, int level = 1)
        {
            this.id = id;
            this.priority = priority;
            this.level = level;
            SetEnvironmentType(EnvironmentType.Normal);
        }

        public void SetEnvironmentType(EnvironmentType environment)
        {
            this.environment = environment;
        }
    }
}
