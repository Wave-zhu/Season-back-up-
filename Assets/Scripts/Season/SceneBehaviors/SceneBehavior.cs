using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Season.Manager;

namespace Season.SceneBehaviors
{
    public abstract class SceneBehavior
    {
        public List<PoolItem> PoolItems;
        public SceneBehavior()
        {
            PoolItems = new();
        }
        
        public virtual Task LoadPoolItems()
        {
            return null;
        }

        public virtual void ReleasePoolItems()
        {
            
        }
            
        public virtual void EntryScene()
        {
            
        }

        public virtual void ExitScene()
        {
            
        }
        
        public virtual string GetAssetByEnum<T>(T enumType) where T : struct, Enum
        {
            return null;
        }
        
    }
}