using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;

namespace KaydenSpriteMod
{
    public class KaydenSpriteModule : EverestModule
    {
        public static KaydenSpriteModule Instance;
        public override Type SettingsType => typeof(KaydenSpriteSettings);
        public static KaydenSpriteSettings Settings => (KaydenSpriteSettings)Instance._Settings;
        public override Type SaveDataType => null;
        public override Type SessionType => null;

        public KaydenSpriteModule()
        {
            Instance = this;
        }

        public override void LoadContent( bool firstLoad )
        {
            base.LoadContent( firstLoad );
		}

        public override void Load()
        {
            On.Celeste.PlayerSprite.ctor += OnPlayerSprite_ctor;
            On.Celeste.LevelLoader.ctor += OnLevelLoader_ctor;
            On.Celeste.PlayerHair.GetHairColor += OnPlayerHair_GetHairColor;
            On.Celeste.Payphone.ctor += OnPayphone_ctor;
        }

        public override void Unload()
        {
            On.Celeste.PlayerSprite.ctor -= OnPlayerSprite_ctor;
            On.Celeste.LevelLoader.ctor -= OnLevelLoader_ctor;
            On.Celeste.PlayerHair.GetHairColor -= OnPlayerHair_GetHairColor;
            On.Celeste.Payphone.ctor -= OnPayphone_ctor;
        }

        private void OnPlayerSprite_ctor( On.Celeste.PlayerSprite.orig_ctor orig, Celeste.PlayerSprite self, PlayerSpriteMode mode )
        {
            orig( self, mode );
            if ( Settings.Enabled && ( self.Mode == PlayerSpriteMode.Madeline || self.Mode == PlayerSpriteMode.MadelineNoBackpack ) )
            {
                string id = "";
                switch ( self.Mode )
                {
                    case PlayerSpriteMode.Madeline:
                        id = "player_KaydenFox";
                        break;
                    case PlayerSpriteMode.MadelineNoBackpack:
                        id = "player_no_backpack_KaydenFox";
                        break;
                }

                DynData<Celeste.PlayerSprite> spriteDynData = new DynData<Celeste.PlayerSprite>( self );
                spriteDynData.Set( "spriteName", id );
                if ( !self.HasHair )
                {
                    Celeste.PlayerSprite.CreateFramesMetadata( id );
                }

                Celeste.GFX.SpriteBank.CreateOn( self, id );
            }
        }

        private void OnLevelLoader_ctor( On.Celeste.LevelLoader.orig_ctor orig, Celeste.LevelLoader self, Celeste.Session session, Vector2? startPosition )
        {
            orig( self, session, startPosition );
            Celeste.PlayerSprite.CreateFramesMetadata( "player_KaydenFox" );
            Celeste.PlayerSprite.CreateFramesMetadata( "player_no_backpack_KaydenFox" );
        }

        private Color OnPlayerHair_GetHairColor( On.Celeste.PlayerHair.orig_GetHairColor orig, Celeste.PlayerHair self, int index )
        {
            if ( Settings.Enabled && ( self.Entity is Celeste.Player ) )
            {
                Celeste.Player player = self.Entity as Celeste.Player;

                if ( player.StateMachine.State != 19 && ( player.Sprite.Mode == PlayerSpriteMode.Madeline || player.Sprite.Mode == PlayerSpriteMode.MadelineNoBackpack ) )
                {
                    int colorIndex = 1;
                    if ( player.Inventory.Dashes > 0 )
                    {
                        colorIndex = player.Dashes;
                    }
                    switch ( colorIndex )
                    {
                        case 0:
                            return new Color( 134, 135, 138 );
                        case 1:
                            return new Color( 78, 60, 42 );
                        case 2:
                            return new Color( 237, 194, 18 );
                    }
                }
            }
            return orig( self, index );
        }

        private void OnPayphone_ctor( On.Celeste.Payphone.orig_ctor orig, Celeste.Payphone self, Vector2 pos )
        {
            orig( self, pos );
            if ( Settings.Enabled )
            {
                self.Remove( self.Sprite );
                self.Add( self.Sprite = GFX.SpriteBank.Create( "payphone_KaydenFox" ) );
                self.Sprite.Play( "idle" );
            }
        }
    }
}