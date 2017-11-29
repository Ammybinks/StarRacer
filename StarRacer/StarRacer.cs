////////////////////////////////////////////////////////////////
// Copyright 2013, CompuScholar, Inc.
//
// This source code is for use by the students and teachers who 
// have purchased the corresponding TeenCoder or KidCoder product.
// It may not be transmitted to other parties for any reason
// without the written consent of CompuScholar, Inc.
// This source is provided as-is for educational purposes only.
// CompuScholar, Inc. makes no warranty and assumes
// no liability regarding the functionality of this program.
//
////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using SpriteLibrary;


namespace StarRacer
{
    // This utility class represents one camera's view.
    // It is provided fully complete as part of the activity starter.

    // NOTE:  Although we build the Camera class for general-purpose
    // panning in world X and Y coordinates, we will only use the Y
    // coordinates for this game since we are scrolling vertically!
    // You may wish to use the Camera class in other games.
    class Camera
    {
        // the position of the upper-left coordinate of the camera
        public Vector2 UpperLeft = new Vector2();
        
        // these members are used to keep the camera view within
        // valid world coordinates
        public int ViewWidth = 0;
        public int ViewHeight = 0;
        public int WorldWidth = 0;
        public int WorldHeight = 0;
        
        // this method will prevent the camera's display from straying
        // outside the bounds of world coordinates
        public void LockCamera()
        {
            if (UpperLeft.X < 0)
                UpperLeft.X = 0;
            if ((UpperLeft.X + ViewWidth) > WorldWidth)
                UpperLeft.X = WorldWidth - ViewWidth;

            if (UpperLeft.Y < 0)
                UpperLeft.Y = 0;
            if ((UpperLeft.Y + ViewHeight) > WorldHeight)
                UpperLeft.Y = WorldHeight - ViewHeight;
        }

        // this utility method will return true if the specified object is at all 
        // visible within the camera view
        public bool IsVisible(Vector2 objectUpperLeft, int objectWidth, int objectHeight)
        {
            // all coordinates are in world coordinates
            Rectangle cameraRect = new Rectangle((int)UpperLeft.X, (int)UpperLeft.Y, ViewWidth, ViewHeight);
            Rectangle objectRect = new Rectangle((int)objectUpperLeft.X, (int)objectUpperLeft.Y, objectWidth, objectHeight);

            return cameraRect.Intersects(objectRect);
        }

    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public class StarRacer : Microsoft.Xna.Framework.Game
    {
        // all memeber variables are provided complete as part of the activity starter.
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // declare textures to hold all images used in the game
        Texture2D asteroidBig1Texture;
        Texture2D asteroidBig2Texture;
        Texture2D asteroidMed1Texture;
        Texture2D asteroidMed2Texture;
        Texture2D asteroidSmall1Texture;
        Texture2D asteroidSmall2Texture;
        Texture2D finishLineTexture;
        Texture2D gameOverlayTexture;
        Texture2D starsBackground1Texture;
        Texture2D starsBackground2Texture;
        Texture2D starsBackground3Texture;
        Texture2D starsBackground4Texture;
        Texture2D starship1Texture;
        Texture2D starship2Texture;

        // declare sprites for each player's star racer
        Sprite StarShip1;
        Sprite StarShip2;
        
        // declare lists to hold the asteroid sprites and the names of the asteroid images
        LinkedList<Sprite> Asteroids = new LinkedList<Sprite>() ;

        // declare lists to hold the starfield background sprites and image names
        LinkedList<Sprite> StarFields = new LinkedList<Sprite>() ;
        
        // declare sprites to hold the finish line and overlay images
        Sprite finishLine = new Sprite();
        Sprite overlay = new Sprite();

        // constant values to control the number of starfields, asteroids, rates of acceleration, etc
        const int NUM_STARFIELDS = 20;
        const int NUM_ASTERIODS = 50;
        const double ACCELERATION_FACTOR = 0.25;
        const int X_STEER_FACTOR = 5;
        const int VIEWPORT_HEIGHT = 600;
        const int VIEWPORT_WIDTH = 398;
        const int WORLD_WIDTH = 398;
        const int VIEW_PORT_SEPARATION = 4; // number of pixels between viewports
        const int WORLD_HEIGHT = NUM_STARFIELDS * VIEWPORT_HEIGHT;

        Random randomNumGen = new Random(DateTime.Now.Millisecond);

        // this enumeration identifies the possible types of screens the user can see
        enum GameScreen
        {
            TITLE = 0,
            PAUSED = 1,
            PLAYING = 2,
            GAMEOVER = 3,
            OPTIONS = 4
        }

        // which screen are we currently looking at?
        GameScreen currentScreen;

        // this flag is true if the game AI is controlling player 2
        bool isGameAIEnabled = false;

        // previous keyboard state
        KeyboardState oldKeyboardState;

        // previous gamepad states
        GamePadState oldGamePadState1;
        GamePadState oldGamePadState2;

        // declare viewports for player1, and player2
        Viewport leftViewport;
        Viewport rightViewport;

        // declare cameras to track player 1 and player 2
        Camera player1Camera;
        Camera player2Camera;

        // declare font for displaying messages
        SpriteFont gameFont;

        // this string will hold the "player X wins" message
        String gameOverMessage;

        // This method is provided fully complete as part of the activity starter.
        public StarRacer()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        // This method is provided fully complete as part of the activity starter.
        protected override void Initialize()
        {
            // make sure window is large enough for our VIEWPORT_HEIGHT and VIEWPORT_WIDTH
            graphics.PreferredBackBufferWidth = VIEWPORT_WIDTH * 2 + VIEW_PORT_SEPARATION;
            graphics.PreferredBackBufferHeight = VIEWPORT_HEIGHT;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            // call base initialize first to get LoadContent() called and make
            // all textures available for sprite setup
            base.Initialize();

            // create the Viewport and Camera for each player
            initializeViews();

            // create the overlay sprite
            overlay = new Sprite();
            overlay.SetTexture(gameOverlayTexture);
            overlay.UpperLeft = Vector2.Zero;

            // start with the title screen, of course
            currentScreen = GameScreen.TITLE;

        }

        // The student should complete this method during an activity.
        private void initializeViews()
        {

            leftViewport.X = 0;
            leftViewport.Y = 0;
            leftViewport.Width = VIEWPORT_WIDTH;
            leftViewport.Height = VIEWPORT_HEIGHT;

            rightViewport.X = VIEWPORT_WIDTH + 4;
            rightViewport.Y = 0;
            rightViewport.Width = VIEWPORT_WIDTH;
            rightViewport.Height = VIEWPORT_HEIGHT;

            player1Camera = new Camera();

            player1Camera.ViewHeight = VIEWPORT_HEIGHT;
            player1Camera.ViewWidth = VIEWPORT_WIDTH;
            player1Camera.WorldHeight = WORLD_HEIGHT;
            player1Camera.WorldWidth = WORLD_WIDTH;

            player2Camera = new Camera();

            player2Camera.ViewHeight = VIEWPORT_HEIGHT;
            player2Camera.ViewWidth = VIEWPORT_WIDTH;
            player2Camera.WorldHeight = WORLD_HEIGHT;
            player2Camera.WorldWidth = WORLD_WIDTH;

        }

        // This method is provided fully complete as part of the activity starter.
        private void startGame()
        {
            // create the starry background
            initializeStarField();

            // create random asteroids
            initializeAsteroids();

            // create star ships
            initializeStarShips();

            // initialize the camera position based on the star ships
            if (player1Camera != null)
                player1Camera.UpperLeft.Y = StarShip1.UpperLeft.Y - VIEWPORT_HEIGHT * 4 / 5;
            if (player2Camera != null)
                player2Camera.UpperLeft.Y = StarShip2.UpperLeft.Y - VIEWPORT_HEIGHT * 4 / 5;

        }

        // This method is provided fully complete as part of the activity starter.
        private void initializeStarShips()
        {
            // set up player one's spaceship
            StarShip1 = new Sprite();
            StarShip1.SetTexture(starship1Texture);
            StarShip1.UpperLeft = new Vector2(VIEWPORT_WIDTH / 2 - StarShip1.GetWidth(), WORLD_HEIGHT - 150);
            StarShip1.SetVelocity(0, 0);
            StarShip1.MaxSpeed = 10;

            // set up player two's spaceship
            StarShip2 = new Sprite();
            StarShip2.SetTexture(starship2Texture);
            StarShip2.UpperLeft = new Vector2(VIEWPORT_WIDTH / 2 + StarShip1.GetWidth(), WORLD_HEIGHT - 150);
            StarShip2.SetVelocity(0, 0);
            StarShip2.MaxSpeed = 10;
        }

        // This method is provided fully complete as part of the activity starter.
        private void initializeStarField()
        {
            //create linked list of star background textures we can pick randomly from
            LinkedList<Texture2D> starTextures = new LinkedList<Texture2D>();
            starTextures.AddLast(starsBackground1Texture);
            starTextures.AddLast(starsBackground2Texture);
            starTextures.AddLast(starsBackground3Texture);
            starTextures.AddLast(starsBackground4Texture);
                        
            //generate random tiles for background
            StarFields.Clear();

            // create starfields for background images
            for (int i = 0; i < NUM_STARFIELDS; i++)
            {
               Sprite starField = new Sprite();

                // position starfield below the previous one
               starField.UpperLeft = new Vector2(0,i * VIEWPORT_HEIGHT);

               // choose random starfield tile image
               int imageNum = randomNumGen.Next(0, starTextures.Count);
               Texture2D texture = starTextures.ElementAt(imageNum);
               starField.SetTexture(texture);
               
               // add to the StarField
               StarFields.AddLast(starField);
            }

            // Now add finish line tile
            finishLine.SetTexture(finishLineTexture);
            finishLine.UpperLeft = new Vector2(0, 30);  // position below the overlay!

        }

        // This method is provided fully complete as part of the activity starter.
        void initializeAsteroids()
        {
            //create linked list of star background textures we can pick randomly from
            LinkedList<Texture2D> asteroidTextures = new LinkedList<Texture2D>();
            asteroidTextures.AddLast(asteroidBig1Texture);
            asteroidTextures.AddLast(asteroidBig2Texture);
            asteroidTextures.AddLast(asteroidMed1Texture);
            asteroidTextures.AddLast(asteroidMed2Texture);
            //asteroidTextures.AddLast(asteroidSmall1Texture);
            //asteroidTextures.AddLast(asteroidSmall2Texture);

            // generate randomly sized asteroids with random location on the background
            Asteroids.Clear();
            for (int i = 0; i < NUM_ASTERIODS; i++)
            {
                Sprite asteroid = new Sprite();
                asteroid.UpperLeft = new Vector2(randomNumGen.Next(0,WORLD_WIDTH), randomNumGen.Next (0, WORLD_HEIGHT));

                // choose random asteroid image
                int imageNum = randomNumGen.Next(0, asteroidTextures.Count);
                Texture2D texture = asteroidTextures.ElementAt(imageNum);
                asteroid.SetTexture(texture);
                
                // give asteroid random direction and speed
                int vX = randomNumGen.Next(-3, 4);  //set random x velocity
                int vY = randomNumGen.Next(-2, 3);  //set (slower) random y velocity
                asteroid.SetVelocity(vX,vY);

                // add to Asteroids list
                Asteroids.AddLast(asteroid);
            }
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        // This method is provided fully complete as part of the activity starter.
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // laod the font for menus
            gameFont = Content.Load<SpriteFont>("Miramo");

            // load all textures used in the game
            asteroidBig1Texture = Content.Load<Texture2D>("Images\\asteroid_big1");
            asteroidBig2Texture = Content.Load<Texture2D>("Images\\asteroid_big2");
            asteroidMed1Texture = Content.Load<Texture2D>("Images\\asteroid_med1");
            asteroidMed2Texture = Content.Load<Texture2D>("Images\\asteroid_med2");
            asteroidSmall1Texture = Content.Load<Texture2D>("Images\\asteroid_small1");
            asteroidSmall2Texture = Content.Load<Texture2D>("Images\\asteroid_small2");
            finishLineTexture = Content.Load<Texture2D>("Images\\finishline");
            gameOverlayTexture = Content.Load<Texture2D>("Images\\game_overlay");
            starsBackground1Texture = Content.Load<Texture2D>("Images\\stars_background1");
            starsBackground2Texture = Content.Load<Texture2D>("Images\\stars_background2");
            starsBackground3Texture = Content.Load<Texture2D>("Images\\stars_background3");
            starsBackground4Texture = Content.Load<Texture2D>("Images\\stars_background4");
            starship1Texture = Content.Load<Texture2D>("Images\\starship");
            starship2Texture = Content.Load<Texture2D>("Images\\starship2");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        // This method is provided fully complete as part of the activity starter.
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        // This method is provided fully complete as part of the activity starter.
        protected override void Update(GameTime gameTime)
        {

            // get the current keyboard state and make sure we have a previous one
            KeyboardState currentKeyboard = Keyboard.GetState();
            if (oldKeyboardState == null)
                oldKeyboardState = currentKeyboard;

            // get the current gamepad states and make sure we have previous ones
            GamePadState currentGamePad1 = GamePad.GetState(PlayerIndex.One);
            GamePadState currentGamePad2 = GamePad.GetState(PlayerIndex.Two);

            if (oldGamePadState1 == null)
                oldGamePadState1 = currentGamePad1;

            if (oldGamePadState2 == null)
                oldGamePadState2 = currentGamePad2;


            // call the appropriate update method based on the current screen
            switch (currentScreen)
            {
                case GameScreen.TITLE:
                    updateTitle(gameTime, currentKeyboard, currentGamePad1 , currentGamePad2);
                    break;
                case GameScreen.OPTIONS:
                    updateOptions(gameTime, currentKeyboard, currentGamePad1 , currentGamePad2);
                    break;
                case GameScreen.PLAYING:
                    updatePlaying(gameTime, currentKeyboard, currentGamePad1 , currentGamePad2);
                    break;
                case GameScreen.PAUSED:
                    updatePaused(gameTime, currentKeyboard, currentGamePad1, currentGamePad2);
                    break;
                case GameScreen.GAMEOVER:
                    updateGameOver(gameTime, currentKeyboard, currentGamePad1, currentGamePad2);
                    break;
            }

            // save our current keyboard state
            oldKeyboardState = currentKeyboard;

            // save the current gamepad states
            oldGamePadState1 = currentGamePad1;
            oldGamePadState2 = currentGamePad2;

            base.Update(gameTime);
        }

        // This method is provided fully complete as part of the activity starter.
        private bool wasKeyPressed(Keys key, KeyboardState currentKeyboard)
        {
            if ((currentKeyboard.IsKeyUp(key) && oldKeyboardState.IsKeyDown(key)))
                return true;
            else
                return false;
        }

        // This method is provided fully complete as part of the activity starter.
        private bool wasButtonAPressed(GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            
            // if the A button was pressed on either the player1 gamepad or
            // player 2 gamepad, return "true"
            if ((currentGamePad1.Buttons.A == ButtonState.Released) &&
                (oldGamePadState1.Buttons.A == ButtonState.Pressed))
                return true;
            else if ((currentGamePad2.Buttons.A == ButtonState.Released) &&
                (oldGamePadState2.Buttons.A == ButtonState.Pressed))
                return true;
            else
                return false;

        }

        // This method is provided fully complete as part of the activity starter.
        private bool wasButtonBPressed(GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
           
            // if the B button was pressed on either the player1 gamepad or
            // player 2 gamepad, return "true"
            if ((currentGamePad1.Buttons.B == ButtonState.Released) &&
                (oldGamePadState1.Buttons.B == ButtonState.Pressed))
                return true;
            else if ((currentGamePad1.Buttons.B == ButtonState.Released) &&
                (oldGamePadState1.Buttons.B == ButtonState.Pressed))
                return true;
            else
                return false;

        }

        // This method is provided fully complete as part of the activity starter.
        private bool wasButtonStartPressed(GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            // if the Start button was pressed on either the player1 gamepad or
            // player 2 gamepad, return "true"
            if ((currentGamePad1.Buttons.Start == ButtonState.Released) &&
                (oldGamePadState1.Buttons.Start == ButtonState.Pressed))
                return true;
            else if ((currentGamePad2.Buttons.Start == ButtonState.Released) &&
                (oldGamePadState2.Buttons.Start == ButtonState.Pressed))
                return true;
            else
                return false;
        }

        // This method is provided fully complete as part of the activity starter.
        private bool wasButtonBackPressed(GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            
            // if the Back button was pressed on either the player1 gamepad or
            // player 2 gamepad, return "true"
            if ((currentGamePad1.Buttons.Back == ButtonState.Released) &&
                (oldGamePadState1.Buttons.Back == ButtonState.Pressed))
                return true;
            else if ((currentGamePad2.Buttons.Back == ButtonState.Released) &&
                (oldGamePadState2.Buttons.Back == ButtonState.Pressed))
                return true;
            else
                return false;
        }

        // This method is provided fully complete as part of the activity starter.
        private void updateTitle(GameTime gameTime, KeyboardState currentKeyboard, GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            // if the space bar (or the gamepad Start button) was pressed from the title screen
            if (wasKeyPressed(Keys.Space,currentKeyboard) || wasButtonStartPressed (currentGamePad1 , currentGamePad2 ))
            {
                // move on to the options screen
                currentScreen = GameScreen.OPTIONS;
            }
        }

        // This method is provided fully complete as part of the activity starter.
        private void updateOptions(GameTime gameTime, KeyboardState currentKeyboard, GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            // if the '1' key (or the gamepad A button) was pressed
            if (wasKeyPressed(Keys.D1, currentKeyboard) || wasButtonAPressed (currentGamePad1 , currentGamePad2 ))
            {
                // start a one-player game
                isGameAIEnabled = true;
                startGame();
                currentScreen = GameScreen.PAUSED;
            }

            // if the '2' key (or the gamepad B button)was pressed
            if (wasKeyPressed(Keys.D2, currentKeyboard) || wasButtonBPressed (currentGamePad1 , currentGamePad2 ))
            {
                // start a two-player game
                isGameAIEnabled = false;
                startGame();
                currentScreen = GameScreen.PAUSED;
            }

            // if the 'Escape' key (or the gamepad Back button) was pressed
            if (wasKeyPressed(Keys.Escape, currentKeyboard) || wasButtonBackPressed(currentGamePad1 , currentGamePad2 ))
            {
                Exit();
            }
        }
        
        // This method is provided fully complete as part of the activity starter.
        private void updatePlaying(GameTime gameTime, KeyboardState currentKeyboard, GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            // Escape key (or gamepad Start button)pauses the game when playing
            if (wasKeyPressed(Keys.Escape, currentKeyboard) || wasButtonStartPressed(currentGamePad1 , currentGamePad2 ))
            {
                currentScreen = GameScreen.PAUSED;
                return; // don't do anything else if we're now paused!
            }

            // check player 1 keys
            if (currentKeyboard.IsKeyDown(Keys.W) || (currentGamePad1.ThumbSticks.Left.Y > 0))
            {
                // 'W' (or gamepad thumbstick up) means accelerate up
                StarShip1.Accelerate(0, -ACCELERATION_FACTOR);
            }

            if (currentKeyboard.IsKeyDown(Keys.A) || (currentGamePad1.ThumbSticks.Left.X < 0))
            {
                // 'A' (or gamepad thumbstick left) means move a bit to the left
                StarShip1.UpperLeft.X -= X_STEER_FACTOR;
                if (StarShip1.UpperLeft.X < 0)
                    StarShip1.UpperLeft.X = 0;
            }

            if (currentKeyboard.IsKeyDown(Keys.D) || (currentGamePad1.ThumbSticks.Left.X > 0))
            {
                // 'D' (or gamepad thumbstick right) means move a bit to the right
                StarShip1.UpperLeft.X += X_STEER_FACTOR;
                if (StarShip1.UpperLeft.X + StarShip1.GetWidth() > VIEWPORT_WIDTH)
                    StarShip1.UpperLeft.X = VIEWPORT_WIDTH - StarShip1.GetWidth();
           }

            if (currentKeyboard.IsKeyDown(Keys.X) || (currentGamePad1.ThumbSticks.Left.Y < 0))
            {
                // 'X' (or gamepad thumbstick down) means slow down
                if (StarShip1.GetVelocity().Y < 0)
                {
                    StarShip1.Accelerate(0, ACCELERATION_FACTOR);
                }
            }

            // now check player2 input unless it's an AI-controlled player
            if (!isGameAIEnabled)
            {
                // check player 2 controls
                if (currentKeyboard.IsKeyDown(Keys.Up) || (currentGamePad2.ThumbSticks.Left.Y > 0))
                {
                    // up arrow means accelerate up
                    StarShip2.Accelerate(0, -ACCELERATION_FACTOR);
                }

                if (currentKeyboard.IsKeyDown(Keys.Down) || (currentGamePad2.ThumbSticks.Left.Y < 0))
                {
                    // down arrow means slow down
                    if (StarShip2.GetVelocity().Y < 0)
                    {
                        StarShip2.Accelerate(0, ACCELERATION_FACTOR);
                    }
                }

                if (currentKeyboard.IsKeyDown(Keys.Left) || (currentGamePad2.ThumbSticks.Left.X < 0))
                {
                    // left arrow means move a bit to the left
                    StarShip2.UpperLeft.X -= X_STEER_FACTOR;
                    if (StarShip2.UpperLeft.X < 0)
                        StarShip2.UpperLeft.X = 0;
                }

                if (currentKeyboard.IsKeyDown(Keys.Right) || (currentGamePad2.ThumbSticks.Left.X > 0))
                {
                    // right arrow means move a bit to the right
                    StarShip2.UpperLeft.X += X_STEER_FACTOR;
                    if (StarShip2.UpperLeft.X + StarShip2.GetWidth() > VIEWPORT_WIDTH)
                        StarShip2.UpperLeft.X = VIEWPORT_WIDTH - StarShip2.GetWidth();
                }
            }
            else
            {
                // call the AI routine to control the second space ship
                DoSimpleAI();
            }
            
            // move both ships in the Y direction 
            // (X velocity we leave at zero so we can move strictly according to left/right keys above)
            StarShip1.Move();
            StarShip2.Move();

            // move the cameras along with the space ship, keeping ship 4/5 of the way down the screen
            if (player1Camera != null)
                player1Camera.UpperLeft.Y = StarShip1.UpperLeft.Y - VIEWPORT_HEIGHT * 4 / 5;
            if (player2Camera != null)
                player2Camera.UpperLeft.Y = StarShip2.UpperLeft.Y - VIEWPORT_HEIGHT * 4 / 5;
            
            // make sure camera remains within valid world coordinates near the finish line!
            if (player1Camera != null)
                player1Camera.LockCamera();
            if (player2Camera != null)
                player2Camera.LockCamera();

            // move all the asteroids, allowing them to wrap around the screen.
            // not realistic, but to the player it might as well be a new asteroid!
            foreach (Sprite asteroid in Asteroids)
            {
                asteroid.MoveAndWrap(WORLD_WIDTH, WORLD_HEIGHT);
            }

            // see if any asteroids have hit a ship
            checkCollisions();

            // check to see if anyone has won
            if (StarShip1.UpperLeft.Y <= (finishLine.UpperLeft.Y + finishLine.GetHeight())) 
            {
                currentScreen = GameScreen.GAMEOVER;
                gameOverMessage = "Player 1 has won!";
            }
            if (StarShip2.UpperLeft.Y <= finishLine.GetHeight())
            {
                currentScreen = GameScreen.GAMEOVER;
                gameOverMessage = "Player 2 has won!";
            }
        }
        

        // This method is provided fully complete as part of the activity starter.
        private void updatePaused(GameTime gameTime, KeyboardState currentKeyboard, GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            // the 'C' key (or the gamepad Start button) will continue the game from Paused state
            if (wasKeyPressed(Keys.C, currentKeyboard) || wasButtonStartPressed (currentGamePad1, currentGamePad2 ))
                currentScreen = GameScreen.PLAYING;

        }

        // This method is provided fully complete as part of the activity starter.
        private void updateGameOver(GameTime gameTime, KeyboardState currentKeyboard, GamePadState currentGamePad1, GamePadState currentGamePad2)
        {
            // the space bar (or the Back button on a gamepad) will move to the options screen after the game is over
            if (wasKeyPressed(Keys.Space,currentKeyboard) || wasButtonBackPressed (currentGamePad1 , currentGamePad2 ))
                currentScreen = GameScreen.OPTIONS;
        }

        // This method is provided fully complete as part of the activity starter.
        private void checkCollisions()
        {
            // check each asteroid to see if it has hit either (or both!) ships
            foreach (Sprite asteroid in Asteroids)
            {
                if (asteroid.IsCollided(StarShip1 ))
                {
                    StarShip1.SetVelocity(0, 0);    // set ship 1's velocity to 0
                }

                if (asteroid.IsCollided(StarShip2))
                {
                    StarShip2.SetVelocity(0, 0);    // set ship 2's velocity to 0
                }

            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        // This method is provided fully complete as part of the activity starter.
        protected override void Draw(GameTime gameTime)
        {
            // save viewport that covers the entire screen for later use
            Viewport original = graphics.GraphicsDevice.Viewport;

            if (currentScreen == GameScreen.TITLE )
            {
                drawTitle();
            }
            else if (currentScreen == GameScreen.OPTIONS )
            {
                drawOptions();
            }
            else
            {
                // draw the playing surface for all other screens
                // (makes a nice background during pause and gameover!)
                drawPlaying(original);
            }

            // ensure viewport covers the whole screen for the next two screens
            GraphicsDevice.Viewport = original;

            if (currentScreen == GameScreen.PAUSED )
                drawPausedMenu();

            if (currentScreen == GameScreen.GAMEOVER)
                drawGameOver();

            base.Draw(gameTime);
        }

        // This method is provided fully complete as part of the activity starter.
        private void drawPlaying(Viewport fullScreen)
        {
            // wipe out whatever was there before
            GraphicsDevice.Clear(Color.Black);

            // draw left and right viewport
            drawPlayerViewport(leftViewport, player1Camera);
            drawPlayerViewport(rightViewport, player2Camera);

            // draw the overlay 
            GraphicsDevice.Viewport = fullScreen;
            spriteBatch.Begin();
            overlay.Draw(spriteBatch);
            spriteBatch.End();

        }

        // The student will complete this method as part of an activity
        private void drawPlayerViewport(Viewport currentViewport, Camera currentCamera)
        {

            graphics.GraphicsDevice.Viewport = currentViewport;

            spriteBatch.Begin();

            DrawStarsAndAsteroids(spriteBatch,currentCamera);

            if (currentCamera.IsVisible(StarShip1.UpperLeft, StarShip1.GetWidth(), StarShip1.GetHeight()))
                StarShip1.Draw(spriteBatch, currentCamera.UpperLeft);


            if (currentCamera.IsVisible(StarShip2.UpperLeft, StarShip2.GetWidth(), StarShip2.GetHeight()))
                StarShip2.Draw(spriteBatch, currentCamera.UpperLeft);

            spriteBatch.End();

        }

        // This method is provided fully complete as part of the activity starter.
        private void DrawStarsAndAsteroids(SpriteBatch spriteBatch, Camera currentCamera)
        {
            // Draw all starfields visible on the screen
            foreach (Sprite starField in StarFields)
            {
                if (currentCamera.IsVisible(starField.UpperLeft, starField.GetWidth(), starField.GetHeight()))
                    starField.Draw(spriteBatch, currentCamera.UpperLeft);
            }

            // Now add finish line tile if visible on the screen
            if (currentCamera.IsVisible(finishLine.UpperLeft, finishLine.GetWidth(), finishLine.GetHeight()))
                finishLine.Draw(spriteBatch, currentCamera.UpperLeft);

            // Draw all asteroids visible on the screen
            foreach (Sprite asteroid in Asteroids)
            {
                if (currentCamera.IsVisible(asteroid.UpperLeft, asteroid.GetWidth(), asteroid.GetHeight()))
                    asteroid.Draw(spriteBatch, currentCamera.UpperLeft);
            }
        }

        // This method is provided fully complete as part of the activity starter.
        private void drawTitle()
        {
            GraphicsDevice.Clear(Color.DarkBlue);
            spriteBatch.Begin();
            spriteBatch.DrawString(gameFont, "Star Racer - Press Spacebar (or Start) to Continue", new Vector2(50, 250), Color.White); 
            spriteBatch.End();
        }

        // This method is provided fully complete as part of the activity starter.
        private void drawOptions()
        {
            GraphicsDevice.Clear(Color.DarkBlue);
            spriteBatch.Begin();
            spriteBatch.DrawString(gameFont, "Press 1 (or A) for 1 Player", new Vector2(150, 200), Color.White);
            spriteBatch.DrawString(gameFont, "Press 2 (or B) for 2 Player", new Vector2(150, 250), Color.White);
            spriteBatch.DrawString(gameFont, "Press Esc (or Back) to exit", new Vector2(150, 300), Color.White);
            spriteBatch.End();
        }

        // This method is provided fully complete as part of the activity starter.
        private void drawPausedMenu()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(gameFont, "Game Paused - Press C (or Start) to Continue", new Vector2(50, 250), Color.White);
            spriteBatch.End();
        }

        // This method is provided fully complete as part of the activity starter.
        private void drawGameOver()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(gameFont, gameOverMessage, new Vector2(200, 250), Color.White);
            spriteBatch.DrawString(gameFont, "Press Spacebar (or Back) to Continue", new Vector2(200, 290), Color.White);
            spriteBatch.End();
        }

        // This method will be completed by the student in the artifical intelligence chapter
        private void DoSimpleAI()
        {

            if (StarShip2.GetVelocity().Y <= StarShip2.MaxSpeed)
            {
                StarShip2.Accelerate(0, -ACCELERATION_FACTOR);
            }
            // run a better algorithm too, if present
            DoBetterAI();
        }

        // This method will be completed by the student in the artifical intelligence chapter
        private void DoBetterAI()
        {
            Sprite ClosestAsteroid = null;
            Double DistanceFromAsteroid = 8008132;

            foreach (Sprite Thing in Asteroids)
            {
                if (player2Camera.IsVisible(Thing.UpperLeft ,Thing.GetWidth(), Thing.GetHeight()))
                {
                    if (Thing.UpperLeft.Y == StarShip2.UpperLeft.Y - StarShip2.GetHeight())
                        return;

                    double Temp = Math.Sqrt(Thing.UpperLeft.X - StarShip2.UpperLeft.X * Thing.UpperLeft.X - StarShip2.UpperLeft.X + Thing.UpperLeft.Y - StarShip2.UpperLeft.Y * Thing.UpperLeft.X - StarShip2.UpperLeft.X);

                    if (Temp <= DistanceFromAsteroid)
                    {
                        ClosestAsteroid = Thing;
                        DistanceFromAsteroid = Temp;
                    }
                }
            }

            if (ClosestAsteroid == null)
            {
                if (StarShip2.UpperLeft.X + StarShip2.GetWidth() / 2 > VIEWPORT_WIDTH / 2)
                {
                    StarShip2.UpperLeft.X -= X_STEER_FACTOR;
                }
                if (StarShip2.UpperLeft.X + StarShip2.GetWidth() / 2 < VIEWPORT_WIDTH / 2)
                {
                    StarShip2.UpperLeft.X += X_STEER_FACTOR;
                }
            }
            else
            {
                if (ClosestAsteroid.UpperLeft.X > StarShip2.UpperLeft.X + StarShip2.GetWidth() / 2)
                {
                    if (StarShip2.UpperLeft.X + StarShip2.GetWidth() / 2 != 0)
                    {
                        StarShip2.UpperLeft.X += X_STEER_FACTOR;
                    }
                }
                if (ClosestAsteroid.UpperLeft.X < StarShip2.UpperLeft.X + StarShip2.GetWidth() / 2)
                {
                    if (StarShip2.UpperLeft.X + StarShip2.GetWidth() / 2 != 398)
                    {
                        StarShip2.UpperLeft.X -= X_STEER_FACTOR;
                    }
                }
            }
        }


    }
}
