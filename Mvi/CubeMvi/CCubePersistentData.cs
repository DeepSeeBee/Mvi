using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.CubeMvi
{
    using CDataQuadrantDic = Dictionary<CCubePosKey, CQuadrantPersistentData>;

    internal abstract class CSpritePersistentData: CServiceLocatorNode
    {
        internal CSpritePersistentData(CServiceLocatorNode aParent, CCubePosKey aCubePosKey):base(aParent)
        {
            this.CubePosKey = aCubePosKey;
        }
        internal readonly CCubePosKey? CubePosKey;
        internal bool Destroyed;

    }

    internal sealed class CNullSpritePersistentData : CSpritePersistentData
    {
        internal CNullSpritePersistentData(CServiceLocatorNode aParent, CCubePosKey aKey):base(aParent, aKey)
        {

        }
    }

    internal sealed class CValidSpritePersistentData : CSpritePersistentData
    {
        internal CValidSpritePersistentData(CServiceLocatorNode aParent, int aId, CCubePosKey aKey) : base(aParent, aKey)
        {
            this.Id = aId;
        }
        internal readonly int Id;
    }

    internal sealed class CQuadrantPersistentData  : CServiceLocatorNode
    {
        internal CQuadrantPersistentData(CServiceLocatorNode aParent, CCubePosKey aCubePosKey, bool dummy):base(aParent)
        {
            this.CubePosKey = aCubePosKey;
        }

        internal CSpritePersistentData GetSpritePersistentData(CSprite aSprite)
        {
            if (aSprite.PersistencyEnabled)
            {
                var aId = aSprite.PersistentId.Value;
                var aExists = this.SpriteDataDic.ContainsKey(aId);
                var aData = aExists ? this.SpriteDataDic[aId] : new CValidSpritePersistentData(this, aId, this.CubePosKey);
                if (!aExists)
                    this.SpriteDataDic.Add(aId, aData);
                return aData;
            }
            else
            {
                return new CNullSpritePersistentData(this, this.CubePosKey);
            }
        }

        internal readonly CCubePosKey CubePosKey;
        private readonly Dictionary<int, CSpritePersistentData> SpriteDataDic = new Dictionary<int, CSpritePersistentData>();
    }

    internal sealed class CCubePersistentData : CServiceLocatorNode
    {
        internal CCubePersistentData(CServiceLocatorNode aParent) : base(aParent)
        {
        }


        internal CQuadrantPersistentData GetQuadrantPersistentData(CCubePosKey aId)
        {
            var aExists = this.QuadrantDataDic.ContainsKey(aId);
            var aData = aExists ? this.QuadrantDataDic[aId] : new CQuadrantPersistentData(this, aId, true);
            if (!aExists)
                this.QuadrantDataDic.Add(aId, aData);
            return aData;
        }
        private readonly CDataQuadrantDic QuadrantDataDic = new CDataQuadrantDic();
    }
}
