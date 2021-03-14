using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Utils3.Strings;
using System.Xml;
using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Sprites.SolarSystem;
using CharlyBeck.Mvi.Sprites.Shot;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Utils3.Enumerables;
using System.Reflection;
using CharlyBeck.Utils3.Reflection;
using CharlyBeck.Utils3.SystemIo;
using CharlyBeck.Mvi.ContentManager;
using CharlyBeck.Mvi.Sprites.Gem;

namespace CharlyBeck.Mvi.Sfx
{
    using CNearestObject=Tuple<double, CObjectId, System.TimeSpan>;
    using CHitpoint = Tuple<TimeSpan, CSoundFile>;
    using CQueuedHitpoint = Tuple<TimeSpan, CSoundFile, TimeSpan>;
    using CTimeSpanRange = Tuple<TimeSpan, TimeSpan>;
    using CDoubleRange = Tuple<double, double>;

    internal sealed class CSoundDirectoryPathAttribute : Attribute
    {
        public CSoundDirectoryPathAttribute(string aPath, bool aIsLeafDir)
        {
            this.Path = aPath;
            this.IsLeafDir = aIsLeafDir;
        }
        public readonly string Path;
        public readonly bool IsLeafDir;
    }

    public enum CSoundDirectoryEnum
    {
        [CSoundDirectoryPath(@"Audio\Ambient", true)]
        Audio_Ambient,
        [CSoundDirectoryPath(@"Audio\Collision", true)]
        Audio_Collision,
        [CSoundDirectoryPath(@"Audio\Destroyed", false)]
        Audio_Destroyed,
        [CSoundDirectoryPath(@"Audio\Destroyed\Planet", true)]
        Audio_Destroyed_Planet,
        [CSoundDirectoryPath(@"Audio\Destroyed\Sun", true)]
        Audio_Destroyed_Sun,
        [CSoundDirectoryPath(@"Audio\Destroyed\Moon", true)]
        Audio_Destroyed_Moon,
        [CSoundDirectoryPath(@"Audio\Flyby", true)]
        Audio_Flyby,
        [CSoundDirectoryPath(@"Audio\Wormhole", false)]
        Audio_Wormhole,
        [CSoundDirectoryPath(@"Audio\Wormhole\Enter", true)]
        Audio_Wormhole_Enter,
        [CSoundDirectoryPath(@"Audio\Wormhole\Exit", true)]
        Audio_Wormhole_Exit,
        [CSoundDirectoryPath(@"Audio\Wormhole\NewWorld", true)]
        Audio_Wormhole_NewWorld,
        [CSoundDirectoryPath(@"Audio\GemCollected", true)]
        Audio_GemCollected,
        [CSoundDirectoryPath(@"Audio\Laser", true)]
        Audio_Laser,
        [CSoundDirectoryPath(@"Audio\GemSpeech", true)]
        Audio_GemSpeech

    }

    internal enum CSoundFxClassEnum
    {
        Flyby,
        Collision,
    }

    internal sealed class CObjectId
    {
        internal CObjectId()
        {
        }
        internal bool IsEqual(CObjectId rhs)
            =>this.RefEquals<CObjectId>(rhs);
    }

    public abstract class CSoundBuffer :CServiceLocatorNode
    {
        protected CSoundBuffer(CServiceLocatorNode aParent)
        {
        }
        public  abstract bool IsPlaying { get; }
        public abstract double Volume { get; set; }
        public abstract void Play();
        public abstract void Stop();
     //   public abstract void SetTargetPosition(double aMs);
    }

    internal sealed class CNullSoundBuffer : CSoundBuffer
    {
        internal CNullSoundBuffer(CServiceLocatorNode aParent)  : base(aParent)
        {

        }
        public override void Play()
        {
        }
        public override void Stop()
        {
        }
        public override bool IsPlaying => false;
        public override double Volume { get => 0d; set { } }
    }

    public abstract class CSoundLoader : CServiceLocatorNode
    {
        protected CSoundLoader(CServiceLocatorNode aParent) : base(aParent)
        {
        }

        public abstract CSoundBuffer LoadSoundBufferTemplate(FileInfo aFileInfo);

        public CSoundBuffer LoadSoundBuffer(FileInfo aFileInfo)
            => CStaticParameters.Sound_Loading_Enabled
            ? this.LoadSoundBufferTemplate(aFileInfo)
            : new CNullSoundBuffer(this)
            ;

    }

    internal sealed class CSoundFile : CServiceLocatorNode
    {
        internal CSoundFile(CServiceLocatorNode aParent, FileInfo aFileInfo) : base(aParent)
        {
            this.SoundBufferLoader = this.ServiceContainer.GetService<CSoundLoader>();
            this.FileInfo = aFileInfo;
            if(this.XmlFileInfo.Exists)
            {
                this.ReadXml();
            }
            this.SoundBuffer = this.SoundBufferLoader.LoadSoundBuffer(this.FileInfo);
        }
        internal readonly FileInfo FileInfo;
        private FileInfo XmlFileInfo =>
            new FileInfo(Path.Combine(this.FileInfo.Directory.FullName, this.FileInfo.Name.TrimEnd(this.FileInfo.Extension) + ".xml"));
        private void ReadXml()
        {
            var aFileInfo = this.XmlFileInfo;
            var aXmlDoc = new XmlDocument();
            aXmlDoc.Load(aFileInfo.FullName);
            var aDocElement = aXmlDoc.DocumentElement;
            var aHitPointElement = aDocElement.SelectSingleNode("Hitpoint");
            var aHitPointText = aHitPointElement is object ? aHitPointElement.InnerText : default(string);
            var aHitPoint = aHitPointText is object ? new double?(double.Parse(aHitPointText)) : default(double?);
            this.HitPoint = aHitPoint.HasValue ? TimeSpan.FromMilliseconds( aHitPoint.Value) : default(TimeSpan?);
        }
        internal TimeSpan? HitPoint { get; private set; }

        private readonly CSoundLoader SoundBufferLoader;
        internal readonly CSoundBuffer SoundBuffer;

    }

    public abstract class CSoundDirectory : CServiceLocatorNode
    {
        internal CSoundDirectory(CServiceLocatorNode aParent) : base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();
            this.RandomGenerator = new CRandomGenerator(this);
            this.RandomGenerator.Begin();
        }
        internal readonly List<CSoundFile> SoundFiles = new List<CSoundFile>();
        internal readonly CRandomGenerator RandomGenerator;
        internal readonly CWorld World;
        internal bool CreateDirectoryDic;
        private Dictionary<CSoundDirectoryEnum, List<CSoundFile>> SoundFileDic  = new Dictionary<CSoundDirectoryEnum, List<CSoundFile>>();
        internal void Play(CSoundFile aSoundFile)
        {
            aSoundFile.SoundBuffer.Volume = this.RandomGenerator.NextFromDoubleRange(this.GetVolumeRange());
            aSoundFile.SoundBuffer.Play();
        }

        internal IEnumerable<CSoundFile> GetSoundFiles(CSoundDirectoryEnum aDirectoryEnum)
        {
            if (!this.CreateDirectoryDic)
                throw new InvalidOperationException();
            else if (this.SoundFileDic.ContainsKey(aDirectoryEnum))
                return this.SoundFileDic[aDirectoryEnum];
            else
                return Array.Empty<CSoundFile>();
        }
        protected void AddFile(FileInfo aFileInfo)
        {
            var aSoundFile = new CSoundFile(this, aFileInfo);
            this.SoundFiles.Add(aSoundFile);
            if(this.CreateDirectoryDic)
            {
                this.AddToMapping(aSoundFile, aFileInfo);
            }
        }
        
        private void AddToMapping(CSoundFile aSoundFile, FileInfo aFileInfo)
        {
            var at = typeof(CSoundDirectoryPathAttribute);
            var aEnumTyp = typeof(CSoundDirectoryEnum);
            var aFields = from aValue in Enum.GetValues(aEnumTyp).Cast<CSoundDirectoryEnum>()
                          select new Tuple<FieldInfo, CSoundDirectoryEnum>(aEnumTyp.GetField(aValue.ToString()), aValue);
            var aAttributes = (from aField in aFields
                               where aField.Item1.IsDefined(at, false)
                               select new Tuple<CSoundDirectoryEnum, CSoundDirectoryPathAttribute>(aField.Item2, aField.Item1.GetCustomAttributes(at, false).OfType<CSoundDirectoryPathAttribute>().Single()));
            var aPath = TrimBaseDirectory(aFileInfo);
            var aDirEnums = (from aAttrib in aAttributes
                            where aPath.StartsWith(aAttrib.Item2.Path)
                            where aAttrib.Item2.IsLeafDir
                             select aAttrib.Item1).ToArray();
            var aDirEnum = aDirEnums.Single();
            var aDic = this.SoundFileDic;
            var aContains = aDic.ContainsKey(aDirEnum);
            var aList = aContains ? aDic[aDirEnum] : new List<CSoundFile>();
            if (!aContains)
                aDic.Add(aDirEnum, aList);
            aList.Add(aSoundFile);
        }
        internal CSoundFile GetRandomSound(CSoundDirectoryEnum aDirectory)
        {
            if (!this.CreateDirectoryDic)
                throw new NotImplementedException();
            else
            {
                var aSounds = this.SoundFileDic[aDirectory].ToArray();
                var aSound = this.RandomGenerator.NextItem<CSoundFile>(aSounds);
                return aSound;
            }
        }
        //protected void AddDirectory(DirectoryInfo aDirectoryInfo, string aExtension)
        //{
        //    var aFileInfos = aDirectoryInfo.GetFiles("*" + aExtension);
        //    foreach (var sf in aFileInfos)
        //        this.AddFile(sf);
        //}
        protected void AddDirectory(CSoundDirectoryEnum aSoundDirectoryEnum)
        {
            var aDirectory = GetDirectoryInfo(aSoundDirectoryEnum.GetCustomAttribute<CSoundDirectoryPathAttribute>().Path);
            var aFiles = this.ContentManager.GetDirectoryContent(aDirectory);
            foreach (var f in aFiles)
                this.AddFile(f);
        }
        internal IEnumerable<CHitpoint> Hitpoints
            => from aSoundFile in this.SoundFiles
               where aSoundFile.HitPoint.HasValue
               select new CHitpoint(aSoundFile.HitPoint.Value, aSoundFile)
               ;


        protected static DirectoryInfo GetBaseDirectoryInfo()
            => CContentManager.GetBaseDirectoryInfo();
        //=> new DirectoryInfo(Path.Combine(new FileInfo(typeof(CSoundDirectory).Assembly.Location).Directory.FullName, "Content"));
        public static string TrimBaseDirectory(FileInfo aFileInfo)
            => CContentManager.TrimBaseDirectory(aFileInfo);
        protected static DirectoryInfo GetDirectoryInfo(params string[] aSubDir)
            => new DirectoryInfo(Path.Combine(new string[] { GetBaseDirectoryInfo().FullName }.Concat(aSubDir).ToArray()));

        #region HitPointEngine
        private CHitPointEngine HitPointEngineM;
        internal CHitPointEngine HitPointEngine => CLazyLoad.Get(ref this.HitPointEngineM, this.NewHitPointEngine);
        private CHitPointEngine NewHitPointEngine()
            => this.HitPointEngineIsDefined ? new CHitPointEngine(this, this.Hitpoints.ToArray()) : throw new InvalidOperationException();
        internal virtual bool HitPointEngineIsDefined => false;
        private void RunHitPointStateMachine()
        {
            if(this.HitPointEngineIsDefined)
            {
                this.HitPointEngine.Update();
            }
        }
        #endregion
        internal virtual void Update()
        {
            this.RunHitPointStateMachine();
            this.RunRandomSoundScapeStateMachine();
        }

        protected virtual CDoubleRange GetVolumeRange()
            => new CDoubleRange(0.75d, 1.0d);
        #region RandomSoundScape
        internal bool RandomSoundScapeIsEnabled;
        private void RunRandomSoundScapeStateMachine()
        {
            if (this.RandomSoundScapeIsEnabled 
            || this.RandomScapeState != CRandomScapeStateEnum.Idle)
            {
                switch (this.RandomScapeState)
                {
                    case CRandomScapeStateEnum.Idle:
                        if (this.SoundFiles.Count > 0)
                        {
                            this.CurrentSoundBuffer = GetRandomSoundBuffer();
                            this.CurrentSoundBuffer.Play();
                            this.RandomScapeState = CRandomScapeStateEnum.Playing;
                        }
                        break;

                    case CRandomScapeStateEnum.Playing:
                        if (!this.CurrentSoundBuffer.IsPlaying)
                        {
                            this.WaitTimeSpan = this.RandomGenerator.NextFromTimeSpanRange(this.WaitTimeMax);
                            this.WaitStopwatch.Start();
                            this.RandomScapeState = CRandomScapeStateEnum.Waiting;
                        }
                        break;

                    case CRandomScapeStateEnum.Waiting:
                        if (this.WaitStopwatch.Elapsed.TotalSeconds >= this.WaitTimeSpan.TotalSeconds)
                        {
                            this.WaitStopwatch.Stop();
                            this.RandomScapeState = CRandomScapeStateEnum.Idle;
                        }
                        break;

                }
            }
        }

        internal CSoundFile GetRandomSoundFile()
            => this.SoundFiles[this.RandomGenerator.NextInteger(0, this.SoundFiles.Count - 1)];
        private CSoundBuffer GetRandomSoundBuffer()
            =>this.GetRandomSoundFile().SoundBuffer;

        protected CSoundBuffer CurrentSoundBuffer;
        private enum CRandomScapeStateEnum
        {
            Idle,
            Playing,
            Waiting
        }
        internal CTimeSpanRange WaitTimeMax = new CTimeSpanRange(new TimeSpan(0, 0, 5), new TimeSpan(0, 0, 50)); // TODO-Params
        private CRandomScapeStateEnum RandomScapeState = CRandomScapeStateEnum.Idle;
        private TimeSpan WaitTimeSpan;
        private readonly Stopwatch WaitStopwatch = new Stopwatch();
        #endregion
        internal void PlayRandomSound()
            => this.GetRandomSoundBuffer().Play();

        #region ContentManager
        private CContentManager ContentManagerM;
        private CContentManager ContentManager => CLazyLoad.Get(ref this.ContentManagerM, () => this.ServiceContainer.GetService<CContentManager>());
        #endregion
    }

    internal sealed class CSoundSequence 
    {
        internal CSoundSequence(int aCountMax, params CSoundFile[] aSoundFiles)
        {
            this.CountMax = aCountMax;
            this.List = new List<CSoundFile>(aSoundFiles);
        }
        internal readonly int CountMax;
        private readonly List<CSoundFile> List;
        internal void Add(CSoundFile aSoundFile)
        {
            while (this.CountMax > 0
               && this.List.Count >= this.CountMax)
            { 
                Debug.Print("CSoundSequence: Remove because of flood.");
                this.List.RemoveAt(0);            
            }
            if (this.List.Count < this.CountMax)
                this.List.Add(aSoundFile);
        }
        private CSoundFile CurrentNullable;
        internal void Update()
        {
            if (this.CurrentNullable is object
            && !this.CurrentNullable.SoundBuffer.IsPlaying)
            {
             //   Debug.Print(DateTime.Now.ToString() + ": CSoundSequence: Sound stopped");
                this.CurrentNullable = default;
            }
            if (this.List.Count > 0
            && !(this.CurrentNullable is object))
            {
               // Debug.Print(DateTime.Now.ToString() + ": CSoundSequence: Sound starts.");
                this.CurrentNullable = this.List.First();
                this.CurrentNullable.SoundBuffer.Play();
                this.List.RemoveAt(0);
            }
        }
    }

    internal sealed class CAmbientSoundDirectory : CSoundDirectory
    {
        internal CAmbientSoundDirectory(CServiceLocatorNode aParent) : base(aParent)
        {
            this.OpenDirectory();
            this.RandomSoundScapeIsEnabled = true;
        }
        private void OpenDirectory()
            => this.AddDirectory(CSoundDirectoryEnum.Audio_Ambient);
    }

    internal sealed class CDestroyedSoundDirectory : CSoundDirectory
    {
        internal CDestroyedSoundDirectory(CServiceLocatorNode aParent):base(aParent)
        {
            this.CreateDirectoryDic = true;
            this.AddDirectories();
            this.World.SpriteDestroyedByShot += delegate (CSprite aSprite, CShotSprite aShot)
            {
                var e = aSprite.DestroyedSound;
                if(e.HasValue)
                    this.GetRandomSound(e.Value).SoundBuffer.Play();
            };
        }

        private void AddDirectories()
            => this.AddDirectory(CSoundDirectoryEnum.Audio_Destroyed);
    }

    internal sealed class CFlybySoundDirectory : CSoundDirectory
    {
        internal CFlybySoundDirectory(CServiceLocatorNode aParent) :base(aParent)
        {
            this.AddDirectory(CSoundDirectoryEnum.Audio_Flyby);
            this.WaitTimeMax = new CTimeSpanRange(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(5000)); // TODO_PARAMETERS
        }
        internal override void Update()
        {
            var aWorld = this.World;
            var aFrameInfo = aWorld.FrameInfo;
            var aFlybySprites = from aSprite in aFrameInfo.SpriteDistances
                                where aSprite.Item1.PlaysFlybySound
                                where aSprite.Item1.DistanceToAvatar < this.MinimumDistance
                                select aSprite;
            var aNearbyCount = aFlybySprites.Count();
            this.RandomSoundScapeIsEnabled = aNearbyCount >= this.MinimumCount;
            //System.Diagnostics.Debug.Assert(!this.RandomSoundScapeIsEnabled);
            base.Update();
        }

        private double MinimumCount = 1; // TODO_PARAMETERS
        private double MinimumDistance = 0.25; // TODO_PARAMETERS
        private void OpenDirectory()
            => this.AddDirectory(CSoundDirectoryEnum.Audio_Flyby);
    }

    internal sealed class CCollisionSoundDirectory : CSoundDirectory
    {
        internal CCollisionSoundDirectory(CServiceLocatorNode aParent)  : base(aParent)
        {
            this.OpenDirectory();

            if(this.HitPointEngineIsDefined)
                this.HitPointEngine.Load();
        }
        private void OpenDirectory()
            => this.AddDirectory(CSoundDirectoryEnum.Audio_Collision);
        internal override bool HitPointEngineIsDefined => false;
    }
    internal sealed class CShotsSoundDirectory : CSoundDirectory
    {
        internal CShotsSoundDirectory(CServiceLocatorNode aParent) : base(aParent)
        {
            this.OpenDirectory();
            this.World.ShootFired += delegate ()
            {
                this.PlayRandomSound();
            };
        }
        private void OpenDirectory()
            => this.AddDirectory(CSoundDirectoryEnum.Audio_Laser);
    }
    internal sealed class CHitPointEngine : CServiceLocatorNode
    {
        internal CHitPointEngine(CServiceLocatorNode aParent, CHitpoint[] aHitPoints) : base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();
            this.HitPoints = aHitPoints.OrderBy(aHitPoint=>aHitPoint.Item1.TotalMilliseconds).ToArray();
            this.BackgroundTaskCancelRequested = false;
            this.BackgroundTask = Task.Factory.StartNew(this.RunBackgroundTask);
        }

        private readonly CWorld World;

        private readonly List<CNearestObject> SamplePoints = new List<CNearestObject>();
        private readonly int SampleCountMax = 40;
        private readonly TimeSpan SampleIntervall = new TimeSpan(0, 0, 0, 0, 25);
        private Task BackgroundTask;
        private Stopwatch SampleStopwatch = new Stopwatch();
        private bool BackgroundTaskCancelRequested;
        private CHitpoint[] HitPoints;
        private volatile CQueuedHitpoint QueuedHitpointNullable;

        private void RunBackgroundTask()
        {
            var aHitpoints = this.HitPoints;
            var aRandom = new Random((int)DateTime.Now.Ticks);
            var aIgnoreCollisionTendencyThisTime = new Func<int, bool>(idx =>
            {
                var aCount2 = aHitpoints.Last().Item1.TotalMilliseconds
                            / this.SampleIntervall.TotalMilliseconds;
                var aCount = (double)aHitpoints.Length;
                var aRnd = aRandom.NextDouble();
                var cmp = ((double)idx * ( this.SampleIntervall.TotalMilliseconds / aHitpoints.Last().Item1.TotalMilliseconds));
                var aIgnore = aRnd > cmp * 0.07d;
                return aIgnore;
            });

            var aSamplePoints = new List<CNearestObject>(this.SampleCountMax);
            var aQueuedHitpoint = default(CQueuedHitpoint);
            var aPlayingHitpoint = default(CQueuedHitpoint);
            while (!this.BackgroundTaskCancelRequested)
            {

                if (aHitpoints.Length > 0)
                {
                    aSamplePoints.Clear();
                    lock (this.SamplePoints)
                    {
                        aSamplePoints.AddRange(this.SamplePoints);
                    }
                    if (aSamplePoints.Count == SampleCountMax)
                    {
                        var aSamplePoints2 = (from aSamplePoint in aSamplePoints
                                              where aSamplePoint.Item2.IsEqual(aSamplePoints.Last().Item2)
                                              select aSamplePoint).ToArray(); // aSamplePoints.GroupBy(aSamplePoint => aSamplePoint.Item2).ToArray();
                        if (aSamplePoints2.Length > 0)
                        {
                            double aCurrentDistance = 0d;
                            bool aGettingNearer = true;
                            for (var i = 1; i < aSamplePoints2.Length && aGettingNearer; ++i)
                            {
                                var aPrevious = aSamplePoints2[i - 1].Item1;
                                var aCurrent = aSamplePoints2[i].Item1;
                                aCurrentDistance = aPrevious - aCurrent;
                                if (aCurrentDistance < 0.0)
                                {
                                    aGettingNearer = false;
                                }
                            }
                            if (aGettingNearer)
                            {
                                var aFirst = aSamplePoints.First();
                                var aLast = aSamplePoints.Last();
                                var aFirstDist = aFirst.Item1;
                                var aLastDist = aLast.Item1;
                                var aLastTimeMs = aLast.Item3.TotalMilliseconds;
                                var aFirstTimeMs = aFirst.Item3.TotalMilliseconds;
                                var aDeltaTime = aLastTimeMs - aFirstTimeMs; // aLast.Item3.Subtract(aFirst.Item3);
                                var aDeltaDistance = aFirstDist - aLastDist;
                                if (aDeltaDistance > 0)
                                {
                                    var aSpeed = aDeltaDistance / aDeltaTime;
                                    var aDistanceToGo = aLastDist;
                                    var aTimeUntilCollision = aSpeed == 0d
                                                            ? double.NaN
                                                            : aDistanceToGo / aSpeed;
                                    if (double.IsNaN(aTimeUntilCollision))
                                    {
                                        this.QueuedHitpointNullable = default;
                                    }
                                    else if(aTimeUntilCollision > 0)
                                    {
                                        bool aIgnored = false;
                                        bool aHitpointQueued = false;
                                        for (var i = 0; i < aHitpoints.Length && !aHitpointQueued &&!aIgnored; ++i)
                                        {
                                            var aHitpoint = aHitpoints[i];
                                            if (aHitpoint.Item1.TotalMilliseconds > aTimeUntilCollision )
                                            {
                                                if (aIgnoreCollisionTendencyThisTime(i))
                                                {
                                                    aIgnored = true;
                                                }
                                                else
                                                {
                                                    var i2 = i > 0 ? i - 1 : i;
                                                    var aPreviousTime = aHitpoints[i2];
                                                    var aTimeToStartHitpoint = aTimeUntilCollision - aHitpoint.Item1.TotalMilliseconds;
                                                    aQueuedHitpoint = new CQueuedHitpoint(aHitpoint.Item1, aHitpoint.Item2, TimeSpan.FromMilliseconds(aTimeToStartHitpoint));
                                                    this.QueuedHitpointNullable = aQueuedHitpoint;
                                                    aPlayingHitpoint = aQueuedHitpoint;
                                                    aHitpointQueued = true;
                                                }
                                                }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep((int)this.SampleIntervall.TotalMilliseconds);
            }
        }

        private void ResetSamples()
        {
            lock (this.SamplePoints)
            {
                this.SamplePoints.Clear();
            }
        }

        private void Sample(CNearestObject aNearestObject)
        {
            bool aRecordSample;
            if(this.SamplePoints.Count > 0)
            {
                var aLastTime = this.SamplePoints.Last().Item3;
                var aThisTime = aNearestObject.Item3;
                var aDiffTime = aThisTime.Subtract(aLastTime);
                aRecordSample = aDiffTime.CompareTo(this.SampleIntervall) >= 0;
            }
            else
            {
                aRecordSample = true;
            }
            if(aRecordSample)
            {
                lock(this.SamplePoints)
                {
                    this.SamplePoints.Add(aNearestObject);
                    while (this.SamplePoints.Count > this.SampleCountMax)
                        this.SamplePoints.RemoveAt(0);
                }
            }
        }
        private CQueuedHitpoint PlayingHitpointNullable;

        internal void Update()
        {
            var aWorld = this.World;
            var aNearest = aWorld.FrameInfo.NearestBumperIsDefined
                         ? new CNearestObject(aWorld.FrameInfo.NearestAsteroid.DistanceToAvatar, aWorld.FrameInfo.NearestAsteroid.ObjectId, aWorld.GameTimeTotal)
                         : default(CNearestObject)
                         ;
            if(aNearest is object)
            {
                this.Sample(aNearest);
            }
            else
            {
                this.ResetSamples();
            }
            var aQueuedHitpointNullable = this.QueuedHitpointNullable;
            if(aQueuedHitpointNullable is object)
            {
                var aStopped = this.PlayingHitpointNullable is object
                            && !this.PlayingHitpointNullable.Item2.SoundBuffer.IsPlaying
                            ;
                if (aStopped)
                    this.PlayingHitpointNullable = default;

                var aYetPlaying = this.PlayingHitpointNullable is object
                               && this.PlayingHitpointNullable.Item2.RefEquals<CSoundFile>(aQueuedHitpointNullable.Item2);
                if(!aYetPlaying)
                {
                    aQueuedHitpointNullable.Item2.SoundBuffer.Play();
                    this.PlayingHitpointNullable = aQueuedHitpointNullable;
                    this.QueuedHitpointNullable = default;
                }
            }
        }
    }

    internal sealed class CWormholeSoundDirectory:CSoundDirectory
    {
        internal CWormholeSoundDirectory(CServiceLocatorNode aParent) :base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();
            this.CreateDirectoryDic = true;
            this.AddDirectory(CSoundDirectoryEnum.Audio_Wormhole);
            this.World.WormholeEntered += this.OnWormholeEntered;
        }
        private readonly CWorld World;

        private void OnWormholeEntered(CSprite aSprite)
        {
            this.GetRandomSound(CSoundDirectoryEnum.Audio_Wormhole_Enter).SoundBuffer.Play();
        }

    }

    internal sealed class CGemCollectedSoundDirectory : CSoundDirectory
    {
        private CSoundSequence SoundSequence = new CSoundSequence(4);

        internal CGemCollectedSoundDirectory(CServiceLocatorNode aParent) : base(aParent)
        {
            this.AddDirectory(CSoundDirectoryEnum.Audio_GemCollected);

            this.World.GemCollected += delegate (CGemSprite aGemSprite)
            {
                var aSound = this.GetRandomSoundFile();
                this.SoundSequence.Add(aSound);
                this.World.OnGemCollectedSoundStarting(aGemSprite, this.SoundSequence.Add);
            };
            this.Init();
        }
        internal override void Update()
        {
            base.Update();
            this.SoundSequence.Update();
        }

    }


    internal sealed class CGemSpeechSoundDirectory :CSoundDirectory
    {
        internal CGemSpeechSoundDirectory(CServiceLocatorNode aParent) :base(aParent)
        {
            this.AddDirectory(CSoundDirectoryEnum.Audio_GemSpeech);
            var aGetGemCategoryEnum = new Func<FileInfo, CGemCategoryEnum>(aFileInfo =>
            { // TODO-Hack
                var aName = TrimBaseDirectory(aFileInfo).TrimEnd(aFileInfo.Extension).TrimStart(@"Audio\GemSpeech\");
                var aSplit = aName.Split('_');
                var aCategoryText = aSplit[0];
                var aCategoryEnum = (CGemCategoryEnum)Enum.Parse(typeof(CGemCategoryEnum), aCategoryText);
                return aCategoryEnum;
            });
            var aGetGemEnum = new Func<FileInfo, CGemEnum>(aFileInfo =>
            {// TODO-Hack
                var aName = TrimBaseDirectory(aFileInfo).TrimEnd(aFileInfo.Extension).TrimStart(@"Audio\GemSpeech\");
                var aSplit = aName.Split('_');
                var aGemEnumText = aSplit[1];
                var aGemEnum = (CGemEnum)Enum.Parse(typeof(CGemEnum), aGemEnumText);
                return aGemEnum;
            });
            var aSoundFiles1 = this.SoundFiles;
            var aSoundFiles2 = (from aSoundFile in aSoundFiles1
                               select new Tuple<CGemCategoryEnum, CGemEnum, CSoundFile>
                               (
                                   aGetGemCategoryEnum(aSoundFile.FileInfo),
                                   aGetGemEnum(aSoundFile.FileInfo),
                                   aSoundFile
                                )).ToArray();
            this.World.GemCollectedSoundStarting += delegate (CGemSprite aGemSprite, Action<CSoundFile> aAddFollowUp)
            {
                var aSound = (from aTest in aSoundFiles2 where aTest.Item2 == aGemSprite.GemEnum select aTest).Single();
                aAddFollowUp(aSound.Item3);
            };

            this.Init();
        }

    }
    internal sealed class CSoundManager : CServiceLocatorNode
    {
        internal CSoundManager(CServiceLocatorNode aParent)  :base(aParent)
        {
            this.CollisionSoundDirectory = new CCollisionSoundDirectory(this);
            this.AmbientSoundDirectory = new CAmbientSoundDirectory(this);
            this.ShotsSoundDirectory = new CShotsSoundDirectory(this);
            this.EventSoundDirectory = new CDestroyedSoundDirectory(this);
            this.FlybySoundDirectory = new CFlybySoundDirectory(this);
            this.WormholeSoundDirectory = new CWormholeSoundDirectory(this);
            this.GemCollectedSoundDirectory = new CGemCollectedSoundDirectory(this);
            this.GemSpeechSoundDirectory = new CGemSpeechSoundDirectory(this);
        }
        private readonly CCollisionSoundDirectory CollisionSoundDirectory;
        private readonly CAmbientSoundDirectory AmbientSoundDirectory;
        private readonly CShotsSoundDirectory ShotsSoundDirectory;
        private readonly CDestroyedSoundDirectory EventSoundDirectory;
        private readonly CFlybySoundDirectory FlybySoundDirectory;
        private readonly CWormholeSoundDirectory WormholeSoundDirectory;
        private readonly CGemCollectedSoundDirectory GemCollectedSoundDirectory;
        private readonly CGemSpeechSoundDirectory GemSpeechSoundDirectory;

        private IEnumerable<CSoundDirectory> SoundDirectories
        {
            get
            {
                yield return this.CollisionSoundDirectory;
                yield return this.AmbientSoundDirectory;
                yield return this.ShotsSoundDirectory;
                yield return this.EventSoundDirectory;
                yield return this.FlybySoundDirectory;
                yield return this.WormholeSoundDirectory;
                yield return this.GemCollectedSoundDirectory;
                yield return this.GemSpeechSoundDirectory;
            }
        }
        internal void Update()
        {
            var aSoundDirectories = this.SoundDirectories;
            foreach (var aSoundDirectory in aSoundDirectories)
                aSoundDirectory.Update();
        }
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            return aServiceContainer;
        }
        #endregion
    }

}
