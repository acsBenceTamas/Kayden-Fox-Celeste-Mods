using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/KillerDebris")]
    [Tracked]
    public class KillerDebris : Entity
    {

        private class Border : Entity
        {
            private Entity[] drawing = new Entity[2];

            public Border(Entity parent, Entity filler)
            {
                drawing[0] = parent;
                drawing[1] = filler;
                base.Depth = parent.Depth + 2;
            }

            public override void Render()
            {
                if (drawing[0].Visible)
                {
                    DrawBorder(drawing[0]);
                    DrawBorder(drawing[1]);
                }
            }

            private void DrawBorder(Entity entity)
            {
                if (entity != null)
                {
                    foreach (Component component in entity.Components)
                    {
                        Image image = component as Image;
                        if (image != null)
                        {
                            Color color = image.Color;
                            Vector2 position = image.Position;
                            image.Color = Color.Black;
                            image.Position = position + new Vector2(0f, -1f);
                            image.Render();
                            image.Position = position + new Vector2(0f, 1f);
                            image.Render();
                            image.Position = position + new Vector2(-1f, 0f);
                            image.Render();
                            image.Position = position + new Vector2(1f, 0f);
                            image.Render();
                            image.Color = color;
                            image.Position = position;
                        }
                    }
                }
            }
        }

        public static ParticleType P_Move;

        public const float ParticleInterval = 0.02f;

        public enum DebrisColor
        {
            Silver,
            Bronze
        }

        private static Dictionary<DebrisColor, string> fgTextureLookup = new Dictionary<DebrisColor, string>
    {
        {
            DebrisColor.Silver,
            "danger/FactoryHelper/debris/fg_silver"
        },
        {
            DebrisColor.Bronze,
            "danger/FactoryHelper/debris/fg_bronze"
        }
    };

        private static Dictionary<DebrisColor, string> bgTextureLookup = new Dictionary<DebrisColor, string>
    {
        {
            DebrisColor.Silver,
            "danger/FactoryHelper/debris/bg_silver"
        },
        {
            DebrisColor.Bronze,
            "danger/FactoryHelper/debris/bg_bronze"
        }
    };

        public bool AttachToSolid;

        private Entity _filler;

        private Border _border;

        private float _offset = Calc.Random.NextFloat();

        private bool _expanded;

        private int _randomSeed;

        private DebrisColor _color;

        public KillerDebris(Vector2 position, bool attachToSolid, string color)
            : base(position)
        {
            Enum.TryParse(color, out _color);
            Tag = Tags.TransitionUpdate;
            Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
            Visible = false;
            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            Add(new LedgeBlocker());
            Depth = -8500;
            AttachToSolid = attachToSolid;
            if (attachToSolid)
            {
                Add(new StaticMover
                {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    OnDestroy = RemoveSelf
                });
            }
            _randomSeed = Calc.Random.Next();
        }

        public KillerDebris(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("attachToSolid"), data.Attr("color", "bronze"))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (InView())
            {
                CreateSprites();
            }
        }

        public void ForceInstantiate()
        {
            CreateSprites();
            Visible = true;
        }

        public override void Update()
        {
            if (!Visible)
            {
                Collidable = false;
                if (InView())
                {
                    Visible = true;
                    if (!_expanded)
                    {
                        CreateSprites();
                    }
                }
            }
            else
            {
                base.Update();
                if (Scene.OnInterval(0.25f, _offset) && !InView())
                {
                    Visible = false;
                }
                if (Scene.OnInterval(0.05f, _offset))
                {
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Collidable = (Math.Abs(entity.X - X) < 128f && Math.Abs(entity.Y - Y) < 128f);
                    }
                }
            }
            if (_filler != null)
            {
                _filler.Position = Position;
            }
        }

        private bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            return X > camera.X - 16f && Y > camera.Y - 16f && X < camera.X + 320f + 16f && Y < camera.Y + 180f + 16f;
        }

        private void CreateSprites()
        {
            if (!_expanded)
            {
                Calc.PushRandom(_randomSeed);
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(fgTextureLookup[_color]);
                MTexture mTexture = Calc.Random.Choose(atlasSubtextures);
                if (!SolidCheck(new Vector2(X - 4f, Y - 4f)))
                {
                    Add(new Image(mTexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f));
                }
                if (!SolidCheck(new Vector2(X + 4f, Y - 4f)))
                {
                    Add(new Image(mTexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f));
                }
                if (!SolidCheck(new Vector2(X + 4f, Y + 4f)))
                {
                    Add(new Image(mTexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f));
                }
                if (!SolidCheck(new Vector2(X - 4f, Y + 4f)))
                {
                    Add(new Image(mTexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f));
                }
                List<Entity> entities = Scene.Tracker.GetEntities<KillerDebris>();
                foreach (KillerDebris item in entities)
                {
                    if (item != this && item.AttachToSolid == AttachToSolid && item.X >= X && (item.Position - Position).Length() < 24f)
                    {
                        AddSprite((Position + item.Position) / 2f - Position);
                    }
                }
                Scene.Add(_border = new Border(this, _filler));
                _expanded = true;
                Calc.PopRandom();
            }
        }

        private void AddSprite(Vector2 offset)
        {
            if (_filler == null)
            {
                Scene.Add(_filler = new Entity(Position));
                _filler.Depth = Depth + 1;
            }
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(bgTextureLookup[_color]);
            Image image = new Image(Calc.Random.Choose(atlasSubtextures));
            image.Position = offset;
            image.Rotation = Calc.Random.Choose(0, 1, 2, 3) * ((float)Math.PI / 2f);
            image.CenterOrigin();
            _filler.Add(image);
        }

        private bool SolidCheck(Vector2 position)
        {
            if (AttachToSolid)
            {
                return false;
            }
            List<Solid> list = Scene.CollideAll<Solid>(position);
            foreach (Solid item in list)
            {
                if (item is SolidTiles)
                {
                    return true;
                }
            }
            return false;
        }

        private void ClearSprites()
        {
            if (_filler != null)
            {
                _filler.RemoveSelf();
            }
            _filler = null;
            if (_border != null)
            {
                _border.RemoveSelf();
            }
            _border = null;
            foreach (Image item in Components.GetAll<Image>())
            {
                item.RemoveSelf();
            }
            _expanded = false;
        }

        private void OnShake(Vector2 pos)
        {
            foreach (Component component in Components)
            {
                if (component is Image)
                {
                    (component as Image).Position = pos;
                }
            }
        }

        private bool IsRiding(Solid solid)
        {
            return CollideCheck(solid);
        }

        private void OnPlayer(Player player)
        {
            player.Die((player.Position - Position).SafeNormalize());
        }

        private void OnHoldable(Holdable h)
        {
            h.HitSpinner(this);
        }

        public override void Removed(Scene scene)
        {
            if (_filler != null && _filler.Scene == scene)
            {
                _filler.RemoveSelf();
            }
            if (_border != null && _border.Scene == scene)
            {
                _border.RemoveSelf();
            }
            base.Removed(scene);
        }
    }
}
