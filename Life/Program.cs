using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Numerics;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }
        public Board(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                string str;
                str = sr.ReadLine();
                int width = Int32.Parse(str);
                str = sr.ReadLine();
                int height = Int32.Parse(str);
                str = sr.ReadLine();
                int cellSize = Int32.Parse(str);

                CellSize = cellSize;
                Cells = new Cell[width / cellSize, height / cellSize];
                for (int x = 0; x < Columns; x++)
                    for (int y = 0; y < Rows; y++)
                        Cells[x, y] = new Cell();
                ConnectNeighbors();

                str = sr.ReadToEnd();
                str.Replace("\n", "");
                str.Replace("\r", "");
                int ind = 0;
                for (int y = 0; y < Rows; ++y)
                {
                    for (int x = 0; x < Columns; ++x)
                    {
                        if (str[ind] == '*')
                        {
                            Cells[x, y].IsAlive = true;
                        }
                        if (str[ind] == ' ')
                        {
                            Cells[x, y].IsAlive = false;
                        }
                        ++ind;
                    }
                    ++ind;
                }
            }
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
        public void saveToFile(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                string str;
                str = Width.ToString();
                sw.WriteLine(str);
                str = Height.ToString();
                sw.WriteLine(str);
                str = CellSize.ToString();
                sw.WriteLine(str);
                for (int row = 0; row < Rows; row++)
                {
                    for (int col = 0; col < Columns; col++)
                    {
                        var cell = Cells[col, row];
                        if (cell.IsAlive)
                        {
                            sw.Write('*');
                        }
                        else
                        {
                            sw.Write(' ');
                        }
                    }
                    sw.Write('\n');
                }
            }
        }
    }
    class Program
    {
        static bool isBloc(List<int[]> list)
        {
            if (list.Count != 4)
            {
                return false;
            }
            int minx = 99999999, miny = 999999999;
            foreach (var t in list)
            {
                if ((t[0] <= minx) && (t[1] <= miny))
                {
                    minx = t[0];
                    miny = t[1];
                }
            }
            int xR = (minx < board.Columns - 1) ? minx + 1 : 0;
            int yB = (miny < board.Rows - 1) ? miny + 1 : 0;
            int[] result = new int[3];
            foreach (var t in list)
            {
                if ((t[0] == xR) && (t[1] == miny))
                {
                    result[0] = 1;
                }
                if ((t[0] == minx) && (t[1] == yB))
                {
                    result[1] = 1;
                }
                if ((t[0] == xR) && (t[1] == yB))
                {
                    result[2] = 1;
                }
            }
            if ((result[0] == 1) && (result[1] == 1) && (result[2] == 1))
            {
                return true;
            }
            return false;
        }

        static Board board;
        static private void Reset()
        {
            board = new Board(
                width: 50,
                height: 20,
                cellSize: 1,
                liveDensity: 0.5);
        }
        static private void Reset(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                string str = sr.ReadToEnd();
                int i = 0;
                while (str[i] != '{')
                {
                    ++i;
                }
                char state = 'a';
                string paramName = "";
                string param = "";
                int width = 0, height = 0, cellSize = 0;
                double liveDensity = 0;
                while (str[i] != '}')
                {
                    if ((state == 'a') && (str[i] == '\"'))
                    {
                        state = 'p';
                        paramName = "";
                        ++i;
                        continue;
                    }
                    else if ((state == 'p') && (str[i] == '\"'))
                    {
                        state = 'w';
                        ++i;
                        continue;
                    }
                    else if (state == 'p')
                    {
                        paramName += str[i];
                        ++i;
                        continue;
                    }
                    else if ((state == 'w') && (str[i] == ':'))
                    {
                        state = 'd';
                        param = "";
                        ++i;
                        continue;
                    }
                    else if (state == 'w')
                    {
                        ++i;
                        continue;
                    }
                    else if ((state == 'd') && (str[i] == ','))
                    {
                        state = 'a';
                        if (paramName == "width")
                        {
                            width = Int32.Parse(param);
                        }
                        else if (paramName == "height")
                        {
                            height = Int32.Parse(param);
                        }
                        else if (paramName == "cellSize")
                        {
                            cellSize = Int32.Parse(param);
                        }
                        else if (paramName == "liveDensity")
                        {
                            liveDensity = Double.Parse(param);
                        }
                        ++i;
                        continue;
                    }
                    else if ((state == 'd') && (str[i] != '\r') && (str[i] != '\n'))
                    {
                        param += str[i];
                        ++i;
                        continue;
                    }
                    ++i;
                }
                if (state == 'd')
                {
                    if (paramName == "width")
                    {
                        width = Int32.Parse(param);
                    }
                    else if (paramName == "height")
                    {
                        height = Int32.Parse(param);
                    }
                    else if (paramName == "cellSize")
                    {
                        cellSize = Int32.Parse(param);
                    }
                    else if (paramName == "liveDensity")
                    {
                        IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
                        liveDensity = double.Parse(param, formatter);
                    }
                }
                board = new Board(width, height, cellSize, liveDensity);
            }
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)   
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
        static void findComb(List<int[]> list, int x, int y)
        {
            Queue < int[] > temp = new Queue<int[]>();
            int[] t = new int[2];
            t[0] = x;
            t[1] = y;
            temp.Enqueue(t);
            list.Add(t);
            while(temp.Count > 0)
            {
                int[] cur = temp.Dequeue();
                int x1 = cur[0];
                int y1 = cur[1];
                int xL = (x1 > 0) ? x1 - 1 : board.Columns - 1;
                int xR = (x1 < board.Columns - 1) ? x1 + 1 : 0;
                int yT = (y1 > 0) ? y1 - 1 : board.Rows - 1;
                int yB = (y1 < board.Rows - 1) ? y1 + 1 : 0;

                if (board.Cells[xL, yT].IsAlive)
                {
                    int[] t1 = new int[2];
                    t1[0] = xL;
                    t1[1] = yT;
                    bool flag = true;
                    foreach (var r in list)
                    {
                        if ((t1[0] == r[0]) && (t1[1] == r[1]))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        list.Add(t1);
                        temp.Enqueue(t1);
                    }
                }
                if (board.Cells[x1, yT].IsAlive)
                {
                    int[] t1 = new int[2];
                    t1[0] = x1;
                    t1[1] = yT;
                    bool flag = true;
                    foreach (var r in list)
                    {
                        if ((t1[0] == r[0]) && (t1[1] == r[1]))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        list.Add(t1);
                        temp.Enqueue(t1);
                    }
                }
                if (board.Cells[xR, yT].IsAlive)
                {
                    int[] t1 = new int[2];
                    t1[0] = xR;
                    t1[1] = yT;
                    bool flag = true;
                    foreach (var r in list)
                    {
                        if ((t1[0] == r[0]) && (t1[1] == r[1]))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        list.Add(t1);
                        temp.Enqueue(t1);
                    }
                }
                if (board.Cells[xL, y1].IsAlive)
                {
                    int[] t1 = new int[2];
                    t1[0] = xL;
                    t1[1] = y1;
                    bool flag = true;
                    foreach (var r in list)
                    {
                        if ((t1[0] == r[0]) && (t1[1] == r[1]))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        list.Add(t1);
                        temp.Enqueue(t1);
                    }
                }
                if (board.Cells[xR, y1].IsAlive)
                {
                    int[] t1 = new int[2];
                    t1[0] = xR;
                    t1[1] = y1;
                    bool flag = true;
                    foreach (var r in list)
                    {
                        if ((t1[0] == r[0]) && (t1[1] == r[1]))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        list.Add(t1);
                        temp.Enqueue(t1);
                    }
                }
                if (board.Cells[xL, yB].IsAlive)
                {
                    int[] t1 = new int[2];
                    t1[0] = xL;
                    t1[1] = yB;
                    bool flag = true;
                    foreach (var r in list)
                    {
                        if ((t1[0] == r[0]) && (t1[1] == r[1]))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        list.Add(t1);
                        temp.Enqueue(t1);
                    }
                }
                if (board.Cells[x1, yB].IsAlive)
                {
                    int[] t1 = new int[2];
                    t1[0] = x1;
                    t1[1] = yB;
                    bool flag = true;
                    foreach (var r in list)
                    {
                        if ((t1[0] == r[0]) && (t1[1] == r[1]))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        list.Add(t1);
                        temp.Enqueue(t1);
                    }
                }
                if (board.Cells[xR, yB].IsAlive)
                {
                    int[] t1 = new int[2];
                    t1[0] = xR;
                    t1[1] = yB;
                    bool flag = true;
                    foreach (var r in list)
                    {
                        if ((t1[0] == r[0]) && (t1[1] == r[1]))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        list.Add(t1);
                        temp.Enqueue(t1);
                    }
                }
            }
        }
        static List<List<int[]>> countComb()
        {
            List < List < int[] >> listCombin = new List<List<int[]>>();
            for (int x = 0; x < board.Columns; ++x)
            {
                for (int y = 0; y < board.Rows; ++y)
                {
                    if (board.Cells[x,y].IsAlive)
                    {
                        List<int[]> combin = new List<int[]>();
                        findComb(combin, x, y);
                        bool flag = true;
                        foreach (var list in listCombin)
                        {
                            foreach(var a in list)
                            {
                                if (a[0] == combin.First()[0])
                                {
                                    if (a[1] == combin.First()[1])
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (flag)
                        {
                            listCombin.Add(combin);
                        }
                    }
                }
            }
            return listCombin;
        }
        static void Main(string[] args)
        {
            Reset("settings.json");
            board = new Board("save.txt");
            Render();
            List<List<int[]>> listCombin = countComb();
            Console.Write("Это блок? ");
            Console.WriteLine(isBloc(listCombin.First()));
            Console.Write("Количество комбинаций ");
            Console.Write(listCombin.Count);
            Console.Write("\n");
            Console.WriteLine("Нажмите любую кнопку");
            Console.ReadKey();
            foreach (var list in listCombin)
            {
                Console.Write("Клеток в комбинации: ");
                Console.Write(list.Count);
                Console.Write("\n");
            }
            //board.saveToFile("save.txt");
            //for (int i = 0; i < 5; ++i)
            //{
            //    Console.Clear();
            //    Render();
            //    board.Advance();
            //    Thread.Sleep(1000);
            //}
            //board = new Board("save.txt");
            //for (int i = 0; i < 5; ++i)
            //{
            //    Console.Clear();
            //    Render();
            //    board.Advance();
            //    Thread.Sleep(1000);
            //}

            Reset();
            bool[,] last = new bool[board.Columns, board.Rows];
            int step = 0;
            while (true)
            {
                Console.Clear();
                Render();
                for (int row = 0; row < board.Rows; row++)
                {
                    for (int col = 0; col < board.Columns; col++)
                    {
                        last[col,row] = board.Cells[col, row].IsAlive;
                    }
                }
                ++step;
                board.Advance();
                bool flag = true;
                for (int row = 0; row < board.Rows; row++)
                {
                    for (int col = 0; col < board.Columns; col++)
                    {
                        if (last[col, row] != board.Cells[col, row].IsAlive)
                        {
                            flag = false;
                        }
                    }
                }
                if (flag)
                {
                    Console.Write("Количество итераций до стабильной фазы ");
                    Console.WriteLine(step);
                    break;
                }
                Thread.Sleep(100);
            }
            Console.ReadKey();
        }
    }
}