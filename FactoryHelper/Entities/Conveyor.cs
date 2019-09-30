using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using FactoryHelper.Components;
using System;

namespace FactoryHelper.Entities
{
    class Conveyor : Solid
    {
        public FactoryActivatorComponent Activator { get; }
        
        private const string _spriteRoot = "objects/FactoryHelper/conveyor/";
        private const float _beltFrequency = 0.025f;
        private const float _gearFrequency = 0.05f;
        private const float _conveyorMoveSpeed = 30.0f;

        private Sprite[] _edgeSprites = new Sprite[2];
        private Sprite[] _gearSprites = new Sprite[2];
        private Sprite[] _midSprites;

        private bool _isMovingLeft { get { return Activator.IsOn; } }

        public Conveyor(EntityData data, Vector2 offset) 
            : this (
                data.Position + offset,
                data.Width,
                data.Bool("startLeft", true),
                data.Attr("activationId", "")
            )
        {
        }

        public Conveyor(Vector2 position, float width, bool startLeft, string activationId) : base(position, width, 16, false)
        {
            Add(Activator = new FactoryActivatorComponent());
            Activator.StartOn = startLeft;
            Activator.ActivationId = activationId;
            Activator.OnTurnOff = ReverseConveyorDirection;
            Activator.OnStartOff = () => { StartAnimation(); ReverseConveyorDirection(); } ;
            Activator.OnTurnOn = ReverseConveyorDirection;
            Activator.OnStartOn = StartAnimation;

            for (int i = 0; i < 2; i++)
            {
                Add(_edgeSprites[i] = new Sprite(GFX.Game, _spriteRoot));
                _edgeSprites[i].Add("left", "belt_edge", _beltFrequency, "left");
                _edgeSprites[i].Add("right", "belt_edge", _beltFrequency, "right", 7, 6, 5, 4, 3, 2, 1, 0);
                _edgeSprites[i].Position = new Vector2((width - 16) * i, 0);

                Add(_gearSprites[i] = new Sprite(GFX.Game, _spriteRoot));
                _gearSprites[i].Add("left", "gear", _gearFrequency, "left");
                _gearSprites[i].Add("right", "gear", _gearFrequency, "right", 3, 2, 1, 0);
                _gearSprites[i].Position = new Vector2((width - 16) * i, 0);
            }
            
            _edgeSprites[1].Rotation = (float)Math.PI;
            _edgeSprites[1].Position += new Vector2(16,16);

            _midSprites = new Sprite[(int)width / 8 - 4];
            
            for (int i = 0; i < _midSprites.Length; i++)
            {
                Add(_midSprites[i] = new Sprite(GFX.Game, _spriteRoot));
                _midSprites[i].Add("left", "belt_mid", _beltFrequency, "left");
                _midSprites[i].Add("right", "belt_mid", _beltFrequency, "right", 7, 6, 5, 4, 3, 2, 1, 0);
                _midSprites[i].Position = new Vector2(16 + 8 * i, 0);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Activator.Added(scene);
        }

        public override void Update()
        {
            base.Update();
            foreach (ConveyorMoverComponent component in Scene.Tracker.GetComponents<ConveyorMoverComponent>())
            {
                if (Collide.Check(this, component.Entity, Position - Vector2.UnitY))
                {
                    component.Move((_isMovingLeft ? -_conveyorMoveSpeed : _conveyorMoveSpeed) * Engine.DeltaTime);
                }
            }
            if (HasPlayerClimbing())
            {
                Player player = GetPlayerClimbing();
                if (player != null)
                {
                    if (((player.CenterX < CenterX) && _isMovingLeft) || ((player.CenterX > CenterX) && !_isMovingLeft))
                    {
                        Console.WriteLine("Should move down");
                        player.Speed.Y = Calc.Approach(player.Speed.Y, 160f, 600f * Engine.DeltaTime);
                        player.LiftSpeed = Vector2.UnitY * Math.Min(player.Speed.Y, 80f);
                        Console.WriteLine($"Speed: {player.Speed.Y} | LiftSpeed: {player.LiftSpeed}");
                    }
                    else
                    {
                        Console.WriteLine("Should move up");
                        player.Speed.Y = Calc.Approach(player.Speed.Y, -160f, 600f * Engine.DeltaTime);
                        player.LiftSpeed = Vector2.UnitY * Math.Max(player.Speed.Y, -80f);
                        Console.WriteLine($"Speed: {player.Speed.Y} | LiftSpeed: {player.LiftSpeed}");
                    }
                    player.NaiveMove(player.Speed * Engine.DeltaTime);
                }
            }
        }

        private void StartAnimation()
        {
            PlayAnimationInArray(_midSprites);
            PlayAnimationInArray(_edgeSprites);
            PlayAnimationInArray(_gearSprites);
            _edgeSprites[1].SetAnimationFrame(4);
        }

        private void ReverseConveyorDirection()
        {
            ReverseAnimationInArray(_midSprites);
            ReverseAnimationInArray(_edgeSprites);
            ReverseAnimationInArray(_gearSprites);
        }

        private void ReverseAnimationInArray(Sprite[] array)
        {
            foreach (Sprite sprite in array)
            {
                sprite.Rate = -sprite.Rate;
            }
        }

        private void PlayAnimationInArray(Sprite[] array)
        {
            foreach (Sprite sprite in array)
            {
                sprite.Play("left");
            }
        }
    }
}
