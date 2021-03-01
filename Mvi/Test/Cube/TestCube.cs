using CharlyBeck.Mvi.Cube;
using CharlyBeck.Utils3.Enumerables;
using CharlyBeck.Mvi.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Mvi.Facade;
using System.Collections;
using System.Diagnostics;
using CharlyBeck.Utils3.ServiceLocator;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Quadrant;
using CharlyBeck.Utils3.Exceptions;

namespace CharlyBeck.Mvi.Test.Cube
{
    using CCubeMoveTest = Tuple<string, int, string[]>;

    internal sealed class CCubeTest
    {

        private static void Test(bool aOk)
        {
            if (!aOk)
                throw new Exception("Test failed.");
        }

        private static void TestEquals(object aLhs, object aRhs)
           => Test(object.Equals(aLhs, aRhs));


        public void Run()
        {
            this.RunTestUnderDevelopment();
            //this.TestCoordinates();
            //this.TestCubeLoading();
        }

        private class C {
            //C c1; C c2; C c3; C c4; C c5; 
        }

        private void MeasurePerformance()
        {
            var aSw = new Stopwatch();
            aSw.Start();
            for (var aIdx = 0; aIdx < 125; ++aIdx)
            {
                new C();
            }
            aSw.Stop();
            Debug.Print("News took " + aSw.Elapsed.TotalMilliseconds + " ms.");
        }

        private void MeasureServiceLocatorPerformance()
        {
            throw new NotImplementedException();
            //var aRoot = new CDefaultServiceLocatorNode();
            //var aFacade = new CTestFacade(aRoot);
            //var aTile = aFacade.Cube.LoadedLeafDimensions.OfType<CTile>().First();
            //var aSpriteData = aTile.TileDataLoadProxy.Loaded; //.TileDescriptor.SpriteDatas.OfType<CBumperSpriteData>().First();
            //var aSw = new Stopwatch();
            //aSw.Start();
            //aSpriteData.ServiceContainer.GetService<CFacade>();
            //aSw.Stop();
            //Debug.Print("GetService took " + aSw.Elapsed.TotalMilliseconds + " ms.");
        }

        public void RunTestUnderDevelopment()
        {
            throw new NotImplementedException();
            //this.MeasurePerformance();
            //this.MeasureServiceLocatorPerformance();

            //var aFacade = new CTestFacade();
            //var aSw = new Stopwatch();
            //aFacade.Cube.MoveToCubeCoordinatesOnDemand(new CCubePos(0, 0, 0));
            //aSw.Start();
            //aFacade.Cube.MoveToCubeCoordinatesOnDemand(new CCubePos(1, 0, 0));
            //aSw.Stop();
            //Debug.Print("Move took " + aSw.Elapsed.TotalMilliseconds + " ms.");
        }


        public void TestCoordinates()
        {
          /*  Test(new CCubePos(0, 0).GetCubeCoordinates(new CCubePos(0, 0, 0), 3).IsEqual(new CCubePos(0, 0, 0)));
            Test(new CCubePos(0, 1).GetCubeCoordinates(new CCubePos(0, 0, 0), 3).IsEqual(new CCubePos(1, 0, 0)));
            Test(new CCubePos(0, 2).GetCubeCoordinates(new CCubePos(0, 0, 0), 3).IsEqual(new CCubePos(2, 0, 0)));
            Test(new CCubePos(0, 3).GetCubeCoordinates(new CCubePos(0, 0, 0), 3).IsEqual(new CCubePos(0, 1, 0)));
            Test(new CCubePos(0, 4).GetCubeCoordinates(new CCubePos(0, 0, 0), 3).IsEqual(new CCubePos(1, 1, 0)));
            Test(new CCubePos(0, 5).GetCubeCoordinates(new CCubePos(0, 0, 0), 3).IsEqual(new CCubePos(2, 1, 0)));
            Test(new CCubePos(0, 6).GetCubeCoordinates(new CCubePos(0, 0, 0), 3).IsEqual(new CCubePos(0, 2, 0)));
          */
        }

        internal sealed class CSpriteTester
        {
            private readonly Dictionary<Type, List<ISprite>> TestSprites = new Dictionary<Type, List<ISprite>>();
            private List<ISprite> GetSprites<TSprite>(bool aNeed = false) where TSprite : ISprite
            {
                var aKey = typeof(TSprite);
                if (!this.TestSprites.ContainsKey(aKey))
                    if (aNeed)
                        throw new Exception("Test failed.");
                    else
                        this.TestSprites.Add(aKey, new List<ISprite>());
                return this.TestSprites[aKey];
            }
            internal void Add<TSprite>(TSprite aSprite) where TSprite : ISprite
                => this.GetSprites<TSprite>().Add(aSprite);
            internal void Remove<TSprite>(TSprite aSprite) where TSprite : ISprite
            {
                var aList = this.GetSprites<TSprite>();
                if (aList.Contains(aSprite))
                    aList.Remove(aSprite);
                else
                    throw new Exception("Test failed.");
            }

            internal int GetCount<T>() where T : ISprite
                => this.GetSprites<T>().Count;

            internal void AssertSprites<T>(CCubePos aCoords) where T : ISprite
            {
                var aSprites = this.GetSprites<T>();
                var aMatches = from aSprite in aSprites where aSprite.SpriteData.AbsoluteCubeCoordinates.IsEqual(aCoords) select aSprite;
                var aOk = aMatches.Count() == 1;
                Test(aOk);
            }
            internal void AssertSprites<T>(IEnumerable<CCubePos> aCoords) where T : ISprite
            {
                foreach (var aCoord in aCoords)
                    this.AssertSprites<T>(aCoord);
            }

        }

        internal sealed class CTestFacade : CFacade
        {
            internal CTestFacade(CServiceLocatorNode aParent) :base(aParent)
            {

            }
            public CTestFacade() : this(new CDefaultServiceLocatorNode())
            {
            }
            public override void AddInGameThreadAction(Action aAction)
            {
                throw new NotImplementedException();
            }
            public override T Throw<T>(Exception aException)
                => aException.Throw<T>();

            internal readonly CSpriteTester SpriteTester = new CSpriteTester();

            public override ISprite<T> NewSprite<T>(T aData) // where T : CSpriteData Schweregrad	Code	Beschreibung	Projekt	Datei	Zeile	Unterdrückungszustand Fehler CS8370  Das Feature "Einschränkungen für Außerkraftsetzung und explizite Schnittstellenimplementierungsmethoden" ist in C# 7.3 nicht verfügbar. Verwenden Sie Sprachversion 8.0 oder höher.	
            {
                return new CTestSprite<T>(this.SpriteTester, (T)(object)aData);
            }
        }
        internal sealed class CTestSprite<T> : ISprite<T> // where T : CSpriteData Schweregrad	Code	Beschreibung	Projekt	Datei	Zeile	Unterdrückungszustand Fehler CS8370  Das Feature "Einschränkungen für Außerkraftsetzung und explizite Schnittstellenimplementierungsmethoden" ist in C# 7.3 nicht verfügbar. Verwenden Sie Sprachversion 8.0 oder höher.	
        {
            internal CTestSprite(CSpriteTester aSpriteTester, T aSpriteData) { this.SpriteDataM = aSpriteData; this.SpriteTester = aSpriteTester; this.SpriteTester.Add(this); }
            public CSpriteData SpriteData { get => (CSpriteData)(object)this.SpriteDataM; }
            private T SpriteDataM;

            private CSpriteTester SpriteTester;
            public void Update(BitArray a) { }
            public void Draw() { }
            public void Unload()
            {
                this.SpriteTester.Remove(this);
            }
        }

        public void TestCubeLoading()
        {
            //var aFacade = new CTestFacade();
            //var aWorld = aFacade.World;
            //var aCube = aWorld.Cube;
            //var aSpriteTester = aFacade.SpriteTester;
            //Test(aCube.DimensionIdx == 2);
            //var aCubeSubDimensions = aCube.LoadedSubDimensions.ToArray();
            //Test(aCubeSubDimensions.Count() == 3);

            //foreach (var aPlane in aCube.LoadedSubDimensions)
            //{
            //    Test(aPlane.LoadedSubDimensions.Count() == 9);
            //    Test(aPlane.DimensionIdx == 1);
            //    foreach (var aTile in aPlane.LoadedSubDimensions)
            //    {
            //        var aTileIdx = aPlane.LoadedSubDimensions.IndexOf(aTile);
            //        var aSize = aCube.Size;
            //        var aX = aTileIdx % aSize;
            //        var aY = aTileIdx - aX / aSize;
            //        TestEquals(aTile.LoadedSubDimensions.Count(), 0);
            //        TestEquals(aTile.DimensionIdx, 0);
            //    }
            //}
            //TestEquals(aSpriteTester.GetCount<CTestSprite<CQuadrantSpriteData>>(), 27);
            //TestEquals(aSpriteTester.GetCount<CTestSprite<CBeyoundSpaceSpriteData>>(), 19);
            //aSpriteTester.AssertSprites<CTestSprite<CBeyoundSpaceSpriteData>>(ToCoordinates("0|0|0", "1|0|0", "2|0|0", "0|1|0", "1|1|0", "2|1|0", "0|2|0", "1|2|0", "2|2|0", "0|0|1", "1|0|1", "2|0|1", "0|1|1", "0|2|1", "0|0|2", "1|0|2", "2|0|2", "0|1|2", "0|2|2"));


            //var aTests = new CCubeMoveTest[]
            //{
            //    new CCubeMoveTest("0|0|0", 27, new string[]{ "0|0|0", "1|0|0", "2|0|0", "0|1|0", "1|1|0", "2|1|0", "0|2|0", "1|2|0", "2|2|0", "0|0|1", "1|0|1", "2|0|1", "0|1|1", "0|2|1", "0|0|2", "1|0|2", "2|0|2", "0|1|2", "0|2|2"}),
            //    new CCubeMoveTest("1|0|0", 27, new string[]{ "0|0|0", "1|0|0", "2|0|0", "0|1|0", "1|1|0", "2|1|0", "0|2|0", "1|2|0", "2|2|0", "0|0|1", "1|0|1", "2|0|1", "0|1|1", "0|2|1", "0|0|2", "1|0|2", "2|0|2", "0|1|2", "0|2|2"}),

            //  //  new CCubeMoveTest("2|0|0", 27, new string[]{ "0|0|0", "1|0|0", "2|0|0", "0|1|0", "1|1|0", "2|1|0", "0|2|0", "1|2|0", "2|2|0", "0|0|1", "1|0|1", "2|0|1", "0|1|1", "0|2|1", "0|0|2", "1|0|2", "2|0|2", "0|1|2", "0|2|2"}),
            //    // 19
            //};
            //foreach(var aTest in aTests)
            //{
            //    var aCubeCoords = ToCoordinate(aTest.Item1);
            //    var aQuadrantSpriteCount = aTest.Item2;
            //    var aBeyoundCoords = ToCoordinates(aTest.Item3).ToArray();

            //    aCube.MoveToCubeCoordinatesOnDemand(aCubeCoords);
            //    TestEquals(aSpriteTester.GetCount<CTestSprite<CQuadrantSpriteData>>(), aQuadrantSpriteCount);
            //    TestEquals(aSpriteTester.GetCount<CTestSprite<CBeyoundSpaceSpriteData>>(), aBeyoundCoords.Length);

            //    aSpriteTester.AssertSprites<CTestSprite<CBeyoundSpaceSpriteData>>(aBeyoundCoords);
            //}

        }
        private static IEnumerable<CCubePos> ToCoords(params byte[] aQuadrantState)
        {
            throw new NotImplementedException();
        }
        private static IEnumerable<CCubePos> ToCoordinates(params string[] aData)
            => from aText in aData select ToCoordinate(aText);
        private static CCubePos ToCoordinate(string aText)
            => CCubePos.Parse(aText);
    }
}

