using System;
using System.Collections.Generic;
using System.Text;

namespace MyTetris
{
    public abstract class BaseShape
    {
        protected bool[,] _matrix = new bool[4, 4];
        protected bool[,] _backup = new bool[4, 4];
        protected string _color = null;

        public BaseShape(string color)
        {
            _color = color;
        }
        public bool[,] GetMatrix()
        {
            return this._matrix;
        }
        public int GetWidth()
        {

            int toRet = 0;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (this._matrix[i, j] == true && toRet < i)
                    {
                        toRet = i;
                    }
                }
            }

            return toRet;
        }
        public int GetHeight()
        {

            int toRet = 0;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (this._matrix[i, j] == true && toRet < j)
                    {
                        toRet = j;
                    }
                }
            }

            return toRet;
        }
        private void ResizeArray<T>(ref T[,] original, int newCoNum, int newRoNum)
        {
            var newArray = new T[newCoNum, newRoNum];
            int columnCount = original.GetLength(1);
            int columnCount2 = newRoNum;
            int columns = original.GetUpperBound(0);
            for (int co = 0; co <= columns; co++)
                Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);
            original = newArray;
        }
        private bool[,] RotateRight(bool[,] matrix, int sizeX, int sizeY)
        {
            int lengthY = sizeX;
            int lengthX = sizeY;
            bool[,] result = new bool[lengthX, lengthY];
            for (int y = 0; y < lengthY; y++)
                for (int x = 0; x < lengthX; x++)
                    result[x, y] = matrix[lengthY - 1 - y, x];
            return result;
        }
        private bool[,] RotateLeft(bool[,] matrix, int sizeX, int sizeY)
        {
            int lengthY = sizeX;
            int lengthX = sizeY;
            bool[,] result = new bool[lengthX, lengthY];
            for (int y = 0; y < lengthY; y++)
                for (int x = 0; x < lengthX; x++)
                    result[x, y] = matrix[y, lengthX - 1 - x];
            return result;
        }
        public void DoRotate()
        {
            Buffer.BlockCopy(_matrix, 0, _backup, 0, _matrix.Length * sizeof(bool));

            var a = GetWidth();
            var b = GetHeight();
            bool[,] c = RotateLeft(this._matrix, a + 1, b + 1);

            ResizeArray(ref c, 4, 4);


            this._matrix = c;
        }
        public void RevertRotate()
        {
            _matrix = _backup;
        }
        public string GetShapeColor()
        {
            return _color;
        }

    }

    // This block define all game shapes
    class MyShape1 : BaseShape
    {
        public MyShape1(string color) : base(color)
        {
            this._matrix[0, 0] = true;
            this._matrix[1, 0] = true;
            this._matrix[2, 0] = true;
            this._matrix[3, 0] = true;
        }
    }
    class MyShape2 : BaseShape
    {
        public MyShape2(string color) : base(color)
        {
            this._matrix[0, 0] = true;
            this._matrix[1, 0] = true;
            this._matrix[2, 1] = true;
            this._matrix[1, 1] = true;
        }
    }
    class MyShape3 : BaseShape
    {
        public MyShape3(string color) : base(color)
        {
            this._matrix[1, 0] = true;
            this._matrix[0, 1] = true;
            this._matrix[1, 1] = true;
            this._matrix[2, 1] = true;
        }
    }
    class MyShape4 : BaseShape
    {
        public MyShape4(string color) : base(color)
        {
            this._matrix[0, 0] = true;
            this._matrix[0, 1] = true;
            this._matrix[1, 0] = true;
            this._matrix[1, 1] = true;
        }
    }
    class MyShape5 : BaseShape
    {
        public MyShape5(string color) : base(color)
        {
            this._matrix[1, 0] = true;
            this._matrix[2, 0] = true;
            this._matrix[0, 1] = true;
            this._matrix[1, 1] = true;
        }
    }
    // End block of shapes

    // class for shapes generating
    public static class ShapeQueue
    {
        private static BaseShape _innerShape = null;
        private static int _ident = -1;                             // needed for reconstruction after game load


        public static BaseShape GetShape(int ident)
        {

            System.Threading.Thread.Sleep(DateTime.Now.Second);      // random generator failed :(

            string rc = RandomColor();

            switch (ident)
            {
                case 0: _innerShape = new MyShape1(rc);
                    _ident = ident;
                    break;
                case 1: _innerShape = new MyShape2(rc);
                    _ident = ident;
                    break;
                case 2: _innerShape = new MyShape3(rc);
                    _ident = ident;
                    break;
                case 3: _innerShape = new MyShape4(rc);
                    _ident = ident;
                    break;
                case 4: _innerShape = new MyShape5(rc);
                    _ident = ident;
                    break;
                default: throw new Exception("Unknown shape!");
                    
            }
            return _innerShape;
        }
        public static int GetIdent()                            // for easily game restore (no reflection needed)
        {
            return _ident;
        }


        private static string RandomColor()
        {
            Random rnd = new Random();
            string hexOutput = String.Format("{0:X}", rnd.Next(0, 0xFFFFFF));

            while (hexOutput.Length < 6)
                hexOutput = "0" + hexOutput;
            return "#" + hexOutput;
        }

    }
}
