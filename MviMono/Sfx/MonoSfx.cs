using CharlyBeck.Mvi.Sfx;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Utils3.Strings;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using CharlyBeck.Mvi.Mono.GameCore;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace MviMono.Sfx
{

    // Zapsplat is usually my first stop for free sounds.
    //Check out the Unity Asset Store, it probably has some free, high quality assets.Or has some cheap asset packs you could buy.
    // https://www.pmsfx.com/free   

    internal sealed class CMonoSoundBuffer : CSoundBuffer
    {
        internal CMonoSoundBuffer(CServiceLocatorNode aParent, SoundEffect aSong) : base(aParent)
        {
            this.SoundEffect = aSong;
        }

        public override double Volume { get => 1d; set { } }

        private SoundEffect SoundEffect;
        public override void Play()
        {
            this.SoundEffect.Play();
            this.Stopwatch.Restart();
            //MediaPlayer.IsRepeating = false;
            //MediaPlayer.Play(this.Song);
        }
        private Stopwatch Stopwatch = new Stopwatch();
        public override void Stop()
        {
            throw new NotImplementedException();
        }
        public override bool IsPlaying
        {
            get
            {
                if (!this.Stopwatch.IsRunning)
                    return false;
                else if (this.Stopwatch.ElapsedMilliseconds < this.SoundEffect.Duration.TotalMilliseconds)
                    return true;
                this.Stopwatch.Stop();
                return false;
            }
        }
    }

    internal sealed class CMonoSoundLoader :CSoundLoader
    {
        internal CMonoSoundLoader(CServiceLocatorNode aParent):base(aParent)
        {
            this.Game = this.ServiceContainer.GetService<CGame>();
        }
        private readonly CGame Game;
        public override CSoundBuffer LoadSoundBufferTemplate(FileInfo aFileInfo)
        {
            var aResName = CSoundDirectory.TrimBaseDirectory(aFileInfo).TrimEnd(aFileInfo.Extension);
            var aSong = this.Game.Content.Load<SoundEffect>(aResName);
            var aSoundBuffer = new CMonoSoundBuffer(this, aSong);
            return aSoundBuffer;
        }
    }
}
