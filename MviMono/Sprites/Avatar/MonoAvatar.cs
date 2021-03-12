using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Sprites.Avatar;
using CharlyBeck.Utils3.ServiceLocator;
using MviMono.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Mono.Sprites.Avatar
{
    internal sealed class CMonoAvatarModel : CMonoModel
    {
        internal CMonoAvatarModel(CServiceLocatorNode aParent):base(aParent)
        {
        }
    }

    internal sealed class CMonoAvatarSprite : CMonoSprite<CAvatarSprite, CMonoAvatarModel>
    {
        internal CMonoAvatarSprite(CServiceLocatorNode aParent) :base(aParent)
        {
            this.MonoModel = this.MonoModels.MonoAvatarModel;
        }
        public override void Draw()
        {
        }
    }
}
