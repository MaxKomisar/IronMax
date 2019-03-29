using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Iron_Max.Particle;

namespace Iron_Max
{
    class Level : IDisposable
    {
        private Tile[,] tiles;
        Texture2D gameOver;
        public float cameraPositionYAxis;
        private const int EntityLayer = 1;
        private float cameraPosition;
        private Layers[] layers;
        public List<Bullet_Enemy> bulletsEnemyis = new List<Bullet_Enemy>();
        public Player Player
        {
            get { return player; }
        }
        Player player;

        public ParticleSystem ParticleSysytem
        {
            get { return particleSystem; }
        }
        ParticleSystem particleSystem;

        public GParticleSystem GParticleSystem
        {
            get { return gParticleSystem; }
        }

        GParticleSystem gParticleSystem;

        public List<Gem> gems = new List<Gem>();
        public List<Enemy> enemies = new List<Enemy>();
        public List<EnemyTank> enemiesTank = new List<EnemyTank>();
        public List<EnemyRR> enemyRobot = new List<EnemyRR>();
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);
        private Random random = new Random(354668); // Arbitrary, but constant seed
        public int Score
        {
            get { return score; }
        }
        int score;
        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;
        private const int PointsPerSecond = 5;
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;
       // private SoundEffect exitReachedSound;
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            content = new ContentManager(serviceProvider, "Content");
            particleSystem = new ParticleSystem();
            gParticleSystem = new Particle.GParticleSystem();
            LoadTiles(fileStream);
            layers = new Layers[2];
            gameOver = Content.Load<Texture2D>("Game-Over_Block");
            layers[0] = new Layers(Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layers(Content, "Backgrounds/Layer1", 0.5f);
            particleSystem.LoadContent(content);
            gParticleSystem.LoadContent(content);
          //  exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }
		
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count]; // Block

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }
        
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'G':
                    return LoadGemTile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("t2", TileCollision.Platform);

                // Various enemies
                case 'A':
                    return LoadEnemyTankTile(x, y, "MonsterA");
                case 'B':
                    return LoadEnemyTile(x, y, "MonsterB");
                case 'C':
                    return LoadEnemyTile(x, y, "MonsterC");
                case 'D':
                    return LoadEnemyTile(x, y, "MonsterD");
                case 'R':
                    return LoadEnemyTileRedRobot(x,y,"MonsterH"); //H

                // Platform block
                case '~':
                    return LoadVarietyTile("t", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("t", 2, TileCollision.Passable);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("t", 7, TileCollision.Impassable);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }
        
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }
      
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }

        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadEnemyTileRedRobot(int x,int y,string spriteset)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemyRobot.Add(new EnemyRR(this, position, spriteset));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadEnemyTankTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemiesTank.Add(new EnemyTank(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadGemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            gems.Add(new Gem(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        public void Dispose()
        {
            Content.Unload();
        }


        #region Bounds and collision

        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

public void Update(GameTime gameTime,KeyboardState keyboardState,GamePadState gamePadState,AccelerometerState accelState,
            DisplayOrientation orientation)
        {
           
                if (!GamePage.Pause)
                {
                    // Pause while the player is dead or time is expired.
                    if (!Player.IsAlive)// || TimeRemaining == TimeSpan.Zero)
                    {
                        // Still want to perform physics on the player.
                        Player.ApplyPhysics(gameTime);
                    }
                    else
                    {
                        // timeRemaining -= gameTime.ElapsedGameTime;
                        Player.Update(gameTime, keyboardState, gamePadState, accelState, orientation);
                        UpdateGems(gameTime);
                        particleSystem.Update(gameTime);
                        gParticleSystem.Update(gameTime);
                        // Falling off the bottom of the level kills the player.
                        if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                            OnPlayerKilled(null);

                        //  OnPlayerKilledTank(null);
                        UpdateEnemies(gameTime);

                        // The player has reached the exit if they are standing on the ground and
                        // his bounding rectangle contains the center of the exit tile. They can only
                        // exit when they have collected all of the gems.
                        if (Player.IsAlive &&
                            Player.IsOnGround &&
                            Player.BoundingRectangle.Contains(exit))
                        {
                            OnExitReached();
                            
                        }

                        
                    }
                }
           
        }

        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; ++i)
            {
               
                Gem gem = gems[i];

                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i--);
                    particleSystem.EngineRocket(new Vector2(gem.Position.X, gem.Position.Y));
                    OnGemCollected(gem, Player);
                }
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {

                if (enemy.Alive)
                {
                    enemy.Update(gameTime);

                    // Touching an enemy instantly kills the player
                    if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                    {
                        OnPlayerKilled(enemy);
                        //   this.Dispose();
                        //this.Content.Dispose();
                    }
                }
            }

            foreach(EnemyTank tanks in enemiesTank)
            {
                if(tanks.Alive)
                {
                    tanks.Update(gameTime);

                    if(tanks.BoundingRectangle.Intersects(Player.BoundingRectangle))
                    {
                        OnPlayerKilledTank(tanks);
                    }
                }
            }

            foreach(EnemyRR redR in enemyRobot)
            {
                if(redR.Alive)
                {
                    redR.Update(gameTime);

                    if(redR.BoundingRectangle.Intersects(Player.BoundingRectangle))
                    {
                        OnPlayerKilledRedRobot(redR);
                    }
                }
            }
            foreach(Bullet_Enemy bulletEnemy in bulletsEnemyis)
            {
                if(bulletEnemy.alive)
                {
                   if(bulletEnemy.rectangle.Intersects(Player.BoundingRectangle))
                   {
                       OnPlayerKilledBullets(bulletEnemy);
                      
                   }
                }
            }

        }
       
        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += Gem.PointValue;

            gem.OnCollected(collectedBy);
        }

        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
            
        }
        private void OnPlayerKilledTank(EnemyTank killedBy)
        {
            Player.OnKilledTank(killedBy);
        }

        private void OnPlayerKilledBullets(Bullet_Enemy killedBy)
        {
            Player.OnKilledBullets(killedBy);
        }

        private void OnPlayerKilledRedRobot(EnemyRR killedBy)
        {
            
            
            Player.OnKilledRedRobot(killedBy);
        }

        private void OnExitReached()
        {
            Player.OnReachedExit();
           // exitReachedSound.Play();
            
            reachedExit = true;
        }

        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            
            spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            //Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, -cameraPositionYAxis, 0.0f);
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, cameraTransform);
            DrawTiles(spriteBatch);

            foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            particleSystem.Draw(spriteBatch);
            gParticleSystem.Draw(spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            foreach (EnemyTank tank in enemiesTank)
                tank.Draw(gameTime, spriteBatch);

            foreach (EnemyRR r in enemyRobot)
                r.Draw(gameTime,spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();
            
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);

            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                   
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.4f;
            
            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;
          
            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;
            

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);

            const float TopMargin = 0.3f;
            const float BottomMargin = 0.1f;
            float marginTop = cameraPositionYAxis + viewport.Height * TopMargin;
            float marginBottom = cameraPositionYAxis + viewport.Height - viewport.Height * BottomMargin;
            // Calculate how far to vertically scroll when the player is near the top or bottom of the screen.    

            float cameraMovementY = 0.0f;
            if (Player.Position.Y < marginTop) //above the top margin    
                cameraMovementY = Player.Position.Y - marginTop;
            else if (Player.Position.Y > marginBottom) //below the bottom margin    
                cameraMovementY = Player.Position.Y - marginBottom;

            float maxCameraPositionYOffset = Tile.Height * Height - viewport.Height;
            cameraPositionYAxis = MathHelper.Clamp(cameraPositionYAxis + cameraMovementY, 0.0f, maxCameraPositionYOffset);    
   

        }
    

        #endregion
    }
}
