using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace TrollLand.Entities
{
    [Tracked(false)]
    [CustomEntity("TrollLand/TrollFakeWall")]
    public class TrollFakeWall : Entity
    {

        private char _fillTile;
        private TileGrid _tiles;
        private bool _fade;
        private EffectCutout _cutout;
        private float _transitionStartAlpha;
        private bool _transitionFade;
        private bool _playRevealWhenTransitionedInto;

        public TrollFakeWall(Vector2 position, char tile, float width, float height)
            : base(position)
        {
            _fillTile = tile;
            Collider = new Hitbox(width, height);
            Depth = -13000;
            Add(_cutout = new EffectCutout());
        }

        public TrollFakeWall(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height)
        {
            _playRevealWhenTransitionedInto = data.Bool("playTransitionReveal");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)X / 8 - tileBounds.Left;
            int y = (int)Y / 8 - tileBounds.Top;
            _tiles = GFX.FGAutotiler.GenerateOverlay(_fillTile, x, y, tilesX, tilesY, solidsData).TileGrid;
            Add(_tiles);
            Add(new TileInterceptor(_tiles, highPriority: false));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (CollideCheck<Player>())
            {
                _tiles.Alpha = 0f;
                _fade = true;
                _cutout.Visible = false;
                if (_playRevealWhenTransitionedInto)
                {
                    Audio.Play("event:/game/general/secret_revealed", base.Center);
                }
            }
            else
            {
                TransitionListener transitionListener = new TransitionListener();
                transitionListener.OnOut = OnTransitionOut;
                transitionListener.OnOutBegin = OnTransitionOutBegin;
                transitionListener.OnIn = OnTransitionIn;
                transitionListener.OnInBegin = OnTransitionInBegin;
                Add(transitionListener);
            }
        }

        private void OnTransitionOutBegin()
        {
            if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
            {
                _transitionFade = true;
                _transitionStartAlpha = _tiles.Alpha;
            }
            else
            {
                _transitionFade = false;
            }
        }

        private void OnTransitionOut(float percent)
        {
            if (_transitionFade)
            {
                _tiles.Alpha = _transitionStartAlpha * (1f - percent);
            }
        }

        private void OnTransitionInBegin()
        {
            Level level = SceneAs<Level>();
            if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value))
            {
                _transitionFade = true;
                _tiles.Alpha = 0f;
            }
            else
            {
                _transitionFade = false;
            }
        }

        private void OnTransitionIn(float percent)
        {
            if (_transitionFade)
            {
                _tiles.Alpha = percent;
            }
        }

        public override void Update()
        {
            base.Update();
            if (_fade)
            {
                _tiles.Alpha = Calc.Approach(_tiles.Alpha, 0f, 2f * Engine.DeltaTime);
                _cutout.Alpha = _tiles.Alpha;
                if (_tiles.Alpha <= 0f)
                {
                    RemoveSelf();
                }
                return;
            }
            Player player = CollideFirst<Player>();
            if (player != null && player.StateMachine.State != 9)
            {
                _fade = true;
                Audio.Play("event:/game/general/secret_revealed", Center);
            }
        }

        public override void Render()
        {
            Level level = base.Scene as Level;
            if (level.ShakeVector.X < 0f && level.Camera.X <= level.Bounds.Left && X <= level.Bounds.Left)
            {
                _tiles.RenderAt(Position + new Vector2(-3f, 0f));
            }
            if (level.ShakeVector.X > 0f && level.Camera.X + 320f >= level.Bounds.Right && X + Width >= level.Bounds.Right)
            {
                _tiles.RenderAt(Position + new Vector2(3f, 0f));
            }
            if (level.ShakeVector.Y < 0f && level.Camera.Y <= level.Bounds.Top && Y <= level.Bounds.Top)
            {
                _tiles.RenderAt(Position + new Vector2(0f, -3f));
            }
            if (level.ShakeVector.Y > 0f && level.Camera.Y + 180f >= level.Bounds.Bottom && Y + Height >= level.Bounds.Bottom)
            {
                _tiles.RenderAt(Position + new Vector2(0f, 3f));
            }
            base.Render();
        }
    }
}
