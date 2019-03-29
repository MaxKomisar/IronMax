using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Iron_Max
{
    public class GameSystemIMax : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Vector2 baseScreenSize = new Vector2(GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight);
        private Matrix globalTransformation;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        Texture2D hpLine;
        Texture2D GemGUI;
        // Global content.
        private SpriteFont hudFont;
        private Texture2D GUI_HP;
        private Texture2D winOverlay;
        private Texture2D gameOver;
        private Texture2D diedOverlay;


        //Virtual GamePad//
        private Texture2D virtualGamePadLeft;
        private Texture2D virtualGamePadRigth;
        private Texture2D virtualGamePadJump;
        private Texture2D virtualGamePadShoot;
        ///////////////////
        
        //Meta-level game state.
        public int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;
        //public bool launchedLevelGame = false;
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private TouchCollection touchState;
        private AccelerometerState accelerometerState;
       
        private VirtualGamePad virtualGamePad;
        private const int numberOfLevels = 10;
      

        public GameSystemIMax()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
  graphics.PreferredBackBufferHeight = 1800;
  graphics.PreferredBackBufferWidth = 3200;
  graphics.ApplyChanges();
  graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
  Accelerometer.Initialize();
  base.Initialize();
        }

     


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            gameOver = Content.Load<Texture2D>("Game-Over_Block");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
            
            hpLine = Content.Load<Texture2D>("Hpl");
            GUI_HP = Content.Load<Texture2D>("Sprites/GUI_hp");
            GemGUI = Content.Load<Texture2D>("Sprites/Gem");
            virtualGamePadJump = Content.Load<Texture2D>("Sprites/Virtual_GUI/jump");
            virtualGamePadLeft = Content.Load<Texture2D>("Sprites/Virtual_GUI/LRG");
            virtualGamePadRigth = Content.Load<Texture2D>("Sprites/Virtual_GUI/RLG");
            virtualGamePadShoot = Content.Load<Texture2D>("Sprites/Virtual_GUI/shoot");
            //Work out how much we need to scale our graphics to fill the screen
            float horScaling = GraphicsDevice.PresentationParameters.BackBufferWidth / baseScreenSize.X;
            float verScaling = GraphicsDevice.PresentationParameters.BackBufferHeight / baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 9.1f);
            globalTransformation = Matrix.CreateScale(screenScalingFactor);
            virtualGamePad = new VirtualGamePad(baseScreenSize, globalTransformation);
            //Content.Load<Texture2D>("Sprites/VirtualControlArrow"));
            LoadNextLevel();
        }
        private void HandleInput(GameTime gameTime)
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            touchState = TouchPanel.GetState();
            gamePadState = virtualGamePad.GetState(touchState, GamePad.GetState(PlayerIndex.One));
            accelerometerState = Accelerometer.GetState();
    bool continuePressed = keyboardState.IsKeyDown(Keys.Space) || touchState.AnyTouch();
  
            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
         

            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
               if (level.ReachedExit)
                        LoadNextLevel();
            }

         
            wasContinuePressed = continuePressed;
            virtualGamePad.Update(gameTime);
        }
        public void LoadNextLevel()
        {
            levelIndex = (levelIndex + 1) % numberOfLevels;
            if (level != null)
                level.Dispose();
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
            level = new Level(Services, fileStream, levelIndex);
        }
        


        public void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }
        public void DisposLevelBackToMenuLevel()
        {
            if (level != null)
            level.Dispose();
        }
        
        protected override void UnloadContent()
        {
            Content.Unload(); // TODO: Unload any non ContentManager content here
        }
        
        protected override void Update(GameTime gameTime)
        {
           
         HandleInput(gameTime);
        level.Update(gameTime, keyboardState, gamePadState,
                                 accelerometerState, Window.CurrentOrientation);
          if (level.Player.Velocity != Vector2.Zero)
               virtualGamePad.NotifyPlayerIsMoving();
          
          
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
           
             level.Draw(gameTime, spriteBatch);
             
             DrawHud();
           
            base.Draw(gameTime);
        }

        private void DrawHud()
        {
                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, globalTransformation);
                Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
                Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
                //Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                //                             titleSafeArea.Y + titleSafeArea.Height / 2.0f);

                Vector2 center = new Vector2(baseScreenSize.X / 2, baseScreenSize.Y / 2);
                if(level.Player.lives <= 100)
                {
                    spriteBatch.Draw(hpLine, new Rectangle(titleSafeArea.X+20, titleSafeArea.Y + 10, level.Player.lives, 30), Color.Red);
                }
                else if(level.Player.lives >= 100)
                 {
               spriteBatch.Draw(hpLine, new Rectangle(titleSafeArea.X+20, titleSafeArea.Y + 10, level.Player.lives, 30), Color.LawnGreen);
                 }
                spriteBatch.Draw(GUI_HP, new Rectangle(0, titleSafeArea.Y, 224, 44), Color.White);
                DrawShadowedString(hudFont, ": " + level.Score.ToString(), hudLocation + new Vector2(26.0f,57.0f), Color.YellowGreen);
                spriteBatch.Draw(GemGUI,new Rectangle(1,50,30,40),Color.White);
               // Draw System Virtual button
spriteBatch.Draw(virtualGamePadLeft, new Rectangle(titleSafeArea.X + 10, titleSafeArea.Y + 410, 120, 70),Color.White);
spriteBatch.Draw(virtualGamePadRigth, new Rectangle(titleSafeArea.X + 130, titleSafeArea.Y + 410, 120, 70), Color.White);
spriteBatch.Draw(virtualGamePadJump, new Rectangle(titleSafeArea.X + 550, titleSafeArea.Y + 410, 120, 70), Color.White);
spriteBatch.Draw(virtualGamePadShoot, new Rectangle(titleSafeArea.X + 680, titleSafeArea.Y + 410, 120, 70), Color.White);


foreach (var touch in touchState)
{
    if (touch.State == TouchLocationState.Moved || touch.State == TouchLocationState.Pressed)
    {
        //Scale the touch position to be in _baseScreenSize coordinates
        Vector2 pos = touch.Position;
        Vector2.Transform(ref pos, ref globalTransformation, out pos);

        if (pos.X >= 10 || pos.Y <= 410) // pos.X <= 10 || pos.Y >= 410)
        {
            keyboardState.IsKeyDown(Keys.Left);
        }

    }
}
                ///
                Texture2D status = null;
                    if (level.ReachedExit)
                    {
                         status = winOverlay;
                    }else if (!level.Player.IsAlive)// || level.Player.Lives <= 0) - work
                    {
                       status = diedOverlay;
                    }
                
                if (status != null)
                {
                    Vector2 statusSize = new Vector2(status.Width, status.Height);
                    spriteBatch.Draw(status, center - statusSize / 2, Color.White);
                }



                if (touchState.IsConnected)
                    virtualGamePad.Draw(spriteBatch);
                spriteBatch.End();
            
        }
        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }

    
}