using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AdventureHelper.Entities
{
    class MultipleNodeTrackSpinner : Entity
    {
        public float PauseTimer;

        /// <summary>
        /// Percentage along the track it currently travels through.
        /// </summary>
        public float Percent { get; private set; }

        /// <summary>
        /// Node points to travel along before returning to <see cref="Start"/>.
        /// </summary>
        public Vector2[] Path { get; private set; }

        /// <summary>
        /// Index of the current start position.
        /// </summary>
        public int CurrentStart { get; private set; }

        /// <summary>
        /// Time it takes to move from the current start position to the current end position.
        /// </summary>
        public float MoveTime { get; }

        /// <summary>
        /// Time it waits at each node before moving on.
        /// </summary>
        public float PauseTime { get; }

        /// <summary>
        /// Whether the spinner is moving or not.
        /// </summary>
        public bool Moving { get; private set; }

        /// <summary>
        /// Angle at which the spinner is facing.
        /// </summary>
        public float Angle { get; private set; }

        public MultipleNodeTrackSpinner(EntityData data, Vector2 offset)
        {
            this.Moving = true;
            base.Collider = new ColliderList(new Collider[]
            {
                new Circle(6f, 0f, 0f),
            });
            base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
            this.Path = new Vector2[data.Nodes.GetLength(0) + 1];
            this.Path[0] = data.Position + offset;
            for (int i=0; i<data.Nodes.GetLength(0); i++)
            {
                Path[i + 1] = data.Nodes[i] + offset;
            }
            this.MoveTime = data.Float("moveTime", 0.4f);
            this.PauseTime = data.Float("pauseTime", 0.2f);
            this.Angle = (this.Path[1] - this.Path[0]).Angle();
            this.Percent = 0f;
            this.CurrentStart = 0;
            this.UpdatePosition();
        }
        public virtual void OnPlayer(Player player)
        {
            bool flag = player.Die((player.Position - this.Position).SafeNormalize(), false, true) != null;
            if (flag)
            {
                this.Moving = false;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.OnTrackStart();
        }

        public void UpdatePosition()
        {
            var start = Path[this.CurrentStart];
            var end = Path[(CurrentStart + 1) % this.Path.Length];
            this.Position = Vector2.Lerp(start, end, Ease.SineInOut(this.Percent));
        }

        public override void Update()
        {
            base.Update();
            if (this.Moving)
            {
                bool stillPaused = this.PauseTimer > 0f;
                if (stillPaused)
                {
                    this.PauseTimer -= Engine.DeltaTime;
                    bool isUnpaused = this.PauseTimer <= 0f;
                    if (isUnpaused)
                    {
                        this.OnTrackStart();
                    }
                }
                else
                {
                    this.Percent = Calc.Approach(this.Percent, 1f, Engine.DeltaTime / MoveTime);
                    this.UpdatePosition();
                    bool reachedDestination = this.Percent >= 1f;
                    if (reachedDestination)
                    {
                        this.CurrentStart = (CurrentStart + 1) % this.Path.Length;
                        this.PauseTimer = PauseTime;
                        this.Percent = 0f;
                        this.OnTrackEnd();
                    }
                }
            }
        }

        public virtual void OnTrackStart()
        {
        }

        public virtual void OnTrackEnd()
        {
            this.Angle = (this.Path[(CurrentStart + 1) % this.Path.Length] - this.Path[CurrentStart]).Angle();
        }
    }
}
