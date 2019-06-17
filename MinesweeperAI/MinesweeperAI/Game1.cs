using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MinesweeperAI
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Textures
        Texture2D Tile;
        Texture2D TileClicked;
        Texture2D ResetButton;
        Texture2D ResetButtonDead;
        Texture2D BombTile;
        Texture2D FlagTile;
        Texture2D Tile1;
        Texture2D Tile2;
        Texture2D Tile3;
        Texture2D Tile4;
        Texture2D Tile5;
        Texture2D Tile6;
        Texture2D Tile7;
        Texture2D Tile8;
        Texture2D PlusButton;
        Texture2D MinusButton;

        //Font
        SpriteFont Tahoma;

        //Booleans
        bool isNotDead = true;
        bool mouseLeftReleased = true;
        bool mouseRightReleased = true;
        bool tabReleased = true;
        bool hasWon = false;

        //Game start constants
        int GameTileColoumns = 30;
        int GameTileRows = 16;
        int GameBombCount = 99;

        //Tile dimensions
        const int TileWidth = 24;
        const int TileHeight = 24;

        //Reset button dimensions;
        const int ResetButtonWidth = 32;
        const int ResetButtonHeight = 32;

        //Tile map offsets
        const int MapLeftOffset = 16;
        const int MapRightOffset = 16;
        const int MapBottomOffset = 16;
        const int MapTopOffset = 64;

        //Reset button offsets
        int ResetButtonLeftOffset;
        int ResetButtonTopOffset = 8;

        //Maps
        int[,] BombMap;
        int[,] DisplayMap;

        //Game bomb counter
        int GameBombCounter;

        //Game safe tiles remaining
        int SafeTilesRemaining;

        //Time elapsed from the start
        double elapsedTime = 0f;

        //AI vars
        bool AITurnedOn = false;
        bool AIPaused = true;
        int Turn = 0;
        int TileChosenX;
        int TileChosenY;
        List<Vector2> LeftClickTileBuffer;
        List<Vector2> RightClickTileBuffer;
        List<Vector2> LuckyRightClickTileBuffer;
        List<Vector2> LuckyLeftClickTileBuffer;
        double waitTime = 0.000001;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            UpdateGameSettings();
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        public void UpdateGameSettings()
        {
            graphics.PreferredBackBufferWidth = MapLeftOffset + GameTileColoumns * TileWidth + MapRightOffset;
            graphics.PreferredBackBufferHeight = MapTopOffset + GameTileRows * TileHeight + MapBottomOffset;
            ResetButtonLeftOffset = ((MapLeftOffset + GameTileColoumns * TileWidth + MapRightOffset) / 2) - (ResetButtonWidth / 2);
            BombMap = new int[GameTileRows, GameTileColoumns];
            DisplayMap = new int[GameTileRows, GameTileColoumns];
            GenerateMap();
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {

            spriteBatch = new SpriteBatch(GraphicsDevice);

            Tile = Content.Load<Texture2D>("Tile");
            TileClicked = Content.Load<Texture2D>("TileClicked");
            ResetButton = Content.Load<Texture2D>("ResetButton");
            ResetButtonDead = Content.Load<Texture2D>("ResetButtonDead");
            BombTile = Content.Load<Texture2D>("TileBomb");
            FlagTile = Content.Load<Texture2D>("TileFlag");
            Tile1 = Content.Load<Texture2D>("Tile1");
            Tile2 = Content.Load<Texture2D>("Tile2");
            Tile3 = Content.Load<Texture2D>("Tile3");
            Tile4 = Content.Load<Texture2D>("Tile4");
            Tile5 = Content.Load<Texture2D>("Tile5");
            Tile6 = Content.Load<Texture2D>("Tile6");
            Tile7 = Content.Load<Texture2D>("Tile7");
            Tile8 = Content.Load<Texture2D>("Tile8");
            PlusButton = Content.Load<Texture2D>("PlusButton");
            MinusButton = Content.Load<Texture2D>("MinusButton");

            Tahoma = Content.Load<SpriteFont>("Tahoma");
        }

        protected override void UnloadContent()
        {

        }


        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState mouseState = Mouse.GetState();

            //Handles left clicks
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (mouseLeftReleased)
                {
                    //On bomb tile click
                    if (isNotDead)
                        if (mouseState.Position.X > MapLeftOffset && mouseState.Position.X < (MapLeftOffset + (GameTileColoumns * TileWidth)) && mouseState.Position.Y > MapTopOffset && mouseState.Position.Y < (MapTopOffset + (GameTileRows * TileHeight)))
                        {
                            int TileClickedX = (mouseState.Position.X - MapLeftOffset) / TileWidth;
                            int TileClickedY = (mouseState.Position.Y - MapTopOffset) / TileHeight;
                            if (DisplayMap[TileClickedY, TileClickedX] != 12)
                            {
                                switch (BombMap[TileClickedY, TileClickedX])
                                {
                                    case 0:
                                        DisplayMap[TileClickedY, TileClickedX] = 10;
                                        SafeTilesRemaining--;
                                        CheckForEmptyTiles(TileClickedX, TileClickedY);
                                        break;
                                    case 9:
                                        Die();
                                        break;
                                    case 12:
                                        break;
                                    default:
                                        DisplayMap[TileClickedY, TileClickedX] = BombMap[TileClickedY, TileClickedX];
                                        SafeTilesRemaining--;
                                        break;
                                }
                            }
                            Console.WriteLine(GetNumberOfUnclikedTilesAround(TileClickedX, TileClickedY));
                        }

                    //On reset button click
                    if (mouseState.Position.X > ResetButtonLeftOffset && mouseState.Position.X < (ResetButtonLeftOffset + ResetButtonWidth) && mouseState.Position.Y > ResetButtonTopOffset && mouseState.Position.Y < (ResetButtonTopOffset + ResetButtonHeight))
                    {
                        AITurnedOn = false;
                        GenerateMap();
                    }

                    //Row buttons
                    if (mouseState.Position.X > (ResetButtonLeftOffset + ResetButtonWidth + 8) && mouseState.Position.X < ((ResetButtonLeftOffset + ResetButtonWidth + 8) + ResetButtonWidth) && mouseState.Position.Y > ResetButtonTopOffset && mouseState.Position.Y < (ResetButtonTopOffset + ResetButtonHeight))
                    {
                        if (GameTileRows > 4)
                        {
                            GameTileRows--;
                            UpdateGameSettings();
                        }
                    }
                    if (mouseState.Position.X > (ResetButtonLeftOffset + (ResetButtonWidth + 8) * 2) && mouseState.Position.X < ((ResetButtonLeftOffset + (ResetButtonWidth + 8) * 2) + ResetButtonWidth) && mouseState.Position.Y > ResetButtonTopOffset && mouseState.Position.Y < (ResetButtonTopOffset + ResetButtonHeight))
                    {
                        GameTileRows++;
                        UpdateGameSettings();
                    }

                    //Coloumn buttons
                    if (mouseState.Position.X > (ResetButtonLeftOffset + (ResetButtonWidth + 8) * 3) && mouseState.Position.X < ((ResetButtonLeftOffset + (ResetButtonWidth + 8) * 3) + ResetButtonWidth) && mouseState.Position.Y > ResetButtonTopOffset && mouseState.Position.Y < (ResetButtonTopOffset + ResetButtonHeight))
                    {
                        if (GameTileColoumns > 4)
                        {
                            GameTileColoumns--;
                            UpdateGameSettings();
                        }
                    }
                    if (mouseState.Position.X > (ResetButtonLeftOffset + (ResetButtonWidth + 8) * 4) && mouseState.Position.X < ((ResetButtonLeftOffset + (ResetButtonWidth + 8) * 4) + ResetButtonWidth) && mouseState.Position.Y > ResetButtonTopOffset && mouseState.Position.Y < (ResetButtonTopOffset + ResetButtonHeight))
                    {
                        GameTileColoumns++;
                        UpdateGameSettings();
                    }

                    //Bomb buttons
                    if (mouseState.Position.X > (ResetButtonLeftOffset + (ResetButtonWidth + 8) * 5) && mouseState.Position.X < ((ResetButtonLeftOffset + (ResetButtonWidth + 8) * 5) + ResetButtonWidth) && mouseState.Position.Y > ResetButtonTopOffset && mouseState.Position.Y < (ResetButtonTopOffset + ResetButtonHeight))
                    {
                        if (GameBombCount > 10)
                        {
                            GameBombCount--;
                            UpdateGameSettings();
                        }
                    }
                    if (mouseState.Position.X > (ResetButtonLeftOffset + (ResetButtonWidth + 8) * 6) && mouseState.Position.X < ((ResetButtonLeftOffset + (ResetButtonWidth + 8) * 6) + ResetButtonWidth) && mouseState.Position.Y > ResetButtonTopOffset && mouseState.Position.Y < (ResetButtonTopOffset + ResetButtonHeight))
                    {
                        if (GameBombCount < GameTileRows * GameTileColoumns)
                        {
                            GameBombCount++;
                            UpdateGameSettings();
                        }
                    }

                    //Turn AI on
                    if (mouseState.Position.X > (ResetButtonLeftOffset + (ResetButtonWidth + 8) * 7) && mouseState.Position.X < ((ResetButtonLeftOffset + (ResetButtonWidth + 8) * 7) + ResetButtonWidth) && mouseState.Position.Y > ResetButtonTopOffset && mouseState.Position.Y < (ResetButtonTopOffset + ResetButtonHeight))
                    {
                        if (isNotDead)
                            RunAI();
                    }

                    mouseLeftReleased = false;
                }
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                if (mouseRightReleased)
                {
                    if (isNotDead)
                        if (mouseState.Position.X > MapLeftOffset && mouseState.Position.X < (MapLeftOffset + (GameTileColoumns * TileWidth)) && mouseState.Position.Y > MapTopOffset && mouseState.Position.Y < (MapTopOffset + (GameTileRows * TileHeight)))
                        {
                            int TileClickedX = (mouseState.Position.X - MapLeftOffset) / TileWidth;
                            int TileClickedY = (mouseState.Position.Y - MapTopOffset) / TileHeight;
                            if (DisplayMap[TileClickedY, TileClickedX] == 0)
                            {
                                DisplayMap[TileClickedY, TileClickedX] = 12;
                                GameBombCounter--;
                            }
                            else if (DisplayMap[TileClickedY, TileClickedX] == 12)
                            {
                                DisplayMap[TileClickedY, TileClickedX] = 0;
                                GameBombCounter++;
                            }
                        }
                    mouseRightReleased = false;
                }
            }

            //Makes so that one click = one tile click
            if (mouseState.LeftButton == ButtonState.Released)
                mouseLeftReleased = true;
            if (mouseState.RightButton == ButtonState.Released)
                mouseRightReleased = true;

            //Tab starts the AI, pauses him and unpauses him
            if (Keyboard.GetState().IsKeyDown(Keys.Tab))
            {
                if (tabReleased)
                {
                    if (!AIPaused)
                        AIPaused = true;
                    else if (Turn > 0)
                        AIPaused = false;
                    else
                        RunAI();
                    tabReleased = false;
                }
            }

            if (Keyboard.GetState().IsKeyUp(Keys.Tab))
                tabReleased = true;

            //R restars game
            if (Keyboard.GetState().IsKeyDown(Keys.R))
                GenerateMap();

            //Win condition
            if (SafeTilesRemaining == 0 || GameBombCounter == 0)
            {
                Win();
            }

            //Update AI if he is on
            if (AITurnedOn)
                PulseAI(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            spriteBatch.Begin();

            //Draw reset tile
            if (isNotDead)
                spriteBatch.Draw(ResetButton, new Rectangle(ResetButtonLeftOffset, ResetButtonTopOffset, ResetButtonWidth, ResetButtonHeight), Color.White);
            else
                spriteBatch.Draw(ResetButtonDead, new Rectangle(ResetButtonLeftOffset, ResetButtonTopOffset, ResetButtonWidth, ResetButtonHeight), Color.White);

            //If won
            if (hasWon)
                spriteBatch.DrawString(Tahoma, "You won!", new Vector2(160, ResetButtonTopOffset + ResetButtonHeight + 4), Color.Red);

            //Draw bomb map
            for (int x = 0; x < GameTileColoumns; x++)
            {
                for (int y = 0; y < GameTileRows; y++)
                {
                    switch (DisplayMap[y, x])
                    {
                        //Show unclicked tiles
                        case 0:
                            spriteBatch.Draw(Tile, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        #region Number tiles
                        case 1:
                            spriteBatch.Draw(Tile1, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        case 2:
                            spriteBatch.Draw(Tile2, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        case 3:
                            spriteBatch.Draw(Tile3, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        case 4:
                            spriteBatch.Draw(Tile4, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        case 5:
                            spriteBatch.Draw(Tile5, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        case 6:
                            spriteBatch.Draw(Tile6, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        case 7:
                            spriteBatch.Draw(Tile7, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        case 8:
                            spriteBatch.Draw(Tile8, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        #endregion
                        //Show clicked bomb
                        case 9:
                            spriteBatch.Draw(BombTile, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        //Show blank tile
                        case 10:
                            spriteBatch.Draw(TileClicked, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        //Show unclicked bombs
                        case 11:
                            spriteBatch.Draw(BombTile, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                        //Show flag tile
                        case 12:
                            spriteBatch.Draw(FlagTile, new Rectangle(x * TileWidth + MapLeftOffset, y * TileHeight + MapTopOffset, TileWidth, TileHeight), Color.White);
                            break;
                    }
                }
            }

            //Draw bomb counter
            spriteBatch.DrawString(Tahoma, GameBombCounter + " bombs left FPS: " + (1 / gameTime.ElapsedGameTime.TotalSeconds), new Vector2(16, 8), Color.Red);
            spriteBatch.DrawString(Tahoma, "Coloumns: " + GameTileColoumns + " Rows: " + GameTileRows, new Vector2(16, 32), Color.Red);

            //Draw plus and minus buttons
            //Rows
            spriteBatch.Draw(MinusButton, new Rectangle(ResetButtonLeftOffset + ResetButtonWidth + 8, ResetButtonTopOffset, ResetButtonWidth, ResetButtonHeight), Color.White);
            spriteBatch.Draw(PlusButton, new Rectangle(ResetButtonLeftOffset + (ResetButtonWidth + 8) * 2, ResetButtonTopOffset, ResetButtonWidth, ResetButtonHeight), Color.White);
            //Coloumns
            spriteBatch.Draw(MinusButton, new Rectangle(ResetButtonLeftOffset + (ResetButtonWidth + 8) * 3, ResetButtonTopOffset, ResetButtonWidth, ResetButtonHeight), Color.White);
            spriteBatch.Draw(PlusButton, new Rectangle(ResetButtonLeftOffset + (ResetButtonWidth + 8) * 4, ResetButtonTopOffset, ResetButtonWidth, ResetButtonHeight), Color.White);
            //Bombs
            spriteBatch.Draw(MinusButton, new Rectangle(ResetButtonLeftOffset + (ResetButtonWidth + 8) * 5, ResetButtonTopOffset, ResetButtonWidth, ResetButtonHeight), Color.White);
            spriteBatch.Draw(PlusButton, new Rectangle(ResetButtonLeftOffset + (ResetButtonWidth + 8) * 6, ResetButtonTopOffset, ResetButtonWidth, ResetButtonHeight), Color.White);

            spriteBatch.DrawString(Tahoma, "Turn: " + Turn, new Vector2(ResetButtonLeftOffset + (ResetButtonWidth + 8) * 7, ResetButtonTopOffset), Color.White);

            //AI button
            spriteBatch.Draw(ResetButton, new Rectangle(ResetButtonLeftOffset + (ResetButtonWidth + 8) * 7, ResetButtonTopOffset, ResetButtonWidth, ResetButtonHeight), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public void GenerateMap()
        {
            Random rnd = new Random();

            //Clears bomb and display map
            Array.Clear(DisplayMap, 0, DisplayMap.Length);
            Array.Clear(BombMap, 0, DisplayMap.Length);

            int BombCounter = GameBombCount;
            GameBombCounter = GameBombCount;

            //If the counter is more than 0 put a bomb somewhere and decrease counter by 1
            while (BombCounter > 0)
            {
                for (int x = 0; x < GameTileColoumns; x++)
                {
                    for (int y = 0; y < GameTileRows; y++)
                    {
                        if (rnd.Next(0, 10) == 9 && BombCounter > 0 && BombMap[y, x] != 9)
                        {
                            BombMap[y, x] = 9;
                            BombCounter--;
                        }
                    }
                }
            }

            //Set numbers on tiles
            for (int x = 0; x < GameTileColoumns; x++)
            {
                for (int y = 0; y < GameTileRows; y++)
                {
                    if (BombMap[y, x] == 0)
                    {
                        int SurroundingBombCounter = 0;
                        //Left
                        if (x - 1 >= 0 && BombMap[y, x - 1] == 9) SurroundingBombCounter++;
                        //Top Left
                        if (x - 1 >= 0 && y - 1 >= 0 && BombMap[y - 1, x - 1] == 9) SurroundingBombCounter++;
                        //Top
                        if (y - 1 >= 0 && BombMap[y - 1, x] == 9) SurroundingBombCounter++;
                        //Top Right
                        if (x + 1 < GameTileColoumns && y - 1 >= 0 && BombMap[y - 1, x + 1] == 9) SurroundingBombCounter++;
                        //Right
                        if (x + 1 < GameTileColoumns && BombMap[y, x + 1] == 9) SurroundingBombCounter++;
                        //Bottom Right
                        if (x + 1 < GameTileColoumns && y + 1 < GameTileRows && BombMap[y + 1, x + 1] == 9) SurroundingBombCounter++;
                        //Bottom
                        if (y + 1 < GameTileRows && BombMap[y + 1, x] == 9) SurroundingBombCounter++;
                        //Bottom Left
                        if (x - 1 >= 0 && y + 1 < GameTileRows && BombMap[y + 1, x - 1] == 9) SurroundingBombCounter++;

                        BombMap[y, x] = SurroundingBombCounter;
                    }
                }
            }

            SafeTilesRemaining = GameTileRows * GameTileColoumns - GameBombCount;
            isNotDead = true;
            hasWon = false;
            Turn = 0;
            AITurnedOn = false;
        }

        public void CheckForEmptyTiles(int x, int y)
        {
            //Left
            if (x - 1 >= 0 && BombMap[y, x - 1] != 9 && DisplayMap[y, x - 1] == 0)
            {
                if (BombMap[y, x - 1] == 0)
                {
                    DisplayMap[y, x - 1] = 10;
                    SafeTilesRemaining--;
                    CheckForEmptyTiles(x - 1, y);
                }
                else
                {
                    DisplayMap[y, x - 1] = BombMap[y, x - 1];
                    SafeTilesRemaining--;
                }
            }

            //Top left
            if (x - 1 >= 0 && y - 1 >= 0 && BombMap[y - 1, x - 1] != 9 && DisplayMap[y - 1, x - 1] == 0)
            {
                if (BombMap[y - 1, x - 1] == 0)
                {
                    DisplayMap[y - 1, x - 1] = 10;
                    SafeTilesRemaining--;
                    CheckForEmptyTiles(x - 1, y - 1);
                }
                else
                {
                    DisplayMap[y - 1, x - 1] = BombMap[y - 1, x - 1];
                    SafeTilesRemaining--;
                }
            }

            //Top
            if (y - 1 >= 0 && BombMap[y - 1, x] != 9 && DisplayMap[y - 1, x] == 0)
            {
                if (BombMap[y - 1, x] == 0)
                {
                    DisplayMap[y - 1, x] = 10;
                    SafeTilesRemaining--;
                    CheckForEmptyTiles(x, y - 1);
                }
                else
                {
                    DisplayMap[y - 1, x] = BombMap[y - 1, x];
                    SafeTilesRemaining--;
                }
            }

            //Top right
            if (x + 1 < GameTileColoumns && y - 1 >= 0 && BombMap[y - 1, x + 1] != 9 && DisplayMap[y - 1, x + 1] == 0)
            {
                if (BombMap[y - 1, x + 1] == 0)
                {
                    DisplayMap[y - 1, x + 1] = 10;
                    SafeTilesRemaining--;
                    CheckForEmptyTiles(x + 1, y - 1);
                }
                else
                {
                    DisplayMap[y - 1, x + 1] = BombMap[y - 1, x + 1];
                    SafeTilesRemaining--;
                }
            }

            //Right
            if (x + 1 < GameTileColoumns && BombMap[y, x + 1] != 9 && DisplayMap[y, x + 1] == 0)
            {
                if (BombMap[y, x + 1] == 0)
                {
                    DisplayMap[y, x + 1] = 10;
                    SafeTilesRemaining--;
                    CheckForEmptyTiles(x + 1, y);
                }
                else
                {
                    DisplayMap[y, x + 1] = BombMap[y, x + 1];
                    SafeTilesRemaining--;
                }
            }

            //Bottom right
            if (x + 1 < GameTileColoumns && y + 1 < GameTileRows && BombMap[y + 1, x + 1] != 9 && DisplayMap[y + 1, x + 1] == 0)
            {
                if (BombMap[y + 1, x + 1] == 0)
                {
                    DisplayMap[y + 1, x + 1] = 10;
                    SafeTilesRemaining--;
                    CheckForEmptyTiles(x + 1, y + 1);
                }
                else
                {
                    DisplayMap[y + 1, x + 1] = BombMap[y + 1, x + 1];
                    SafeTilesRemaining--;
                }
            }

            //Bottom
            if (y + 1 < GameTileRows && BombMap[y + 1, x] != 9 && DisplayMap[y + 1, x] == 0)
            {
                if (BombMap[y + 1, x] == 0)
                {
                    DisplayMap[y + 1, x] = 10;
                    SafeTilesRemaining--;
                    CheckForEmptyTiles(x, y + 1);
                }
                else
                {
                    DisplayMap[y + 1, x] = BombMap[y + 1, x];
                    SafeTilesRemaining--;
                }
            }

            //Bottom left
            if (x - 1 > 0 && y + 1 < GameTileRows && BombMap[y + 1, x - 1] != 9 && DisplayMap[y + 1, x - 1] == 0)
            {
                if (BombMap[y + 1, x - 1] == 0)
                {
                    DisplayMap[y + 1, x - 1] = 10;
                    SafeTilesRemaining--;
                    CheckForEmptyTiles(x - 1, y + 1);
                }
                else
                {
                    DisplayMap[y + 1, x - 1] = BombMap[y + 1, x - 1];
                    SafeTilesRemaining--;
                }
            }
        }

        public void Die()
        {
            isNotDead = false;

            //Show unclicked bombs
            for (int x = 0; x < GameTileColoumns; x++)
            {
                for (int y = 0; y < GameTileRows; y++)
                {
                    if (BombMap[y, x] == 9)
                        DisplayMap[y, x] = 11;
                }
            }

            AITurnedOn = false;
            AIPaused = true;
        }

        public void Win()
        {
            hasWon = true;
            AITurnedOn = false;
            AIPaused = true;
        }

        public int GetNumberOfUnclikedTilesAround(int x, int y)
        {
            int Tiles = 0;
            if (x - 1 >= 0 && DisplayMap[y, x - 1] == 0)
                Tiles++;
            if (x - 1 >= 0 && y - 1 >= 0 && DisplayMap[y - 1, x - 1] == 0)
                Tiles++;
            if (y - 1 >= 0 && DisplayMap[y - 1, x] == 0)
                Tiles++;
            if (x + 1 < GameTileColoumns && y - 1 >= 0 && DisplayMap[y - 1, x + 1] == 0)
                Tiles++;
            if (x + 1 < GameTileColoumns && DisplayMap[y, x + 1] == 0)
                Tiles++;
            if (x + 1 < GameTileColoumns && y + 1 < GameTileRows && DisplayMap[y + 1, x + 1] == 0)
                Tiles++;
            if (y + 1 < GameTileRows && DisplayMap[y + 1, x] == 0)
                Tiles++;
            if (x - 1 >= 0 && y + 1 < GameTileRows && DisplayMap[y + 1, x - 1] == 0)
                Tiles++;

            return Tiles;
        }

        public int GetNumberOfFlagTilesAround(int x, int y)
        {
            int Tiles = 0;
            if (x - 1 >= 0 && DisplayMap[y, x - 1] == 12)
                Tiles++;
            if (x - 1 >= 0 && y - 1 >= 0 && DisplayMap[y - 1, x - 1] == 12)
                Tiles++;
            if (y - 1 >= 0 && DisplayMap[y - 1, x] == 12)
                Tiles++;
            if (x + 1 < GameTileColoumns && y - 1 >= 0 && DisplayMap[y - 1, x + 1] == 12)
                Tiles++;
            if (x + 1 < GameTileColoumns && DisplayMap[y, x + 1] == 12)
                Tiles++;
            if (x + 1 < GameTileColoumns && y + 1 < GameTileRows && DisplayMap[y + 1, x + 1] == 12)
                Tiles++;
            if (y + 1 < GameTileRows && DisplayMap[y + 1, x] == 12)
                Tiles++;
            if (x - 1 >= 0 && y + 1 < GameTileRows && DisplayMap[y + 1, x - 1] == 12)
                Tiles++;

            return Tiles;
        }

        public List<Vector2> GetUnclickedTilesAround(int x, int y)
        {
            List<Vector2> list = new List<Vector2>();
            if (x - 1 >= 0 && DisplayMap[y, x - 1] == 0)
                list.Add(new Vector2(x - 1, y));
            if (x - 1 >= 0 && y - 1 >= 0 && DisplayMap[y - 1, x - 1] == 0)
                list.Add(new Vector2(x - 1, y - 1));
            if (y - 1 >= 0 && DisplayMap[y - 1, x] == 0)
                list.Add(new Vector2(x, y - 1));
            if (x + 1 < GameTileColoumns && y - 1 >= 0 && DisplayMap[y - 1, x + 1] == 0)
                list.Add(new Vector2(x + 1, y - 1));
            if (x + 1 < GameTileColoumns && DisplayMap[y, x + 1] == 0)
                list.Add(new Vector2(x + 1, y));
            if (x + 1 < GameTileColoumns && y + 1 < GameTileRows && DisplayMap[y + 1, x + 1] == 0)
                list.Add(new Vector2(x + 1, y + 1));
            if (y + 1 < GameTileRows && DisplayMap[y + 1, x] == 0)
                list.Add(new Vector2(x, y + 1));
            if (x - 1 >= 0 && y + 1 < GameTileRows && DisplayMap[y + 1, x - 1] == 0)
                list.Add(new Vector2(x - 1, y + 1));
            return list;
        }

        public List<Vector2> GetNumberedTilesAround(int x, int y)
        {
            List<Vector2> list = new List<Vector2>();
            if (x - 1 >= 0 && DisplayMap[y, x - 1] > 0 && DisplayMap[y, x - 1] < 9)
                list.Add(new Vector2(x - 1, y));
            if (x - 1 >= 0 && y - 1 >= 0 && DisplayMap[y - 1, x - 1] > 0 && DisplayMap[y - 1, x - 1] < 9)
                list.Add(new Vector2(x - 1, y - 1));
            if (y - 1 >= 0 && DisplayMap[y - 1, x] > 0 && DisplayMap[y - 1, x] < 9)
                list.Add(new Vector2(x, y - 1));
            if (x + 1 < GameTileColoumns && y - 1 >= 0 && DisplayMap[y - 1, x + 1] > 0 && DisplayMap[y - 1, x + 1] < 9)
                list.Add(new Vector2(x + 1, y - 1));
            if (x + 1 < GameTileColoumns && DisplayMap[y, x + 1] > 0 && DisplayMap[y, x + 1] < 9)
                list.Add(new Vector2(x + 1, y));
            if (x + 1 < GameTileColoumns && y + 1 < GameTileRows && DisplayMap[y + 1, x + 1] > 0 && DisplayMap[y + 1, x + 1] < 9)
                list.Add(new Vector2(x + 1, y + 1));
            if (y + 1 < GameTileRows && DisplayMap[y + 1, x] > 0 && DisplayMap[y + 1, x] < 9)
                list.Add(new Vector2(x, y + 1));
            if (x - 1 >= 0 && y + 1 < GameTileRows && DisplayMap[y + 1, x - 1] > 0 && DisplayMap[y + 1, x - 1] < 9)
                list.Add(new Vector2(x - 1, y + 1));
            return list;
        }



        public void RunAI()
        {
            Mouse.SetPosition(MapLeftOffset, MapTopOffset);
            Turn = 0;
            AITurnedOn = true;
            AIPaused = false;
            LeftClickTileBuffer = new List<Vector2>();
            RightClickTileBuffer = new List<Vector2>();
            LuckyRightClickTileBuffer = new List<Vector2>();
            LuckyLeftClickTileBuffer = new List<Vector2>();
        }

        public void PulseAI(GameTime gameTime)
        {
            if (!AIPaused)
            {
                elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (Turn < 5)
                {
                    if (elapsedTime >= waitTime)
                    {
                        Random rnd = new Random();
                        TileChosenX = rnd.Next(0, GameTileColoumns);
                        TileChosenY = rnd.Next(0, GameTileRows);

                        AIMoveMouse(gameTime, TileChosenX, TileChosenY);
                        AIClick();
                        elapsedTime = 0;
                    }
                }

                if (Turn >= 5)
                {
                    for (int x = 0; x < GameTileColoumns; x++)
                    {
                        for (int y = 0; y < GameTileRows; y++)
                        {
                            if (DisplayMap[y, x] != 0 && DisplayMap[y, x] != 10 && DisplayMap[y, x] != 12)
                            {
                                if (DisplayMap[y, x] == GetNumberOfFlagTilesAround(x, y))
                                {

                                    foreach (Vector2 pos in GetUnclickedTilesAround(x, y))
                                    {
                                        if (!LeftClickTileBuffer.Contains(pos))
                                        {
                                            LeftClickTileBuffer.Add(pos);
                                            LuckyRightClickTileBuffer.Clear();
                                            LuckyLeftClickTileBuffer.Clear();
                                        }
                                    }
                                }
                                else if (DisplayMap[y, x] == GetNumberOfUnclikedTilesAround(x, y) + GetNumberOfFlagTilesAround(x, y))
                                {
                                    foreach (Vector2 pos in GetUnclickedTilesAround(x, y))
                                    {
                                        if (!RightClickTileBuffer.Contains(pos))
                                        {
                                            RightClickTileBuffer.Add(pos);
                                            LuckyRightClickTileBuffer.Clear();
                                            LuckyLeftClickTileBuffer.Clear();
                                        }
                                        if (LeftClickTileBuffer.Contains(pos))
                                        {
                                            LeftClickTileBuffer.Remove(pos);
                                            LuckyRightClickTileBuffer.Clear();
                                            LuckyLeftClickTileBuffer.Clear();
                                        }
                                    }

                                }
                                else if (LeftClickTileBuffer.Count == 0 && RightClickTileBuffer.Count == 0)
                                {
                                    if (GetNumberOfUnclikedTilesAround(x, y) > 1 && DisplayMap[y, x] > GetNumberOfFlagTilesAround(x, y))
                                    {
                                        foreach (Vector2 unclickedtile in GetUnclickedTilesAround(x, y))
                                        {
                                            if (GetNumberedTilesAround(x, y).Count != 0)
                                                foreach (Vector2 number in GetNumberedTilesAround(x, y))
                                                {
                                                    if (DisplayMap[(int)number.Y, (int)number.X] >= GetNumberOfFlagTilesAround(x, y))
                                                    {
                                                        if (!LuckyRightClickTileBuffer.Contains(unclickedtile))
                                                            LuckyRightClickTileBuffer.Add(unclickedtile);
                                                    }
                                                }
                                            else
                                            {
                                                if (!LuckyRightClickTileBuffer.Contains(unclickedtile))
                                                    LuckyRightClickTileBuffer.Add(unclickedtile);
                                            }
                                        }
                                    }
                                }
                                else continue;
                            }
                            else continue;
                        }
                    }

                    if (elapsedTime >= waitTime)
                    {
                        if (RightClickTileBuffer.Count != 0)
                        {
                            AIMoveMouse(gameTime, (int)RightClickTileBuffer[0].X, (int)RightClickTileBuffer[0].Y);
                            AIRightClick();
                            RightClickTileBuffer.Remove(RightClickTileBuffer[0]);
                            LuckyLeftClickTileBuffer.Clear();
                            LuckyRightClickTileBuffer.Clear();
                            elapsedTime = 0;
                        }
                        else if (LeftClickTileBuffer.Count != 0)
                        {
                            AIMoveMouse(gameTime, (int)LeftClickTileBuffer[0].X, (int)LeftClickTileBuffer[0].Y);
                            AIClick();
                            LeftClickTileBuffer.Remove(LeftClickTileBuffer[0]);
                            LuckyLeftClickTileBuffer.Clear();
                            LuckyRightClickTileBuffer.Clear();
                            elapsedTime = 0;
                        }
                        else if (LuckyRightClickTileBuffer.Count != 0 && LeftClickTileBuffer.Count == 0 && RightClickTileBuffer.Count == 0)
                        {
                            Random rnd = new Random();
                            int random = rnd.Next(0, LuckyRightClickTileBuffer.Count - 1);
                            AIMoveMouse(gameTime, (int)LuckyRightClickTileBuffer[random].X, (int)LuckyRightClickTileBuffer[random].Y);
                            AIRightClick();
                            LuckyRightClickTileBuffer.Remove(LuckyRightClickTileBuffer[0]);
                            elapsedTime = 0;
                        }
                        else if (LuckyLeftClickTileBuffer.Count != 0 && LeftClickTileBuffer.Count == 0 && RightClickTileBuffer.Count == 0)
                        {
                            Random rnd = new Random();
                            int random = rnd.Next(0, LuckyLeftClickTileBuffer.Count - 1);
                            AIMoveMouse(gameTime, (int)LuckyLeftClickTileBuffer[random].X, (int)LuckyLeftClickTileBuffer[random].Y);
                            AIClick();
                            LuckyLeftClickTileBuffer.Remove(LuckyLeftClickTileBuffer[0]);
                            elapsedTime = 0;
                        }
                    }
                }
            }

        }

        public void AIMoveMouse(GameTime gameTime, int TileChosenX, int TileChosenY)
        {
            Mouse.SetPosition(MapLeftOffset + (TileChosenX * TileWidth) + (TileWidth / 2), MapTopOffset + (TileChosenY * TileHeight) + (TileHeight / 2));
        }

        public void AIClick()
        {
            MouseState mouseState = Mouse.GetState();
            //On bomb tile click
            if (isNotDead)
                if (mouseState.Position.X > MapLeftOffset && mouseState.Position.X < (MapLeftOffset + (GameTileColoumns * TileWidth)) && mouseState.Position.Y > MapTopOffset && mouseState.Position.Y < (MapTopOffset + (GameTileRows * TileHeight)))
                {
                    int TileClickedX = (mouseState.Position.X - MapLeftOffset) / TileWidth;
                    int TileClickedY = (mouseState.Position.Y - MapTopOffset) / TileHeight;
                    if (DisplayMap[TileClickedY, TileClickedX] != 12)
                    {
                        switch (BombMap[TileClickedY, TileClickedX])
                        {
                            case 0:
                                DisplayMap[TileClickedY, TileClickedX] = 10;
                                CheckForEmptyTiles(TileClickedX, TileClickedY);
                                break;
                            case 9:
                                Die();
                                break;
                            case 12:
                                break;
                            default:
                                DisplayMap[TileClickedY, TileClickedX] = BombMap[TileClickedY, TileClickedX];
                                break;
                        }
                        Turn++;
                    }

                }
        }

        public void AIRightClick()
        {
            MouseState mouseState = Mouse.GetState();
            if (isNotDead)
            {
                if (mouseState.Position.X > MapLeftOffset && mouseState.Position.X < (MapLeftOffset + (GameTileColoumns * TileWidth)) && mouseState.Position.Y > MapTopOffset && mouseState.Position.Y < (MapTopOffset + (GameTileRows * TileHeight)))
                {
                    int TileClickedX = (mouseState.Position.X - MapLeftOffset) / TileWidth;
                    int TileClickedY = (mouseState.Position.Y - MapTopOffset) / TileHeight;
                    if (DisplayMap[TileClickedY, TileClickedX] == 0)
                    {
                        DisplayMap[TileClickedY, TileClickedX] = 12;
                        GameBombCounter--;
                    }
                    else if (DisplayMap[TileClickedY, TileClickedX] == 12)
                    {
                        DisplayMap[TileClickedY, TileClickedX] = 0;
                        GameBombCounter++;
                    }
                    Turn++;
                }
            }
        }
    }
}
