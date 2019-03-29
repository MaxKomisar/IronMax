using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Iron_Max.Particle;

namespace Iron_Max
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    class Player
    {
        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private Animation shootAnimation;
        private Animation runShootAnimation;
        private Animation jumpShootAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        //private SpriteEffects flipBullet = SpriteEffects.None;
        private AnimationPlayer sprite;
        
        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive
        {
            get { return isAlive; }
            set { isAlive = value; }
        }
        bool isAlive;


        GameObject objectBulletIMax;
        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;
        
        public int lives = 190;
        

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 7250.0f;
        private const float GroundDragFactor = 0.8f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 2.0f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 4400.0f;
        private const float MaxFallSpeed = 1950.0f;
        private const float JumpControlPower = 0.24f; 

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 4.5f;
        private const Buttons JumpButton = Buttons.A;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height - 40);
            }
        }

        /// <summary>
        /// Constructors a new player.
        /// </summary>
        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {

            //objectBulletIMax = new GameObject[2];
            //for (int i = 0; i < 2; i++)
            //{
                objectBulletIMax = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Bullet_000"));
            //}

                // Load animated textures.
           
          
idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle_1"), 0.1f, false);
runShootAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/RunPlayer_Anim"), 0.1f, true);
shootAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Shoot_Anim"), 0.1f, true);
runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Iron_Max_Player_Run_Anim"), 0.1f, true);
jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Iron_Max_Jump_Anim_Player"), 0.1f, false);
celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
jumpShootAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/JumpShoot_Anim"), 0.1f, true);
dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die_"), 0.1f, false);

            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            lives = 190;
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
 public void Update(GameTime gameTime,KeyboardState keyboardState,GamePadState gamePadState,AccelerometerState accelState,DisplayOrientation orientation)
        {
            GetInput(keyboardState, gamePadState, accelState, orientation);
            UpdateBullets();
            ApplyPhysics(gameTime);
             

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    
                    if(keyboardState.IsKeyDown(Keys.Z))
                    {
                        sprite.PlayAnimation(runShootAnimation);
                        FireBullet();
                    }else 
                  sprite.PlayAnimation(runAnimation);
              
                }
                else if(gamePadState.IsButtonDown(Buttons.X) ||
                     keyboardState.IsKeyDown(Keys.Z) ||
                     keyboardState.IsKeyDown(Keys.I))
                {
                    FireBullet();
                    sprite.PlayAnimation(shootAnimation);
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                   
                }
            }
            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(
            KeyboardState keyboardState, 
            GamePadState gamePadState,
            AccelerometerState accelState, 
            DisplayOrientation orientation)
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
               
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

          
            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);
            if(isJumping == true & keyboardState.IsKeyDown(Keys.Z))
            {
                FireBullet();
                sprite.PlayAnimation(jumpShootAnimation);
            }
           
        }


        private void FireBullet()
        {
                if (!objectBulletIMax.alive)
                {
                    //And set it to alive.
                    objectBulletIMax.alive = true;

                    if (flip == SpriteEffects.FlipHorizontally) //Facing right
                    {
                        objectBulletIMax.position = new Vector2(Position.X - 350, Position.Y - 300);
                        Level.GParticleSystem.EngineRocket(new Vector2(Position.X - 150, Position.Y - 300));
                       // objectBulletIMax.rotation = -180;
                        objectBulletIMax.velocity = new Vector2(-40, 0);
                        
                    }
                    else //Facing left
                    {
                        objectBulletIMax.position = new Vector2(Position.X + 150, Position.Y - 300);
                       // objectBulletIMax.rotation = 0;
                        objectBulletIMax.velocity = new Vector2(40, 0);
                        Level.GParticleSystem.EngineRocket(new Vector2(Position.X + 150, Position.Y - 300));
                    }

                    return;
                }
        }
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    sprite.PlayAnimation(jumpAnimation);
                    
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            if (lives == 0)
            {
                isAlive = false;
                if (killedBy != null)
                    killedSound.Play();
                else
                    fallSound.Play();
                sprite.PlayAnimation(dieAnimation);
            }
                lives--;
        }

        public void OnKilledTank(EnemyTank killedTankBy)
        {
            if (lives == 0)
            {
                isAlive = false;
                if (killedTankBy != null)
                    killedSound.Play();
                else
                    fallSound.Play();
                sprite.PlayAnimation(dieAnimation);
            }
            lives--;

        }

        public void OnKilledRedRobot(EnemyRR killedRedRobotBy)
        {
            if (lives == 0)
            {
                isAlive = false;
                if (killedRedRobotBy != null)
                    killedSound.Play();
                else
                    fallSound.Play();
                
                sprite.PlayAnimation(dieAnimation);
            }
            lives--;

        }
        public void OnKilledBullets(Bullet_Enemy killedBullets)
        {
            if (lives == 0)
            {
                isAlive = false;
                if (killedBullets != null)
                    killedSound.Play();
                else
                    fallSound.Play();
                sprite.PlayAnimation(dieAnimation);
            }
            lives--;
        }

        

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }



        private void UpdateBullets()
        {
                if (objectBulletIMax.alive)
                {
                    objectBulletIMax.position += objectBulletIMax.velocity;

                    //    (int)bullet.position.X,


                    foreach (Enemy enemy in level.enemies)
                    {
                        if (objectBulletIMax.rectangle.Intersects(enemy.BoundingRectangle))
                        {
                            enemy.Alive = false;
                            objectBulletIMax.alive = false;//kast_afrodita
                        }
                    }
                    foreach(EnemyTank tank in level.enemiesTank)
                    {
                        if(objectBulletIMax.rectangle.Intersects(tank.BoundingRectangle))
                        {
                            tank.Alive = false;
                            
                            objectBulletIMax.alive = false;
                        }
                    }
                    foreach(EnemyRR r in level.enemyRobot)
                    {
                        if(objectBulletIMax.rectangle.Intersects(r.BoundingRectangle))
                        {
                           // r.Alive = false;
                            objectBulletIMax.alive = false;

                        }
                    }

                    //    (int)bullet.position.Y)))

                        if(objectBulletIMax.position.X > Position.X + 1000 || objectBulletIMax.position.X < Position.X - 1000)
                    {
                        objectBulletIMax.alive = false;
                       // continue;
                    }

                    ////Collision rectangle for each bullet -Will also be
                    ////used for collisions with enemies.
                    Rectangle bulletRect = new Rectangle(
                        (int)objectBulletIMax.position.X - objectBulletIMax.sprite.Width * 2,
                        (int)objectBulletIMax.position.Y - objectBulletIMax.sprite.Height * 2,
                        objectBulletIMax.sprite.Width * 4,
                        objectBulletIMax.sprite.Height * 4);

                    //Everything below here can be deleted if you want
                    //your bullets to shoot through all tiles.

                    //Look for adjacent tiles to the bullet
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
                                    objectBulletIMax.alive = false;
                            }
                            
                        }
                    }
                }
            //}
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (objectBulletIMax.alive)
                {
                    //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
                    // Draw some sprites with different alpha tints
                    for (int i = 0; i < 2; i++)
                    {

                        spriteBatch.Draw(objectBulletIMax.sprite,new Vector2(objectBulletIMax.position.X + (i * 50),objectBulletIMax.position.Y), null, Color.White, objectBulletIMax.rotation, objectBulletIMax.center, 1.0f, flip, 0);
                        //_spriteBatch.Draw(_spriteTexture, new Vector2(i * 20, 100),
                        //new Color(Color.White, i * 12));
                    }
                    // End the sprite batch
                   // spriteBatch.End();


                        
                }
            if (Velocity.X > 0)
                flip = SpriteEffects.None;
            else if (Velocity.X < 0)
                flip = SpriteEffects.FlipHorizontally;
            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
    }
}
