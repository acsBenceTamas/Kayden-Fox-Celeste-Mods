using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;

namespace FactoryHelper.Entities
{
    [CustomEntity("FactoryHelper/ThrowBoxSpawner")]
    class ThrowBoxSpawner : Entity
    {
        private Random _rnd = new Random();
        private float _delay;
        private int _maximum;
        private bool _isMetal;
        private bool _isRandom;
        private HashSet<ThrowBox> _boxes = new HashSet<ThrowBox>();
        private bool _fromTop;
        private bool _tutorial;

        public ThrowBoxSpawner(EntityData data, Vector2 offset) 
            : this(data.Position + offset, data.Float("delay", 5f), data.Int("maximum", 0), data.Bool("isMetal", false), data.Bool("isRandom",false), data.Bool("fromTop", true), data.Bool("tutorial", false))
        {
        }

        public ThrowBoxSpawner(Vector2 position, float delay, int maximum, bool isMetal, bool isRandom, bool fromTop, bool tutorial) : base(position)
        {
            _maximum = maximum;
            _delay = delay;
            _isMetal = isMetal;
            _isRandom = isRandom;
            _fromTop = fromTop;
            _tutorial = tutorial;
        }

        public override void Update()
        {
            base.Update();
            if (Scene.OnInterval(_delay))
            {
                TrySpawnThrowBox();
            }
        }

        private void TrySpawnThrowBox()
        {
            if (_maximum <= 0 || _boxes.Count < _maximum)
            {
                float posY = _fromTop ? SceneAs<Level>().Bounds.Top - 15 : Position.Y;
                float posX = _fromTop ? Position.X : GetClosestPositionH();
                ThrowBox crate = new ThrowBox(
                    position: new Vector2(posX, posY),
                    isMetal: _isRandom ? Calc.Random.Chance(0.5f) : _isMetal,
                    tutorial: _tutorial
                    );
                Scene.Add(crate);
                _boxes.Add(crate);
                crate.OnRemoved = () => _boxes.Remove(crate);
            }
        }

        private float GetClosestPositionH()
        {
            Level level = SceneAs<Level>();
            if (Math.Abs(Left - level.Bounds.Left) < Math.Abs(Right - level.Bounds.Right))
            {
                return level.Bounds.Left - 15f;
            }
            else
            {
                return level.Bounds.Right -1f;
            }
        }
    }
}
