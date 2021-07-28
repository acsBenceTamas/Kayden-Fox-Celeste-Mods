using System;
using System.Collections.Generic;
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

        /// <summary>
        /// The text representing the Pause Flag. When the Pause Flag is active, the spinner will stop moving.
        /// </summary>
        public String PauseFlag { get; private set; }

        /// <summary>
        /// The text representing the Pause Flag. When the Pause Flag is active, the spinner will stop moving.
        /// </summary>
        public bool HasPauseFlag { get; private set; }

        /// <summary>
        /// Tracks if the player has died to this entity.
        /// </summary>
        public bool playerDead { get; private set; }

        /// <summary>
        /// If set to true, this will cause the entity to halt its movement during a cutscene.
        /// </summary>
        public bool PauseOnCutscene { get; private set; }

        public MultipleNodeTrackSpinner(EntityData data, Vector2 offset)
        { 
            this.PauseOnCutscene = data.Bool("pauseOnCutscene"); 
            this.PauseFlag = data.Attr("pauseFlag");
            this.HasPauseFlag = !PauseFlag.Equals("");

            this.playerDead = false;
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
  
            playerDead = player.Die((player.Position - this.Position).SafeNormalize(), false, true) != null;
            if (playerDead)
            { 
                this.Moving = false;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (this.HasPauseFlag)
            {
                SceneAs<Level>().Session.SetFlag(PauseFlag, false);
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

            bool cutsceneRunning = false;
            if (this.PauseOnCutscene) {
                List<CutsceneEntity> cutScene = SceneAs<Level>().Entities.FindAll<CutsceneEntity>();
                foreach (CutsceneEntity element in cutScene)
                {
                    if (element.Running) { cutsceneRunning = true; }
                }
            }

            bool pauseFlag = false;
            if (this.HasPauseFlag)
            {
                pauseFlag = SceneAs<Level>().Session.GetFlag(PauseFlag);
            }

            if (!cutsceneRunning && !pauseFlag) { this.Moving = true; }
            else
            {
                this.Moving = false;
            }

            if (this.Moving && !playerDead)
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
