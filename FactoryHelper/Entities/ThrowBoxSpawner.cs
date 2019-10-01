using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Celeste;

namespace FactoryHelper.Entities
{
    class ThrowBoxSpawner : Entity
    {
        private Random _rnd = new Random();
        private float _delay;
        private int _maximum;
        private bool _isMetal;
        private bool _isRandom;
        private HashSet<ThrowBox> _boxes = new HashSet<ThrowBox>();

        public ThrowBoxSpawner(EntityData data, Vector2 offset) 
            : this(data.Position + offset, data.Float("delay", 5f), data.Int("maximum", 0), data.Bool("isMetal", false), data.Bool("isRandom",false))
        {
        }

        public ThrowBoxSpawner(Vector2 position, float delay, int maximum, bool isMetal, bool isRandom) : base(position)
        {
            _maximum = maximum;
            _delay = delay;
            _isMetal = isMetal;
            _isRandom = isRandom;
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
                ThrowBox crate = new ThrowBox(
                    position: new Vector2(Position.X, SceneAs<Level>().Bounds.Top - 15),
                    isMetal: _isRandom ? Calc.Random.Chance(0.5f) : _isMetal
                    );
                Scene.Add(crate);
                _boxes.Add(crate);
                crate.OnRemoved = () => _boxes.Remove(crate);
            }
            Console.WriteLine($"AFTER| Count:{_boxes.Count} | Max: {_maximum}");
        }
    }
}
