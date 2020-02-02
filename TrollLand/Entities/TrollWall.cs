using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace TrollLand.Entities
{
    [Tracked(false)]
    [CustomEntity("TrollLand/TrollWall")]
    public class TrollWall : Solid
    {
        public string IdString;

        private const float _accelerationTime = 0.25f;
        private const float _shakeTime = 0.5f;
        private const float _pauseTime = 0.25f;

        private readonly Vector2[] _path;
        private readonly float _maxSpeed;
        private readonly bool _doPause;

        private float _speed = 0f;
        private float _speedPercent = 0f;
        private int _pathIndex = 0;
        private TileGrid _tilegrid;
        private SoundSource _shakingSfx;
        private Vector2 _shake;
        private Coroutine _sequemce;
        private float _pauseTimer = 0f;

        public TrollWall(Vector2[] path, int width, int height, char tileType, float maxSpeed, string idString, bool doPause, bool underFg) : base(path[0], width, height, false)
        {
            if (underFg)
            {
                Depth = Depths.FGTerrain + 1;
            }
            else
            {
                Depth = -10501;
            }
            IdString = idString;
            _path = path;
            _maxSpeed = maxSpeed;
            _doPause = doPause;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            Add(_tilegrid = GFX.FGAutotiler.GenerateBox(tileType, width / 8, height / 8).TileGrid);
            Add(_shakingSfx = new SoundSource());
        }

        public TrollWall(EntityData data, Vector2 offset) : 
            this(data.NodesWithPosition(offset), data.Width, data.Height, data.Attr("tiletype", "3")[0], data.Float("maxSpeed", 30f), data.Attr("idString"), data.Bool("doPause", true), data.Bool("underFg"))
        {
        }

        public override void Update()
        {
            base.Update();
            _tilegrid.Position = Shake;
            if (_pauseTimer > 0)
            {
                _pauseTimer -= Engine.DeltaTime;
            }
            if (_pathIndex > 0 && _pathIndex < _path.Length && _pauseTimer <= 0f)
            {
                if (_speed < _maxSpeed)
                {
                    _speedPercent = Calc.Approach(_speedPercent, 1f, Engine.DeltaTime / _accelerationTime);
                    _speed = Ease.Linear(_speedPercent) * _maxSpeed;
                }
                Vector2 direction = _path[_pathIndex] - Position;
                float dist = (direction).Length();
                if (dist < Engine.DeltaTime * _speed)
                {
                    MoveTo(_path[_pathIndex]);
                    _pathIndex++;
                    if (_doPause || (!_doPause && _pathIndex == _path.Length))
                    {
                        _shakingSfx.Stop();
                        Audio.Play("event:/game/00_prologue/fallblock_first_impact", Position);
                        SceneAs<Level>().Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                        StartShaking(0.25f);
                        _pauseTimer = _pauseTime;
                        _speedPercent = 0f;
                    }
                }
                else
                {
                    Move(Engine.DeltaTime * _speed * direction.SafeNormalize());
                }
            }
        }

        public void StartSequence()
        {
            if (_sequemce == null)
            {
                Add(_sequemce = new Coroutine(Sequence()));
            }
        }

        private void Move(Vector2 vector)
        {
            MoveH(vector.X);
            MoveV(vector.Y);
        }

        private IEnumerator Sequence()
        {
            _shakingSfx.Play("event:/game/00_prologue/fallblock_first_shake");
            StartShaking(_shakeTime);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return _shakeTime;
            _shakingSfx.Param("release", 1f);
            _pathIndex++;
        }
    }
}
