using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Iron_Max
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    enum FaceDirectionTank
    {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class EnemyTank
    {
        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        /// 
        Bullet_Enemy[] bullets;

        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
        }

        bool alive;
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Animations
        private AnimationTank runAnimation;
        private AnimationTank idleAnimation;
        private AnimationTank dieAnimation;
        private AnimationEnemyTank sprite;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        private FaceDirectionTank direction = FaceDirectionTank.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private const float MaxWaitTime = 0.5f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private const float MoveSpeed = 64.0f;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public EnemyTank(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.position = position;
            this.alive = true;
            LoadContent(spriteSet);
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            bullets = new Bullet_Enemy[10];
            for (int i = 0; i < 10;i++ )
            {
                bullets[i] = new Bullet_Enemy(Level.Content.Load<Texture2D>("Sprites/Bullet_Enemy"));
            }
                
            runAnimation = new AnimationTank(Level.Content.Load<Texture2D>(spriteSet + "Tank"), 0.1f, true);
            idleAnimation = new AnimationTank(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new AnimationTank(Level.Content.Load<Texture2D>(spriteSet + "ex"), 0.10f, false);
            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }



        private void FireBullet()
        {
            foreach (Bullet_Enemy bullet in bullets)
            {
                if (Level.Player.Position.X - Position.X < 100 && Level.Player.Position.Y - Position.Y <= 50 && direction.Equals(FaceDirectionTank.Left) ||
                       Level.Player.Position.X > Position.X && Level.Player.Position.Y - Position.Y <= 50 && direction.Equals(FaceDirectionTank.Right))
                {
                    if (!bullet.alive)
                    {
                        //And set it to alive.
                        bullet.alive = true;
                        if (direction == FaceDirectionTank.Left) //Facing right
                        {
                            bullet.position = new Vector2(Position.X - 169, Position.Y - 236);

                            bullet.velocity = new Vector2(-40, 0);
                        }
                        else //Facing left
                        {
                            bullet.position = new Vector2(Position.X + 169, Position.Y - 236);
                            bullet.velocity = new Vector2(40, 0);
                        }
                    }
                }

                if(alive == false)
                {
                    bullet.alive = false;
                }
            }
        }

        private void UpdateBullets()
        {
            foreach(Bullet_Enemy bullet in bullets)
            { 
            if (bullet.alive)
            {
                bullet.position += bullet.velocity;

                if(bullet.rectangle.Intersects(Level.Player.BoundingRectangle))
                {
                    Level.Player.lives -= 1;
                    bullet.alive = false;
                    if(level.Player.lives <= 0)
                    {
                        level.Player.IsAlive = false;
                    }
                }

                if (bullet.position.X > Position.X + 1000 || bullet.position.X < Position.X - 1000)
                {
                    bullet.alive = false;
                }
                Rectangle bulletRect = new Rectangle((int)bullet.position.X - bullet.sprite.Width * 2,
                    (int)bullet.position.Y - bullet.sprite.Height * 2,
                    bullet.sprite.Width * 4,
                    bullet.sprite.Height * 4);

                Rectangle bounds = new Rectangle(
                    bulletRect.Center.X - 6,
                    bulletRect.Center.Y - 6,
                    bulletRect.Width / 4,
                    bulletRect.Height / 4);
                int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
                int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
                int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
                int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

                // For each potentially colliding tile
                for (int y = topTile; y <= bottomTile; ++y)
                {
                    for (int x = leftTile; x <= rightTile; ++x)
                    {
                        TileCollision collision = Level.GetCollision(x, y);

                        //If we collide with an Impassable or Platform tile
                        //then delete our bullet.
                        if (collision == TileCollision.Impassable ||
                            collision == TileCollision.Platform)
                        {
                            if (bulletRect.Intersects(bounds))
                                bullet.alive = false;
                        }
                    }
                }
            }
          }
        }

        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateBullets();
            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirectionTank)(-(int)direction);
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    // Move in the current direction.
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                }
            }

                FireBullet();
           
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Stop running when the game is paused or before turning around.
            if (!Level.Player.IsAlive ||
                Level.ReachedExit)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else if (!alive) //|| ((waitTime > 0) && !alive))
            {
                sprite.PlayAnimation(dieAnimation);
               // dead tank
              //  TimeSpan.FromSeconds(10);
                //dieAnimation.Texture.Dispose();
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }
            


            foreach(Bullet_Enemy bullet in bullets)
            {
                if(bullet.alive)
                {
                    spriteBatch.Draw(bullet.sprite, bullet.position, Color.White);
                }
            }
           
            
            
            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
    }
}
