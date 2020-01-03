using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Mapset;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.Util;
using StorybrewCommon.Subtitles;
using StorybrewCommon.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO; 

namespace StorybrewScripts
{
    public class Lyric
    {
        [JsonProperty("sentence")] 
        public string Sentence { get; set; }
        
        [JsonProperty("startTime")]
        public double startTime { get; set; }
        
        [JsonProperty("endTime")]
        public double endTime { get; set; }
        
        [JsonProperty("kiai")]
        public bool kiai { get; set; }
        
        [JsonProperty("color")] 
        public string Color { get; set; }    
    }

    public class collab
    {
        [JsonProperty("mapper")] 
        public string Mapper { get; set; }
        
        [JsonProperty("startTime")]
        public double startTime { get; set; }
        
        [JsonProperty("endTime")]
        public double endTime { get; set; }
          
    }
    public class Main : StoryboardObjectGenerator
    {   
        public bool IUnderstandSpritePoolsExistToSaveOsbSizeAndAreBadForPerformances = false;
        
        StoryboardLayer backLayer, backhitLayer, foreLayer, hitLayer,kiaiLayer;
        FontGenerator whiteFont, blackFont, collabFont, creditFont, kiaiFont;
        public override void Generate()
        {
		    backLayer = GetLayer("b");
            backhitLayer = GetLayer("bh");
            foreLayer = GetLayer("f");
            hitLayer = GetLayer("h");
            kiaiLayer = GetLayer("kiai");
            setupFonts();
            setupBackground();
            
            setupLyrics();

            setupTransitions();

            collabNames();
            
            var vig = foreLayer.CreateSprite("sb/v.png");
            vig.Scale(0,480.0f/1080);
            vig.Fade(0,1);
            vig.Fade(220685,0);

            
        }

        void setupBackground()
        {
            removeBG();
            mainBG();
            kiaiBG();
            freestyleBG();
            setupFlash();
            credits();
            afterCreditBuildUp();
            
            whitebg();

        }

        void mainBG()
        {   
            var bg = backLayer.CreateSprite("hello.jpg");
            bg.Scale(0,854.0f/1300);
            bg.Fade(0,0.35);
            bg.Fade(10340, 0);
            bg.Fade(11030, 0.5);
            bg.Fade(20685,0);
            bg.Fade(44134,0.35);
            bg.Fade(48789,0);
            bg.Fade(165513,0.35);
            bg.Scale(OsbEasing.OutExpo,187582,187927,854.0f/1300, 9);
            bg.Scale(187927,854.0f/1300);
            bg.Fade(187582,187927, 0.35,0);
            
        }
        void whitebg()
        {
            var bg = backLayer.CreateSprite("sb/p.png");
            bg.ScaleVec(0,new Vector2(854,480));
            bg.Fade(0,0); 
            bg.Fade(22065,1);
            bg.Fade(43444,0);
            
            
            bg.Fade(110340,1);
            bg.Fade(118616,119220,1,0);
            bg.Fade(74478, 75858,0,1);
            bg.Fade(75858,0);
            bg.Fade(108961, 109651,0,1);
            


        }

        #region Kiai
            void kiaiBG()
            {
                var bgBlur = backhitLayer.CreateSprite("sb/blur.jpg");
                bgBlur.Scale(49651, (854.0f/1300) + 0.07f);
                bgBlur.Fade(49651, 1);
                bgBlur.Fade(71375, 71720,1,0);

                bgBlur.Fade(122754, 1);
                bgBlur.Fade(137582, 137927,1,0);
                bgBlur.Fade(198616,1);
                bgBlur.Fade(214823,215168,1,0);

                var bgOverlay = backhitLayer.CreateSprite("hello.jpg");
                bgOverlay.Fade(0,0);
                bgOverlay.Fade(49651, 0.2f);
                bgOverlay.Scale(49651, (854.0f/1300) + 0.07f);
                bgOverlay.Fade(71375, 71720, 0.2,0);
                bgOverlay.Fade(122754, 0.2f);
                bgOverlay.Fade(137582, 137927, 0.2,0);
                bgOverlay.Fade(198616, 0.2f);
                bgOverlay.Fade(209651, 0);
                bgOverlay.Additive(bgOverlay.CommandsStartTime,bgOverlay.CommandsEndTime);
               
                var lastHitObject = Beatmap.HitObjects.First();

                var EasingType = OsbEasing.OutSine;
                    foreach (var hitObject in Beatmap.HitObjects)
                    {
                        if ((hitObject.StartTime < 49651 - 5 || 
                        (71720 - 5 <= hitObject.StartTime && hitObject.StartTime < 122754 - 5) 
                        || 137927 - 5 <= hitObject.StartTime && hitObject.StartTime < 198616 - 5
                        || 209651 - 5 <= hitObject.StartTime))
                        {
                            lastHitObject = hitObject;
                            continue;
                        }
                        
                        var oldVec = lastHitObject.PositionAtTime(lastHitObject.EndTime);
                        var oldPos = GetTrackedLocation(oldVec.X, oldVec.Y);
                        var newVec = hitObject.PositionAtTime(hitObject.StartTime);
                        var newPos = GetTrackedLocation(newVec.X, newVec.Y);
                        bgOverlay.Move(EasingType, lastHitObject.EndTime, hitObject.StartTime, oldPos.X, oldPos.Y, newPos.X, newPos.Y);
                        lastHitObject = hitObject;

                        if (hitObject is OsuSlider)
                        {
                            var timestep = tick(0,4);
                            var startTime = hitObject.StartTime;
                            while (true)
                            {
                                var endTime = startTime + timestep;

                                var complete = hitObject.EndTime - endTime < 5;
                                if (complete) endTime = hitObject.EndTime;

                                oldVec = hitObject.PositionAtTime(startTime);
                                oldPos = GetTrackedLocation(oldVec.X, oldVec.Y);
                                newVec = hitObject.PositionAtTime(endTime);
                                newPos = GetTrackedLocation(newVec.X, newVec.Y);
                                bgOverlay.Move(EasingType, startTime + 1, endTime, oldPos.X, oldPos.Y, newPos.X, newPos.Y);
                                Log($"Point: {startTime}: {oldVec.ToString()}, {newVec.ToString()}");
                                if (complete) Log($"EndTime: {endTime.ToString()}");
                                if (complete) break;
                                startTime += timestep;

                            }
                        }
                        
                    }
                kiaiParticles(49651,71720);
                afterKiai1();
                kiaiParticles(122754,137927);
                kiaiParticles(198616,209651);
                squareOut(71375, 71720);
                squareOut(137582, 137927);
                squareOut(209306,209651); 
                afterKiai2();
                end();
            }
            
            void afterKiai1()
            {
                for(var time = 71720; time <= 73099; time += (int)tick(0,1))
                {
                    generateCluster(time, time + tick(0,1));
                }

                for(var time = 73099; time <= 74478; time += (int)tick(0,2))
                {
                    generateCluster(time, time + tick(0,2));
                }

                for(var time = 74478; time <= 74909; time += (int)tick(0,4))
                {
                    generateCluster(time, time + tick(0,4));
                }
            }
            void afterKiai2()
            {
                for(var time = 137927; time <= 139306; time += (int)tick(0,1))
                {
                    generateCluster(time, time + tick(0,1));
                }

                for(var time = 139306; time <= 141375; time += (int)tick(0,2))
                {
                    generateCluster(time, time + tick(0,2));
                }

                var dot = backLayer.CreateSprite("sb/pl.png");
                dot.Scale(OsbEasing.InExpo, 140685, 142065, 0, 10);

                toSquare(142065, 142237, 143271, 143444, Color4.White);
            }

            void end()
            {
                for(var time = 215168; time <= 216547; time += (int)tick(0,1))
                {
                    generateCluster(time, time + tick(0,1));
                }

                for(var time = 216547; time <= 217409; time += (int)tick(0,2))
                {
                    generateCluster(time, time + tick(0,2));
                }

                var dot = backLayer.CreateSprite("sb/pl.png");
                dot.Scale(OsbEasing.InExpo, 217927, 219306, 0, 10);

            }

            void kiaiParticles(Double StartTime, double EndTime)
            {
                var particleCount = 250;
                var particleDuration = 630;

                using (var pool = new OsbSpritePool(backLayer, "sb/pl.png", OsbOrigin.Centre, (sprite, startTime, endTime) =>
                {
                    
                    sprite.Color(startTime,new Color4(255,226,198,255));
                }))
                {
                    var timeStep = particleDuration / particleCount;
                    for (var startTime = (double)StartTime - particleDuration; startTime <= EndTime - particleDuration; startTime += timeStep)
                    {
                        var endTime = startTime + (particleDuration + Random(-55, 330));

                        var sprite = pool.Get(startTime, endTime);
                        
                        var startX = Random(-107, 747);
                        var startY = 520;
                        var endX = startX;
                        var endY = -100;
                        sprite.Scale(startTime, Random(0.025,0.15));
                        sprite.Move(startTime, endTime, startX, startY, endX, endY);
                    
                    }
                }

                if(StartTime == 122754)
                {
                    var plate = kiaiLayer.CreateSprite("sb/p.png");
                    plate.ScaleVec(121375, new Vector2(854,480));
                    plate.Fade(121375,1);
                    plate.Fade(122754,0);
                    plate.Color(121892,122409,Color4.Black, Color4.White); 
                    
                }
                if(StartTime == 198616)
                {
                    var plate = kiaiLayer.CreateSprite("sb/p.png");
                    plate.ScaleVec(197237, new Vector2(854,480));
                    plate.Fade(197237,1);
                    plate.Fade(198616,0);
                    plate.Color(197237,Color4.Black);
                 }
            }
        #endregion

        #region Freestyle
        
            void freestyleBG()
            {
                var bg = backLayer.CreateSprite("hello.jpg");
                bg.Scale(0,854.0f/1300);
                bg.Fade(0,0);
                bg.Fade(77237, 0.45);
                bg.Fade(97927,0);

                List<double> flashTimes = new List<double>{77237, 79996, 81375, 82754, 82754, 84134, 85513, 86547, 88272, 89651, 91030, 93444, 93789, 96547};
                var fadebg = backhitLayer.CreateSprite("sb/p.png");
                fadebg.ScaleVec(0,new Vector2(854,500));
                fadebg.Fade(0,0);
                foreach(var hitobject in Beatmap.HitObjects)
                {
                    foreach(var flashtime in flashTimes)
                    {
                        if(hitobject.StartTime <= flashtime && hitobject.EndTime >= flashtime)
                        {
                            fadebg.Color(flashtime, hitobject.Color);
                            fadebg.Fade(flashtime, flashtime + 1000, 0.43, 0);
                        }
                    }
                }
                freestyleSliders();
                darkysRings();
            }
    
            void freestyleSliders()
            {   
                var sliderTimes = new List<double> {77237, 78616, 79996, 80685, 80858, 81030};
                
                using (var pool = new OsbSpritePool(hitLayer, "sb/pl.png", OsbOrigin.Centre, (sprite, startTime, endTime) =>
                {

                }))
                {
                    foreach(var hitobject in Beatmap.HitObjects)
                    {
                        if(hitobject is OsuSlider)
                        {
                            foreach(var sliderTime in sliderTimes)
                            {
                                if((hitobject.StartTime >= 77237 - 5 && hitobject.StartTime < 86892 + 5) || 
                                (hitobject.StartTime >= 88271 - 5 && hitobject.StartTime < 97927)
                                || (hitobject.StartTime >= 143444 - 5 && hitobject.StartTime < 153099)
                                || (hitobject.StartTime >= 154478 - 5 && hitobject.StartTime < 165513)
                                || (hitobject.StartTime >= 104823 - 5 && hitobject.StartTime < 105513)
                                )
                                {
                                    //Log($"We got one at: {hitobject.StartTime}");
                                    for(var time = hitobject.StartTime; time <= hitobject.EndTime; time += tick(0, 12))
                                    {
                                        var sprite = pool.Get(time, hitobject.EndTime + (time - hitobject.StartTime + 120));
                                        sprite.Scale(time, time + 50, 0, 0.5);
                                        sprite.Fade(time,1);
                                        sprite.Color(time, hitobject.Color);
                                        sprite.Fade(hitobject.EndTime, hitobject.EndTime + (time - hitobject.StartTime + 120), 1, 0);
                                        sprite.Move(time,hitobject.PositionAtTime(time));
                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }

            void darkysRings()
            {
                var ringTimes = new List<double> {99306, 102065, 104823, 107582};

                var ring = backhitLayer.CreateSprite("sb/r.png");
                foreach(var hitobject in Beatmap.HitObjects)
                {   
                    foreach(var ringTime in ringTimes)
                    {
                        if(hitobject.StartTime >= ringTime - 5 && hitobject.StartTime < ringTime + 5)
                        {
                            var endTime = hitobject.StartTime + (tick(0,1) * 2);
                            ring.Fade(hitobject.StartTime,1);
                            ring.Move(hitobject.StartTime, hitobject.Position);
                            ring.Scale(hitobject.StartTime, hitobject.StartTime + tick(0,16), 0, 0.25);
                            for(var i = hitobject.StartTime + tick(0,16); i < endTime; i += tick(0,8))
                            {
                                ring.Scale(i, i + tick(0,16), 0.25, 0.2);
                                ring.Scale(i + tick(0,16), i + tick(0,8), 0.2, 0.25);
                            }
                            ring.Fade(endTime,0);
                        }
                    }

                    var ring2 = hitLayer.CreateSprite("sb/r.png");

                    
                    for(double i = 24823; i <= 41375; i += tick(0, 1) * 8)
                    {
                        if(hitobject.StartTime >= i - 5 && hitobject.StartTime < i + 5)
                        {
                            ring2.Fade(hitobject.StartTime,0.75);
                            ring2.Move(hitobject.StartTime, hitobject.Position);
                            ring2.Scale(OsbEasing.OutExpo,hitobject.StartTime, hitobject.StartTime + 1000, 0, 0.75);
                            ring2.Fade(hitobject.StartTime + 250, hitobject.StartTime + 750, 0.75,0);
                            
                        }
                    }
                    if(hitobject.StartTime >= 111030 - 5 && hitobject.StartTime < 111030 + 5)
                    {
                        ring2.Fade(hitobject.StartTime,0.75);
                        ring2.Move(hitobject.StartTime, hitobject.Position);
                        ring2.Scale(OsbEasing.OutExpo,hitobject.StartTime, hitobject.StartTime + 1000, 0, 0.75);
                        ring2.Fade(hitobject.StartTime + 250, hitobject.StartTime + 750, 0.75,0);
                        
                    }

                    

                }
            }
        
        #endregion

        void setupTransitions()
        {
            DoubleBlind(8789, 10340, Color4.White);
            toSquare(10340, 10513, 10685, 11030, Color4.White);

            
        }

        

        void removeBG()
        {
            var bg = backLayer.CreateSprite("hello.jpg");
            bg.Fade(0,0);
        }

        #region Transitions

            void setupFlash()
            {
                var flash = foreLayer.CreateSprite("sb/p.png");
                flash.ScaleVec(0,new Vector2(854,480));
                flash.Fade(0,685,1,0);
                flash.Fade(11030, 11547,1,0);
                flash.Fade(20685,21461,1,0);
                
                flash.Fade(122754, 123013, 1,0);
                flash.Fade(49651, 49823,1,0);
                flash.Fade(44134, 44565, 1,0);
                flash.Fade(77237, 77668,1,0);
                flash.Fade(48703,1);
                flash.Fade(49651,0);
                flash.Fade(47927, 48703, 0, 1);
                flash.Fade(143444, 143875, 1,0);
                
                
            }
            void DoubleBlind(double startTime, double endTime, Color4 color)
            {
                var top = foreLayer.CreateSprite("sb/p.png",OsbOrigin.TopCentre, new Vector2(320, 0));
                if(color != Color4.White || color != new Color4(255,255,255,255)) top.Color(startTime,color);
                top.ScaleVec(OsbEasing.InOutSine, startTime,endTime, new Vector2(854,0), new Vector2(854, 240));

                var bottom = foreLayer.CreateSprite("sb/p.png",OsbOrigin.BottomCentre, new Vector2(320, 480));
                if(color != Color4.White || color != new Color4(255,255,255,255)) bottom.Color(startTime,color);
                bottom.ScaleVec(OsbEasing.InOutSine, startTime,endTime, new Vector2(854,0), new Vector2(854, 240));
            }

            void toSquare(double startScaleIn, double endScaleIn, double startScaleOut, double endScaleOut, Color4 color)
            {
                var square = foreLayer.CreateSprite("sb/p.png");
                square.ScaleVec(startScaleIn, endScaleIn, new Vector2(854,854), new Vector2(200,200));
                square.Rotate(startScaleIn, endScaleIn, 0, ConvertToRadians(45));
                square.ScaleVec(endScaleIn, startScaleOut, new Vector2(200,200), new Vector2(180,180));
                square.ScaleVec(OsbEasing.InExpo,startScaleOut,endScaleOut,new Vector2(180,180), new Vector2(1708,1708)); 

            }

            void squareOut(double startTime, double endTime)
            {
                var square = backLayer.CreateSprite("sb/p.png");
                square.Scale(OsbEasing.InOutExpo, startTime, endTime, 50, 1200);
                square.Rotate(OsbEasing.InOutExpo, startTime, endTime, 0, ConvertToRadians(420));
                square.Fade(startTime,1);
                square.Fade(endTime, endTime + 650,1,0); 
            }

        #endregion

        #region Lyrics
            void setupLyrics()
            {
                
		        List<Lyric> Lyrics = JsonConvert.DeserializeObject<List<Lyric>>(File.ReadAllText($"{ProjectPath}/lyrics.json"));

                foreach(var lyric in Lyrics)
                {
                    if(lyric.kiai)
                    {
                        generateKiaiLyric(lyric);
                    }else{
                        switch(lyric.Color)
                        {
                            case "white":
                                generateLyric(lyric, whiteFont);
                                break;
                            case "black":
                                generateLyric(lyric, blackFont);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            void generateLyric(Lyric lyric, FontGenerator font) 
            {
                var fontTexture = font.GetTexture(lyric.Sentence.ToUpper());
                var sprite = foreLayer.CreateSprite(fontTexture.Path);
                sprite.MoveY(OsbEasing.InExpo, lyric.startTime - 100, lyric.startTime, 200, 235);
                sprite.MoveY(lyric.startTime, lyric.endTime- 100, 235,245);
                sprite.MoveY(OsbEasing.InExpo, lyric.endTime - 100, lyric.endTime, 245, 350);
                sprite.Fade(lyric.startTime - 100, lyric.startTime, 0,1);
                sprite.Fade(lyric.endTime - 100, lyric.endTime, 1, 0);
                sprite.Scale(lyric.startTime - 100, 0.25); 
            }

            void generateKiaiLyric(Lyric lyric)
            {
                var fontTexture = kiaiFont.GetTexture(lyric.Sentence.ToUpper());
                var sprite = foreLayer.CreateSprite(fontTexture.Path);
                sprite.MoveY(OsbEasing.InExpo, lyric.startTime - 100, lyric.startTime, 350, 245);
                sprite.MoveY(lyric.startTime, lyric.endTime- 100, 245,235);
                sprite.MoveY(OsbEasing.InExpo, lyric.endTime - 100, lyric.endTime, 235, 200);
                sprite.Fade(lyric.startTime - 100, lyric.startTime, 0,1);
                sprite.Fade(lyric.endTime - 100, lyric.endTime, 1, 0);
                sprite.Scale(lyric.startTime - 100, 0.35); 
            }

            void setupFonts()
            {
                whiteFont = LoadFont("sb/f/w", new FontDescription() 
                {
                    FontPath = "fonts/V-GERB(bold).ttf",
                    Color = Color4.White
                });
                blackFont = LoadFont("sb/f/b", new FontDescription() 
                {
                    FontPath = "fonts/V-GERB(bold).ttf", 
                    Color = Color4.Black
                });

                creditFont = LoadFont("sb/f/cr", new FontDescription() 
                {
                    FontPath = "fonts/V-GERB(bold).ttf", 
                    Color = Color4.White
                });

                kiaiFont = LoadFont("sb/f/k", new FontDescription()
                {
                    FontPath = "fonts/ostrich-regular.ttf",
                    FontSize = 120,
                    Color = Color4.Black
                });
            }
        #endregion

        #region Collab
            void collabNames()
            {
                switch(Beatmap.Name)
                {
                    case "Neil x Ameth's Conflux":
                        generateNames("conflux");
                        break;
                    case "Testo x Kalibe's Insane":
                        generateNames("insane");
                        break;
                    default:
                        break;
                }
            }

            void generateNames(string fileName) 
            {
                setupCollabFont();
                List<collab> collabs = JsonConvert.DeserializeObject<List<collab>>(File.ReadAllText($"{ProjectPath}/{fileName}.json"));
                var namePlate = hitLayer.CreateSprite("sb/p.png");
                namePlate.Rotate(0,ConvertToRadians(45));
                namePlate.MoveY(0,480);
                namePlate.Fade(0,0);
                namePlate.Color(0,Color4.Black);
                foreach(var mapper in collabs)
                {
                    if(mapper.startTime != 77237 && mapper.startTime != 143444)
                    {
                        namePlate.Scale(OsbEasing.InExpo, mapper.startTime, mapper.startTime + 500, 150,200);
                        namePlate.Fade(OsbEasing.InExpo, mapper.startTime, mapper.startTime + 500, 0, 0.9);
                        if(mapper.endTime == 49651)
                        {
                            namePlate.Fade(48616 - 500, 48616, 0.9, 0);
                            namePlate.Scale(OsbEasing.InExpo, 48616 - 500, 48616, 200, 0);
                        }else{
                            namePlate.Fade(mapper.endTime - 500, mapper.endTime, 0.9, 0);
                            namePlate.Scale(OsbEasing.InExpo, mapper.endTime - 500, mapper.endTime, 200, 0);
                        }
                    }
  
                    var mapperTexture = collabFont.GetTexture(mapper.Mapper.ToUpper());
                    var sprite = hitLayer.CreateSprite(mapperTexture.Path);
                    sprite.MoveY(mapper.startTime, 440);
                    sprite.ScaleVec(OsbEasing.InExpo, mapper.startTime, mapper.startTime + 500, new Vector2(0, 0.25f), new Vector2(0.25f,0.25f));
                    if(mapper.endTime == 49651)
                        {
                            sprite.Fade(48616 - 500, 48616, 0.9, 0);
                            sprite.ScaleVec(OsbEasing.InExpo, 48616 - 500, 48616, new Vector2(0.25f,0.25f), new Vector2(0, 0.25f));
                        }else{
                            sprite.Fade(mapper.endTime - 500, mapper.endTime, 0.9, 0);
                            sprite.ScaleVec(OsbEasing.InExpo, mapper.endTime - 500, mapper.endTime, new Vector2(0.25f,0.25f), new Vector2(0, 0.25f));
                        }

                }
            }

            void setupCollabFont()
            {
                collabFont = LoadFont("sb/f/c", new FontDescription() 
                {
                    FontPath = "fonts/V-GERB(bold).ttf",
                    Color = Color4.White
                });
                collabFont.GetTexture("NeilPerry".ToUpper());
                collabFont.GetTexture("Testo".ToUpper());
                collabFont.GetTexture("Ameth Rianno".ToUpper());
                collabFont.GetTexture("Kalibe".ToUpper());
               
            }
        #endregion

        void credits()
        {
            var line = backLayer.CreateSprite("sb/p.png");
            
            line.ScaleVec(OsbEasing.OutExpo,165513, 166547, new Vector2(0,5), new Vector2(450,5));
            line.Fade(165513,1);

            line.ScaleVec(176547, 187237, new Vector2(450,5), new Vector2(250,250));
            line.ScaleVec(187409, 187582, new Vector2(250,250), new Vector2(270,270));
            line.Rotate(176547, 187237,0,ConvertToRadians(360 + 45));
            line.Fade(187582,0);


            var songTitle = "Beggars (Consols Remix)";
            var songArtist = "Krewella x DISKORD";
            var setHost = "NeilPerry";
            var difficultyName = "";
            var difficultyMapper = "";

            switch(Beatmap.Name)
            {
                case "Djulus' Normal":
                    difficultyName = "Normal";
                    difficultyMapper = "Djulus";
                break;
                case "SchoolBoy's Advanced":
                    difficultyName = "Advanced";
                    difficultyMapper = "Schoolboy";
                break;
                case "Testo x Kalibe's Insane":
                    difficultyName = "Insane";
                    difficultyMapper = "Testo x Kalibe";
                break;
                case "Neil x Ameth's Conflux":
                    difficultyName = "Conflux";
                    difficultyMapper = "NeilPerry x Ameth Rianno";
                break;
                default:
                    difficultyName = Beatmap.Name;
                    difficultyMapper = "NeilPerry";
                break;
            }
            creditFont.GetTexture("Normal".ToUpper());
            creditFont.GetTexture("Djulus".ToUpper());
            creditFont.GetTexture("Advanced".ToUpper());
            creditFont.GetTexture("Schoolboy".ToUpper());
            creditFont.GetTexture("Insane".ToUpper());
            creditFont.GetTexture("Testo x Kalibe".ToUpper());
            creditFont.GetTexture("Conflux".ToUpper());
            creditFont.GetTexture("NeilPerry x Ameth Rianno".ToUpper());
            creditFont.GetTexture("Hard".ToUpper());
            creditFont.GetTexture("NeilPerry".ToUpper());

            
            var difficultyMapperTexture = creditFont.GetTexture(difficultyMapper.ToUpper());
            var difficultyNameTexture = creditFont.GetTexture(difficultyName.ToUpper());
            var setHostTexture = creditFont.GetTexture(setHost.ToUpper());
            var setTexture = creditFont.GetTexture("MAPSET");
            var songTitleTexture = creditFont.GetTexture(songTitle.ToUpper());
            var songArtistTexture = creditFont.GetTexture(songArtist.ToUpper());

            var storyboardTexture = creditFont.GetTexture("STORYBOARD");
            var storyboarderTexture = creditFont.GetTexture("COPPERTINE");

            
            generateCredit(backLayer, songTitleTexture, songArtistTexture, 165513, 166030, 168099, 168271);
            generateCredit(backLayer, setTexture, setHostTexture, 168271, 168616, 170858, 171030);
            generateCredit(hitLayer, difficultyNameTexture, difficultyMapperTexture, 171030, 171375, 173616, 173789);
            generateCredit(backLayer, storyboardTexture, storyboarderTexture, 173789, 174134, 176375, 176547);
        }

        void afterCreditBuildUp()
        {
            var square = backLayer.CreateSprite("sb/p.png"); 
            square.Rotate(187582, ConvertToRadians(45));
            square.StartLoopGroup(187582, 20);
                square.Scale(OsbEasing.OutExpo, 0, tick(0, 1), 270, 250);
            square.EndGroup();

            square.StartLoopGroup(194478, 8);
                square.Scale(OsbEasing.OutExpo, 0, tick(0, 2), 270, 250);
            square.EndGroup();

            var scaleCurr = 250;
            var rotateCurr = ConvertToRadians(45);
            for(double time = 195858; time <= 197237; time += tick(0,4))
            {
                square.Scale(OsbEasing.OutExpo, time, time + tick(0, 4), scaleCurr + 70, scaleCurr + 50);
                
                scaleCurr += 50;

            }

            square.Rotate(195858, 197237, ConvertToRadians(45), ConvertToRadians(45*7));
            //square.StartLoopGroup(195858, 8);
            //    square.Scale(OsbEasing.OutExpo, 0, tick(0, 4), 270, 250);
            //square.EndGroup();

            generateCluster(187582, 190340);
            generateCluster(190340, 192754);

            for(var time = 193099; time <= 194478; time += (int)tick(0,1))
            {
                generateCluster(time, time + tick(0,1));
            }

            for(var time = 194478; time <= 195858; time += (int)tick(0,2))
            {
                generateCluster(time, time + tick(0,2));
            }

            toSquare(197237, 197409, 198444, 198616, Color4.White);
        }

        void generateCluster(double startTime, double endTime)
        {
            
            var xCenter = Random(-107, 747);
            var yCenter = Random(0,480);

            for(var i = 0; i <= Math.Floor((decimal)Random(5, 15)); i++)
            {
                var angle = Random(0, Math.PI*2);
                var radius = Random(25, 100);
                var position = new Vector2(
                    (float)xCenter +  (float)Math.Cos(angle) *  (float)radius,
                    (float)yCenter +  (float)Math.Sin(angle) *  (float)radius
                );

                var square = backLayer.CreateSprite("sb/p.png");
                square.Scale(startTime,10);
                square.Fade(startTime, startTime + 110,0,1);
                square.Rotate(OsbEasing.InExpo,startTime, endTime, Random(0, Math.PI*2),angle);
                square.Move(OsbEasing.OutExpo, startTime, startTime + 20, new Vector2(xCenter, yCenter), position);
                square.Move(OsbEasing.InSine, startTime + 20, endTime + tick(0,1) * 2, position, new Vector2(320,240));
            }
        }

        void generateCredit(StoryboardLayer layer, FontTexture topTexture, FontTexture bottomTexture, double startMoveOut, double endMoveOut, double startMoveIn, double endMoveIn)
        {
            var top = layer.CreateSprite(topTexture.Path);
            top.Scale(startMoveOut,0.25);
            top.MoveY(OsbEasing.OutExpo,startMoveOut,endMoveOut,240, 240 - (topTexture.BaseHeight * 0.25));
            top.Fade(startMoveOut, endMoveOut, 0,1);
            top.MoveY(endMoveOut,startMoveIn, 240 - (topTexture.BaseHeight * 0.25), 240 - (topTexture.BaseHeight * 0.2));
            top.MoveY(startMoveIn,endMoveIn, 240 - (topTexture.BaseHeight * 0.2), 240);
            top.Fade(startMoveIn, endMoveIn, 1,0);

            var bottom = layer.CreateSprite(bottomTexture.Path);
            bottom.Scale(startMoveOut,0.25);
            bottom.MoveY(OsbEasing.OutExpo,startMoveOut,endMoveOut,240, 240 + (bottomTexture.BaseHeight * 0.25));
            bottom.Fade(startMoveOut, endMoveOut, 0,1);
            bottom.MoveY(endMoveOut,startMoveIn, 240 + (bottomTexture.BaseHeight * 0.25), 240 + (bottomTexture.BaseHeight * 0.2));
            bottom.MoveY(startMoveIn,endMoveIn, 240 + (bottomTexture.BaseHeight * 0.2), 240);
            bottom.Fade(startMoveIn, endMoveIn, 1,0);
        }

        private void Divide(OsbSprite[] sprites, double angle, int time)
        {
            for(int i = 0; i < 2; i++)
            {
                Vector2 angleVector = new Vector2(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle)
                );

                foreach(var sprite in sprites)
                {
                    Vector2 position = sprite.PositionAt(time);
                    var rotation = sprite.RotationAt(time);
                    var scale = sprite.ScaleAt(time);

                    var nSprite = GetLayer(i.ToString()).CreateSprite(sprite.TexturePath, sprite.Origin);
                    nSprite.ScaleVec(time, scale.X, scale.Y);
                    nSprite.Rotate(time, rotation);
                    nSprite.Move(OsbEasing.OutExpo, time, time + 500, position, i == 0 ? position + angleVector * 25 : position - angleVector * 25);
                    nSprite.Fade(time, time + 500, 0.6, 0);
                    nSprite.Color(time, i == 0 ? Color4.Cyan : Color4.Red);

                }
            }
        }
        double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        double tick(double time,double divisor)
        {
            return Beatmap.GetTimingPointAt((int)time).BeatDuration / divisor;
        }

        public Vector2 GetTrackedLocation(float x, float y)
        {
            var MoveAmount = .078f;
            var midX = 320f;
            var midY = 240f;

            var newX = -(midX - x)*MoveAmount + midX;
            var newY = -(midY - y)*MoveAmount + midY;

            return new Vector2(newX, newY);
        }


    }

}

