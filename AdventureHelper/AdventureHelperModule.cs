using System;
using Microsoft.Xna.Framework;
using Celeste.Mod.AdventureHelper.Entities;
using Monocle;

namespace Celeste.Mod.AdventureHelper
{
    class AdventureHelperModule : EverestModule
    {

        public static AdventureHelperModule Instance;

        public AdventureHelperModule()
        {
            Instance = this;
        }

        public override Type SettingsType => null;
        public override Type SessionType => typeof(AdventureHelperSession);
        public static AdventureHelperSession Session => (AdventureHelperSession)Instance._Session;

        public static SpriteBank SpriteBank { get; private set; }

        public override void Load()
        {
            Everest.Events.Level.OnLoadEntity += LevelOnLoadEntity;
            AdventureHelperHooks.Load();
        }

        public override void Unload()
        {
            Everest.Events.Level.OnLoadEntity -= LevelOnLoadEntity;
            AdventureHelperHooks.Unload();
        }

        public override void LoadContent(bool firstLoad)
        {
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/KaydenFox/AdventureHelperSprites.xml");
        }

        private bool LevelOnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            if (entityData.Name.StartsWith("AdventureHelper"))
            {
                switch (entityData.Name)
                {
                    case "AdventureHelper/BladeTrackSpinnerMultinode":
                        level.Add(new BladeTrackSpinnerMultinode(entityData, offset));
                        return true;
                    case "AdventureHelper/DustTrackSpinnerMultinode":
                        level.Add(new DustTrackSpinnerMultinode(entityData, offset));
                        return true;
                    case "AdventureHelper/StarTrackSpinnerMultinode":
                        level.Add(new StarTrackSpinnerMultinode(entityData, offset));
                        return true;
                    case "AdventureHelper/ZipMoverNoReturn":
                        level.Add(new ZipMoverNoReturn(entityData, offset));
                        return true;
                    case "AdventureHelper/LinkedZipMover":
                        level.Add(new LinkedZipMover(entityData, offset));
                        return true;
                    case "AdventureHelper/LinkedZipMoverNoReturn":
                        level.Add(new LinkedZipMoverNoReturn(entityData, offset));
                        return true;
                    case "AdventureHelper/CustomCrystalHeart":
                        if ( !level.Session.HeartGem )
                        {
                            level.Add( new CustomCrystalHeart( entityData, offset ) );
                        }
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
