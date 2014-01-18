using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace MyTetris
{

    public enum CheckSide { Left, Right };

    static class PlayBox
    {

        private const int _bs = 16;                         // brick size in pixels
        private const int _brickTypeNum = 5;                // size of brick type queue

        private static int _width = 10;                     // size of game screen (in bricks, not pixels!!!)
        private static int _height = 20;

        private static BaseShape _actualShape = null;       // handle actual shape
        private static BaseShape _nextShape = null;         // handle next shape
        
        private static int _xOffset = 0;                    //  shape x position (in bricks unit) 
        private static int _yOffset = 0;            

        private static bool[,] _collisionBox = new bool[_width, _height];   // handle collision box

        // Doublebuffer hack                                // no GDI+ flick
        private static PictureBox _innerPic = null;
        private static Bitmap _bufferedData = null;
        //

        private static PictureBox _previewPic = null;       // target object for next shape preview

        public delegate void ShowScore(int sc);             // delegate for score method handle
        public static ShowScore ScoreDlg;

        public delegate void GameOver();                    // delegate for game over handle
        public static GameOver GameOverDlg;

        private static bool playing = false;                // true if game is running


        public static void StartGame()
        {

            // Create actual shape
            Random rnd = new Random();
            int brick = rnd.Next(_brickTypeNum);
            _actualShape = ShapeQueue.GetShape(brick);

            // Init
            playing = true;
            _collisionBox = new bool[_width, _height];
            _xOffset = 0;
            _yOffset = 0;


            GetNextShape();                                 // for shape preview            
            DrawShape();                                    // draw actual shape before first tick
            
        }

        private static void GetNextShape()
        {
            Random rnd = new Random();
            int brick = rnd.Next(_brickTypeNum);
            
            for(int i=0; i < DateTime.Now.Second; i++)
                brick = rnd.Next(_brickTypeNum);
            
            _nextShape = ShapeQueue.GetShape(brick);
            DrawNextShape();
        }


        public static void SetOutput(PictureBox pb,PictureBox previewBox)
        {
            _innerPic = pb;
            _previewPic = previewBox;
        }

        public static void DrawBox()
        {
   
            bool isGameOver = CheckGameOver();

            if (isGameOver)
            {
                CheckCol();
                GameOverDlg();
                playing = false;
            }

            DrawPlayground();
            DrawShape();
        }

        private static void DrawPlayground()
        {
            _bufferedData = new Bitmap(_innerPic.Width, _innerPic.Height);

            SolidBrush myBrush = new SolidBrush(Color.Black);


            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    Rectangle ee = new Rectangle(i * _bs, j * _bs, _bs + 1, _bs + 1);


                    using (Graphics graf = Graphics.FromImage(_bufferedData))
                    {
                        var pen = new Pen(Color.Silver, 1);
                        graf.FillRectangle(myBrush, ee);
                    }
                }
            }

           
            Rectangle bx = new Rectangle(0, 0, _width * _bs, _height * _bs);


            using (Graphics graf = Graphics.FromImage(_bufferedData))
            {
                var pen = new Pen(Color.Silver, 1);
                graf.DrawRectangle(pen, bx);
            }  
            
            _innerPic.Image = _bufferedData;
        }

        private static void DrawNextShape()
        {
            Bitmap bp = new Bitmap(_previewPic.Width, _previewPic.Height);
            bool[,] tmp = _nextShape.GetMatrix();

            Color col = System.Drawing.ColorTranslator.FromHtml(_nextShape.GetShapeColor());
            SolidBrush myBrush = new SolidBrush(col);

            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if ((i < 4 && j < 4) && tmp[i, j] == true)
                    {
                        Rectangle ee = new Rectangle(i * _bs, j * _bs, _bs, _bs);

                        using (Graphics graf = Graphics.FromImage(bp))
                        {
                            graf.FillRectangle(myBrush, ee);
                            var pen = new Pen(Color.White, 1);
                            graf.DrawRectangle(pen, ee);
                        }

                    }
                }
            }

                _previewPic.Image = bp;
        }

        private static void DrawShape()
        {
            if (playing)
            {
                bool[,] tmp = _actualShape.GetMatrix();

                Color col = System.Drawing.ColorTranslator.FromHtml(_actualShape.GetShapeColor());
                SolidBrush myBrush = new SolidBrush(col);

                for (int i = 0; i < _width; i++)
                {
                    for (int j = 0; j < _height; j++)
                    {
                        if ((i < 4 && j < 4) && tmp[i, j] == true)
                        {
                            Rectangle ee = new Rectangle(i * _bs + _xOffset * _bs, j * _bs + _yOffset * _bs, _bs, _bs);

                            using (Graphics graf = Graphics.FromImage(_bufferedData))
                            {
                                graf.FillRectangle(myBrush, ee);
                                var pen = new Pen(Color.White, 1);
                                graf.DrawRectangle(pen, ee);
                            }

                        }
                    }
                }
            }

            _innerPic.Image = _bufferedData; 

            DrawCol();
        }

        public static void DoTick()
        {
            if (playing)
            {

                DrawNextShape();

                if (CheckCol() == false && (_yOffset * _bs < _height * _bs - (_actualShape.GetHeight() * _bs)))
                {
                    _yOffset = _yOffset + 1;
                }
                else
                {
                    bool[,] tmp = _actualShape.GetMatrix();

                    for (int i = 0; i < _width; i++)
                    {
                        for (int j = 0; j < _height; j++)
                        {
                            if ((i < 4 && j < 4) && tmp[i, j] == true)
                            {
                                _collisionBox[i + _xOffset, j + _yOffset] = true;

                            }
                        }
                    }

                    ScoreDlg(CountScore());


                    _xOffset = 0;
                    _yOffset = 0;

                    

                    _actualShape = _nextShape;
                    GetNextShape();

                }
            }
        }

        private static bool CheckCol()
        {
            bool toRet = false;
            bool[,] tmp = _actualShape.GetMatrix();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                    if (tmp[i, j] == true)
                    {

                        if (_yOffset + j + 1 >= _height)        // no collision, last line                            
                            return true;

                        if (_collisionBox[_xOffset + i, _yOffset + j + 1] == true)        // check collision bottom
                        {
                            toRet = true;                           
                        }

                    }
                }
            }

            
                

            return toRet;
        }

        private static bool [,] RemoveLineArray(bool[,] inData, int removeLine)
        {
            List<bool[]> tmpData = new List<bool[]>();

            for (int i = 0; i < _height; i++)
            {
                if (i == removeLine)
                {
                    continue;
                }

                bool[] oneLine = new bool[_width];

                for (int j = 0; j < _width; j++)
                {
                    oneLine[j] = inData[j, i];
                }

                tmpData.Add(oneLine);

            }

            bool[,] m = new bool[_width, _height];
            int rOffset = 1;


            foreach (bool[] k in tmpData)
            {
                int lOffset = 0;
                foreach (bool l in k)
                {
                    m[lOffset, rOffset] = l;
                    lOffset = lOffset + 1;
                }
                rOffset = rOffset + 1;

            }
            
            return m;
        }

        private static int CountScore()
        {
            List<int> fireLine = new List<int>();

            // first collect all lines to fire
            for (int i = _height-1; i >= 0 ; i--)
            {
                bool completed = false;
                
                bool[] tmpArr = new bool[_width];

                for (int j = 0; j < _width; j++)
                {
                    if (_collisionBox[j, i] == true)
                    {
                        completed = true;
                    }
                    else
                    {
                        completed = false;
                        break;
                    }
                }

                if (completed)
                {
                    fireLine.Add(i);
                }
            }

            // now iterate lines to remove and recalc new offset
            int shiftLines = 0;
            foreach(int a in fireLine)
            {
                _collisionBox = RemoveLineArray(_collisionBox, a + shiftLines);
                shiftLines = shiftLines + 1;        // one line was removed, recalc new offset 
            }

            return (shiftLines * 10) * shiftLines;
                        
        }

        private static bool CheckColSide(CheckSide cs)
        {
            bool toRet = false;
            bool[,] tmp = _actualShape.GetMatrix();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                    if (tmp[i, j] == true)
                    {

                        if ( cs == CheckSide.Left && (_xOffset + i - 1 >= 0) && _collisionBox[_xOffset + i - 1, _yOffset + j] == true)  // check collision left
                        {
                            toRet = true;
                        }

                        if ( cs == CheckSide.Right && (_xOffset + i + 1 < _width) && _collisionBox[_xOffset + i + 1, _yOffset + j] == true)  // check collision right
                        {
                            toRet = true;
                        }
                    }
                }
            }

            return toRet;
        }

        public static void RotateShape()
        {
            // exception rape
            try
            {
                _actualShape.DoRotate();
                CheckCol();
            }
            catch
            {
                _actualShape.RevertRotate();
            }
        }
        
        public static void DrawCol()
        {

            SolidBrush myBrush = new SolidBrush(Color.SkyBlue);

            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if (_collisionBox[i, j] == true)
                    {
                        Rectangle ee = new Rectangle(i * _bs + 1, j * _bs + 1, _bs, _bs);                        

                        using (Graphics graf = Graphics.FromImage(_bufferedData))
                        {
                            graf.FillRectangle(myBrush, ee);
                            var pen = new Pen(Color.Gold, 1);
                            graf.DrawRectangle(pen, ee);
                        }
                    }
                }
            }

            _innerPic.Image = _bufferedData;
           
        }

        public static void MoveRight()
        {
            var a = _actualShape.GetWidth() + 1;
            if (_xOffset + a < _width)
            {
                bool canMove = CheckColSide(CheckSide.Right);

                if (!canMove)
                {
                    _xOffset = _xOffset + 1;
                }
            }
        }

        public static void MoveLeft()
        {
            var a = _actualShape.GetWidth()+1;
            if (_xOffset - 1 >= 0)
            {
                
                bool canMove = CheckColSide(CheckSide.Left);

                if (!canMove)
                {
                    _xOffset = _xOffset - 1;                 
                }
            }
        }
        
        public static void MoveDown()
        {
            DoTick();            
        }
        
        public static void SaveGame(string fileName)
        {
            List<string> allData = new List<string>();

            for(int i = 0; i < _height; i++)
            {

                string[] oneLine = new string[_width];

                for (int j = 0; j < _width; j++)
                {
                    oneLine[j] = _collisionBox[j, i].ToString();
                }

                allData.Add(String.Join(";", oneLine));
            }

            string serializeArray = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Join(",", allData.ToArray())));
            
            string nl = Environment.NewLine;

            File.WriteAllText(fileName, serializeArray + nl);
            File.AppendAllText(fileName, ShapeQueue.GetIdent().ToString() + nl);
            File.AppendAllText(fileName, _xOffset.ToString() + nl);
            File.AppendAllText(fileName, _yOffset.ToString());

        }
        
        public static void LoadGame(string fileName)
        {
            string[] getLines = File.ReadAllLines(fileName);
            string[] tmpArr = Encoding.UTF8.GetString(Convert.FromBase64String(getLines[0])).Split(',');

            for (int i = 0; i < _height; i++)
            {       
                string[] getData = tmpArr[i].Split(';');
                for (int j = 0; j < _width; j++)
                {
                    _collisionBox[j, i] = Convert.ToBoolean(getData[j]);
                }
            }

            _actualShape = ShapeQueue.GetShape(Int32.Parse(getLines[1]));
            _xOffset = Int32.Parse(getLines[2]);
            _yOffset = Int32.Parse(getLines[3]);


        }

        public static bool CheckGameOver()
        {

            bool isOver = false;

            if (_actualShape != null)
            {
                int gh = _actualShape.GetHeight() + 1;

                for (int a = 0; a < gh; a++)
                {
                    for (int i = 0; i < _width; i++)
                    {
                        if (_collisionBox[i, a] == true)
                        {
                            isOver = true;
                            break;
                        }
                    }
                }
            }
            return isOver;
        }
    }
}
