using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;


/*****
 * INFORMATION
 * 
 *  20 rows , 10 columns 
 *  20x 10 matrix 
 *  7 pieces 
 *  6 known sets
 * 
 * 
 * 
 * 
 */



namespace Botten_Anna
{
    public class FacebookTetris
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32.Dll", EntryPoint = "SendMessageA", SetLastError = true)]
        public static extern bool SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        IntPtr FlashWindow;
        //Columns by rows err wtf
        public bool[,] MapGame = new bool[10, 20];
        int R;// value of red for no block in space
        int G;// value of greddn for no block in space
        int B;// value of blue for no block in space
        bool DEGUB_MODE;
        TetrisPieces[] Que;
        public enum TetrisPieces : uint
        {
            LeftZ = 0,
            RightZ = 1,
            Strait = 2,
            Cube = 3,
            LeftL = 4, //BLUE
            RightL = 5, //ORANGE
            Tee = 6,
            UNKNOWN = 7
        }

        public Color MapPieceToColor(TetrisPieces piece) {
            Color c = new Color();
            if (TetrisPieces.RightZ == piece) {
                return Color.FromArgb(0, 108, 252, 1);
            }
            else if (TetrisPieces.LeftZ == piece) {
                return Color.FromArgb(0, 252, 0, 0);
            }
            else if (TetrisPieces.LeftL == piece) {

                return Color.FromArgb(0, 0, 67, 252);
            }
            else if (TetrisPieces.RightL == piece) {
                return Color.FromArgb(0, 244, 106, 5);
            }
            else if (TetrisPieces.Strait == piece) {
                return Color.FromArgb(0, 4, 210, 252);
            }
            else if (TetrisPieces.Tee == piece) {
                return Color.FromArgb(0, 248, 133, 253);
            }
            else if (TetrisPieces.Cube == piece) {
                return Color.FromArgb(0, 223, 209, 81);
            }
            else {
                return Color.FromArgb(0, 0, 0, 0);
            }
        }

        private TetrisPieces MapColorToPiece(Color c) {
            //TODO make sure none overlap
            if (c.R < 120 && c.R > 90 && c.G > 240 && c.G < 254 && c.B >= 0 && c.B <= 15) {
                // Rightz
                return TetrisPieces.RightZ;
            }
            else if (c.R > 248 && c.G < 15 && c.B < 15) {
                return TetrisPieces.LeftZ;
            }
            else if (c.R < 20 && c.G < 88 && c.G > 47 && c.B > 235) {
                return TetrisPieces.LeftL;
            }
            else if (c.R > 234 && c.G > 80 && c.G < 120 && c.B < 18) {
                return TetrisPieces.RightL;
            }
            else if (c.R < 23 && c.G > 189 && c.G < 225 && c.B > 235) {
                return TetrisPieces.Strait;
            }
            else if (c.R > 230 && c.G < 150 && c.G > 113 && c.B > 245) {
                return TetrisPieces.Tee;
            }
            else if (c.R < 240 && c.R > 205 && c.G > 180 && c.G < 220 && c.B > 60 && c.B < 101) {
                return TetrisPieces.Cube;
            }
            else {
                Console.Write("WARNING OSR FAILED. BLOCK NOT RECOGNIZED\n");
                return TetrisPieces.UNKNOWN;
            }




        }

        //check has moved()


        public FacebookTetris(IntPtr window, int r, int g, int b) {
            FlashWindow = window;
            R = r;
            G = g;
            B = b;
            DEGUB_MODE = true;
            Que = new TetrisPieces[6]; //Current to last
            UpdateGame();
        }

        public FacebookTetris Copy() {
            return this;
        }

        public bool UpdateGame() {
            System.Drawing.Bitmap bmp = extractImage();
            //Color[] s = BuldColorListFromRectangle(bmp, new Point(481, 200), new Point(489, 240));
            //s=BuldColorListFromLists(s,BuldColorListFromRectangle(bmp, new Point(406, 174), new Point(504, 184)));
            ////Color[] First = new Color[1] { new Color() };
            //foreach (Color c in s)
            //{
            //    Console.Write("Color.FromArgb(255," + c.R + "," + c.G + "," + c.B + ") ,");

            //}
            bool isNew=RetrieveMap(bmp);
            if (isNew)
                UpdatePieces(bmp);
            return isNew;
        }



        private Color[] BuldColorListFromLists(Color[] s, Color[] c) {
            ArrayList list = new ArrayList();
            foreach (Color color in s) {
                if (!list.Contains(color)) {
                    list.Add(color);
                }
            }
            foreach (Color color in c) {
                if (!list.Contains(color)) {
                    list.Add(color);
                }
            }
            return list.ToArray(typeof(Color)) as Color[];
        }

        private Color[] BuldColorListFromRectangle(System.Drawing.Bitmap bmp, Point TopLeft, Point BottomLeft) {
            ArrayList list = new ArrayList();
            for (int i = TopLeft.Y; i < BottomLeft.Y; i++) {
                for (int z = TopLeft.X; z < BottomLeft.X; z++) {
                    try {
                        Color c = bmp.GetPixel(z, i);
                        if (!list.Contains(c)) {
                            list.Add(c);
                        }
                    }
                    catch (IndexOutOfRangeException e) {
                        Console.Write("Invalid BMP DOESNT CONTAIN POINT: " + z + "," + i + "\n");
                        return null;
                    }
                }
            }
            return list.ToArray(typeof(Color)) as Color[];
        }

        /**
         * Find Color by average nonBackground color pixels 
         * 
         * 
         */
        private bool ColorInList(Color[] colors, Color color) {

            foreach (Color c in colors) {
                if (c.Equals(color))
                    return true;
            }
            return false;
        }

        private Color FindColorByAverage(System.Drawing.Bitmap bmp, Color[] background, Point TopLeft, Point BottomLeft) {
            long redAverage = 0;
            long greenAverage = 0;
            long blueAverage = 0;
            long Pixles = 0;
            for (int i = TopLeft.Y; i < BottomLeft.Y; i++) {
                for (int z = TopLeft.X; z < BottomLeft.X; z++) {
                    try {

                        Color c = bmp.GetPixel(z, i);
                        if (!ColorInList(background, c)) {
                            redAverage += c.R;
                            blueAverage += c.B;
                            greenAverage += c.G;
                            Pixles++;
                            bmp.SetPixel(z, i, Color.Black);
                        }
                    }
                    catch (IndexOutOfRangeException e) {
                        Console.Write("Invalid BMP DOESNT CONTAIN POINT: " + z + "," + i + "\n");
                        return Color.FromArgb(0, 0, 0);
                    }
                }
            }

            if (Pixles == 0) {

            }
            else {
                redAverage /= Pixles;
                blueAverage /= Pixles;
                greenAverage /= Pixles;

            }
            // Console.Write("PIXEL: " + Pixles + "----");
            return Color.FromArgb((int)redAverage, (int)greenAverage, (int)blueAverage);
        }

        private TetrisPieces hardCodeded(Color c) {
            if (c.R < 126 && c.R > 100 && c.G > 190 && c.G < 210 && c.B >= 45 && c.B <= 70) {
                // Rightz
                if (DEGUB_MODE)
                    Console.Write("RIGHT-Z\n");
                return TetrisPieces.RightZ;

            }
            else if (c.R > 190 && c.R <= 210 && c.G < 65 && c.G > 45 && c.B > 45 && c.B < 65) {
                if (DEGUB_MODE)
                    Console.Write("Left-Z\n");
                return TetrisPieces.LeftZ;

            }
            else if (c.R < 65 && c.R > 55 && c.G < 108 && c.G > 90 && c.B < 207 && c.B > 190) {
                if (DEGUB_MODE)
                    Console.Write("LEFT-L\n");
                return TetrisPieces.LeftL;

            }
            else if (c.R >= 200 && c.R < 210 && c.G >= 120 && c.G <= 136 && c.B < 76 && c.B > 58) {
                if (DEGUB_MODE)
                    Console.Write("RIGHT-L\n");
                return TetrisPieces.RightL;
            }
            else if (c.R < 65 && c.R > 55 && c.G > 170 && c.G < 180 && c.B > 190 && c.B < 210) {
                if (DEGUB_MODE)
                    Console.Write("Line\n");
                return TetrisPieces.Strait;
            }
            else if (c.R > 190 && c.R < 209 && c.G < 65 && c.G > 55 && c.B > 190 && c.B < 203) {
                if (DEGUB_MODE)
                    Console.Write("Tee\n");
                return TetrisPieces.Tee;

            }
            else if (c.R < 210 && c.R > 188 && c.G > 180 && c.G < 200 && c.B > 55 && c.B < 70) {
                if (DEGUB_MODE)
                    Console.Write("Cube\n");
                return TetrisPieces.Cube;
            }
            else {
                Console.Write("WARNING OSR FAILED. BLOCK NOT RECOGNIZED\n");
                return TetrisPieces.UNKNOWN;
            }


        }

        private void UpdatePieces(System.Drawing.Bitmap bmp) {
            //  bmp.Save("C:\\Users\\0xFFFF\\Documents\\Tetris Bot\\test.bmp");
            //new Point(406, 176), new Point(481, 250)
            //Backgrounds..... 
            //413,271 - 489,324
            //backgrounds...
            //413,325 - 489,373
            //backgrounds...
            //413,373 - 489,420
            //backgrounds
            //413,420 - 489,474

            Color[] FirstBackGroundColors = new Color[] { Color.FromArgb(255, 217, 223, 235), Color.FromArgb(255, 194, 203, 223), Color.FromArgb(255, 40, 66, 140), Color.FromArgb(255, 241, 243, 247), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 73, 95, 157), Color.FromArgb(255, 174, 184, 211), Color.FromArgb(255, 139, 154, 193), Color.FromArgb(255, 107, 125, 175), Color.FromArgb(255, 183, 193, 217), Color.FromArgb(255, 53, 77, 147), Color.FromArgb(255, 62, 85, 151), Color.FromArgb(255, 214, 219, 233), Color.FromArgb(255, 95, 115, 169), Color.FromArgb(255, 128, 144, 187), Color.FromArgb(255, 147, 160, 197), Color.FromArgb(255, 161, 173, 205), Color.FromArgb(255, 93, 113, 168), Color.FromArgb(255, 172, 183, 211), Color.FromArgb(255, 80, 101, 161), Color.FromArgb(255, 205, 213, 229), Color.FromArgb(255, 120, 136, 183), Color.FromArgb(255, 117, 134, 181), Color.FromArgb(255, 84, 105, 163), Color.FromArgb(255, 187, 195, 219), Color.FromArgb(255, 66, 89, 154), Color.FromArgb(255, 134, 148, 190), Color.FromArgb(255, 106, 124, 175), Color.FromArgb(255, 51, 75, 145), Color.FromArgb(255, 201, 207, 226), Color.FromArgb(255, 250, 251, 252), Color.FromArgb(255, 211, 216, 231), Color.FromArgb(255, 79, 101, 161), Color.FromArgb(255, 237, 240, 245), Color.FromArgb(255, 224, 228, 238), Color.FromArgb(255, 132, 147, 189), Color.FromArgb(255, 150, 164, 199), Color.FromArgb(255, 172, 181, 210), Color.FromArgb(255, 198, 205, 224), Color.FromArgb(255, 67, 89, 154) };
            Color c = FindColorByAverage(bmp, FirstBackGroundColors, new Point(416, 176), new Point(485, 250));
            //Console.Write("Color1:" + c.R + "," + c.G + "," + c.B + "\n");
            TetrisPieces p;
            if (DEGUB_MODE)
                Console.Write("1:");
            if (c.R == 61 && c.G == 180 && c.B == 201) {
                if (DEGUB_MODE)
                    Console.Write("Next BLOCK IS LINE\n");
                p = TetrisPieces.Strait;
            }
            else if (c.R == 201 && c.G == 193 && c.B == 63) {
                if (DEGUB_MODE)
                    Console.Write("Next BLOCK IS Square\n");
                p = TetrisPieces.Cube;
            }
            else if (c.R == 212 && c.G == 129 && c.B == 70) {
                if (DEGUB_MODE)
                    Console.Write("Next BLOCK IS Right-L\n");
                p = TetrisPieces.RightL;
            }
            else if (c.R == 62 && c.G == 103 && c.B == 202) {
                if (DEGUB_MODE)
                    Console.Write("Next BLOCK IS Left-L\n");
                p = TetrisPieces.LeftL;
            }
            else if (c.R == 201 && c.G == 61 && c.B == 60) {
                if (DEGUB_MODE)
                    Console.Write("Next BLOCK IS Left-z\n");
                p = TetrisPieces.LeftZ;
            }
            else if (c.R == 202 && c.G == 63 && c.B == 201) {
                if (DEGUB_MODE)
                    Console.Write("Next BLOCK IS Tee\n");
                p = TetrisPieces.Tee;

            }
            else if (c.R == 123 && c.G == 202 && c.B == 63) {
                if (DEGUB_MODE)
                    Console.Write("Next BLOCK IS Right-z\n");
                p = TetrisPieces.RightZ;
            }
            else {
                if (DEGUB_MODE)
                    Console.Write("Next BLOCK IS UNKOWN COLORS LINKED: \n");
                p = TetrisPieces.UNKNOWN;
            }
            Que[1] = p;
            Color[] OtherThanFirstBackGround = new Color[1] { Color.FromArgb(255, 243, 244, 248) };
            c = FindColorByAverage(bmp, OtherThanFirstBackGround, new Point(413, 271), new Point(489, 324));
            if (DEGUB_MODE)
                Console.Write("2:");

            Que[2] = hardCodeded(c);
            c = FindColorByAverage(bmp, OtherThanFirstBackGround, new Point(413, 326), new Point(489, 372));
            if (DEGUB_MODE)
                Console.Write("3:");
            Que[3] = hardCodeded(c);
            c = FindColorByAverage(bmp, OtherThanFirstBackGround, new Point(413, 374), new Point(489, 419));
            if (DEGUB_MODE)
                Console.Write("4:");
            Que[4] = hardCodeded(c);
            c = FindColorByAverage(bmp, OtherThanFirstBackGround, new Point(413, 421), new Point(489, 474));
            if (DEGUB_MODE)
                Console.Write("5:");
            Que[5] = hardCodeded(c);
            // Console.Write("Color5:" + c.R + "," + c.G + "," + c.B + "\n");
        }

        private void checkIfStateChange() {

        }

        private bool RetrieveMap(System.Drawing.Bitmap bmp) {
            const int StartingBlockX = 182;
            const int StartingBlockY = 172;
            int currentX;
            int currentY;
            //get bitmap of screen

            Point[] CurrentBlock = new Point[4];
            
            //iterate by ROWS
            int count = 0;
            bool hasStateChange = false;
            for (int i = 0; i < 20; i++) {
                currentY = StartingBlockY + i * (9 + 3 + 10);

                for (int z = 0; z < 10; z++) {

                    currentX = StartingBlockX + z * (9 + 3 + 10);

                                                     //Decayed-bmp.SetPixel(currentX, currentY, Color.Black);
                    //get pixel of bit map
                    Color c = bmp.GetPixel(currentX, currentY);
                    //check if pixel RGB is something that is white (no block)
                    if ((c.B == B & c.G == G && c.R == R)) {
                        if (MapGame[z, i] == false) {
                        } else {
                            MapGame[z, i] = false;
                            hasStateChange = true;
                        }
                    }
                    else if (c.R == 210 && c.G == 210 && c.B == 211) {
                        if (MapGame[z, i] == false) {
                        } else {
                            MapGame[z, i] = false;
                            hasStateChange = true;
                        }
                       
                        CurrentBlock[count++] = new Point(z, i);
                    }
                    else //there is a block
                    {
                        //Console.WriteLine(c.R + "," + c.G + "," + c.B);
                        if (MapGame[z, i] == true) {
                        } else {
                            MapGame[z, i] = true;
                            hasStateChange = true;
                        }
                    }


                }
                // bmp.Save("C:\\bit.bmp");
            }

            //used to generate testing maps
            //Console.Write("\n\n\n" + "bool[,] map=new bool[,]{");


            //for (int z = 9; z >= 0; z--) {
            //    Console.Write("{");
            //    for (int i = 19; i >= 0; i--) {
            //        if (MapGame[z, i]) {
            //            Console.Write("true");
            //        } else {
            //            Console.Write("false");
            //        }
            //        if (i != 0)
            //            Console.Write(",");
            //    }
            //    Console.Write("}, ");
            //}
            //Console.Write("}\n");

            //find piece
            //reduce to 0 relitive quoords
            if (hasStateChange == true) {
                int smallY = CurrentBlock[0].Y;
                int smallX = CurrentBlock[0].X;
                for (int i = 0; i < 4; i++) {
                    //find smallest x
                    //find smallest y
                    if (CurrentBlock[i].X < smallX)
                        smallX = CurrentBlock[i].X;
                    if (CurrentBlock[i].Y < smallY)
                        smallY = CurrentBlock[i].Y;
                }
                for (int i = 0; i < 4; i++) {
                    CurrentBlock[i].X -= smallX;
                    CurrentBlock[i].Y -= smallY;
                }
                //preform heuristics
                //BUILT THOUGH TEST DRIVEN DEV
                if (DEGUB_MODE)
                    Console.Write("0:");
                if (CurrentBlock[0].Y == CurrentBlock[1].Y && CurrentBlock[1].Y == CurrentBlock[2].Y && CurrentBlock[1].Y == CurrentBlock[3].Y) {
                    if (DEGUB_MODE)
                        Console.WriteLine("LINE CURRENT DROP");
                    Que[0] = TetrisPieces.Strait;
                } else if (CurrentBlock[0].Y == CurrentBlock[1].Y && CurrentBlock[2].Y == CurrentBlock[3].Y && CurrentBlock[3].X == 2 && CurrentBlock[0].Y == 1) {
                    if (DEGUB_MODE)
                        Console.WriteLine("LEfT Z CURRENT DROP");
                    Que[0] = TetrisPieces.LeftZ;
                } else if (CurrentBlock[0].Y == CurrentBlock[1].Y && CurrentBlock[2].Y == CurrentBlock[3].Y && CurrentBlock[0].Y == 0 && CurrentBlock[3].X == 1 && CurrentBlock[1].X == 2) {
                    if (DEGUB_MODE)
                        Console.WriteLine("Right Z CURRENT DROP");
                    Que[0] = TetrisPieces.RightZ;
                } else if (CurrentBlock[0].Y == CurrentBlock[1].Y && CurrentBlock[2].Y == CurrentBlock[3].Y && CurrentBlock[0].Y == 0 && CurrentBlock[0].X != CurrentBlock[1].X) {
                    if (DEGUB_MODE)
                        Console.WriteLine("Cube CURRENT DROP");
                    Que[0] = TetrisPieces.Cube;
                } else if (CurrentBlock[0].Y != CurrentBlock[1].Y && CurrentBlock[2].Y == CurrentBlock[3].Y && CurrentBlock[3].X == 2 && CurrentBlock[0].X == 0) {
                    if (DEGUB_MODE)
                        Console.WriteLine("Left-L CURRENT DROP");
                    Que[0] = TetrisPieces.LeftL;
                } else if (CurrentBlock[0].Y != CurrentBlock[1].Y && CurrentBlock[2].Y == CurrentBlock[3].Y && CurrentBlock[3].X == 2 && CurrentBlock[0].X == 2) {
                    if (DEGUB_MODE)
                        Console.WriteLine("Right-L CURRENT DROP");
                    Que[0] = TetrisPieces.RightL;
                } else if (CurrentBlock[0].Y != CurrentBlock[1].Y && CurrentBlock[2].Y == CurrentBlock[3].Y && CurrentBlock[3].X == 2 && CurrentBlock[1].X == 0) {
                    if (DEGUB_MODE)
                        Console.WriteLine("Tee CURRENT DROP");
                    Que[0] = TetrisPieces.Tee;
                }
            }
            return hasStateChange;
        }




        public System.Drawing.Bitmap extractImage() {
            return CaptureWindow(FlashWindow);
        }


        public System.Drawing.Bitmap CaptureWindow(IntPtr hWnd) {
            System.Drawing.Rectangle rctForm = System.Drawing.Rectangle.Empty;

            using (System.Drawing.Graphics grfx = System.Drawing.Graphics.FromHdc(GetWindowDC(hWnd))) {
                rctForm = System.Drawing.Rectangle.Round(grfx.VisibleClipBounds);
            }

            System.Drawing.Bitmap pImage = new System.Drawing.Bitmap(rctForm.Width, rctForm.Height);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(pImage);

            IntPtr hDC = graphics.GetHdc();
            //paint control onto graphics using provided options        
            try {
                PrintWindow(hWnd, hDC, (uint)0);
            }
            catch {
                if (DEGUB_MODE)
                    Console.Write("Hat i got a printWindow Failure what u do?!?!\n");
            }
            finally {
                graphics.ReleaseHdc(hDC);
            }
            return pImage;
        }



    }






    public class TetrisAI
    {





      IntPtr FlashWindow;
        MatrixSolver SolvingDisplay;
        public TetrisAI(IntPtr window, int m, int n) {
            FlashWindow = window;
            SolvingDisplay=new MatrixSolver(m, n);
            SolvingDisplay.Show();


            visualMatrix();
        }

        
        

        public void Move(FacebookTetris UptodateMap) {
            // analize all possible moves!! whelp crap
            /*
             * 
             * 
             * 
             * There is always 4 blocks
             * They must always stay in reletive position
             * they can only be placed on top of top blocks
             */
            SolvingDisplay.setMatrix(UptodateMap.MapGame);

            //


        }
        private static void visualMatrix(bool[][,] p) {

            for (int r = 0; r < p.GetLength(0); r++) {
                for (int i = 3; i >= 0; i--) { //rows

                    for (int z = 0; z < 4; z++) {
                        if (p[r][z, i])
                            Console.Write("*");
                        else
                            Console.Write(" ");
                    }
                    Console.Write("\n");

                }
                Console.Write("--------------\n");
            }
        }

        public static void visualMatrix(){
            bool[][,] p = PieceToMatracies(FacebookTetris.TetrisPieces.Cube);
            visualMatrix(p);
            p = PieceToMatracies(FacebookTetris.TetrisPieces.LeftL);
            visualMatrix(p);
            p = PieceToMatracies(FacebookTetris.TetrisPieces.RightL);
            visualMatrix(p);
            p = PieceToMatracies(FacebookTetris.TetrisPieces.LeftZ);
            visualMatrix(p);
            p = PieceToMatracies(FacebookTetris.TetrisPieces.Tee);
            visualMatrix(p);
            p = PieceToMatracies(FacebookTetris.TetrisPieces.Strait);
            visualMatrix(p);
            p = PieceToMatracies(FacebookTetris.TetrisPieces.RightZ);
            visualMatrix(p);
        }

        private static bool[][,] PieceToMatracies(FacebookTetris.TetrisPieces r) {
            bool[][,] returned;
            if (r == FacebookTetris.TetrisPieces.Cube) {
                returned = new bool[1][,];
                returned[0] = new bool[4, 4];
                returned[0][0, 0] = true;
                returned[0][0, 1] = true;
                returned[0][1, 0] = true;
                returned[0][1, 1] = true;
            } else if (r == FacebookTetris.TetrisPieces.Strait) {
                returned = new bool[2][,];
                returned[0] = new bool[4, 4];
                returned[1] = new bool[4, 4];
                returned[0][0, 0] = true;
                returned[0][1, 0] = true;
                returned[0][2, 0] = true;
                returned[0][3, 0] = true;

                returned[1][0, 0] = true;
                returned[1][0, 1] = true;
                returned[1][0, 2] = true;
                returned[1][0, 3] = true;
            } else if (r == FacebookTetris.TetrisPieces.Tee) {
                returned = new bool[4][,];
                returned[0] = new bool[4, 4];
                returned[1] = new bool[4, 4];
                returned[2] = new bool[4, 4];
                returned[3] = new bool[4, 4];
                
                returned[0][0, 0] = true;
                returned[0][1, 0] = true;
                returned[0][2, 0] = true;
                returned[0][1, 1] = true;

                returned[1][0, 0] = true;
                returned[1][0, 1] = true;
                returned[1][0, 2] = true;
                returned[1][1, 1] = true;

                returned[2][0, 1] = true;
                returned[2][1, 0] = true;
                returned[2][1, 1] = true;
                returned[2][1, 2] = true;

                returned[3][0, 1] = true;
                returned[3][1, 1] = true;
                returned[3][2, 1] = true;
                returned[3][1, 0] = true;
            } else if (r == FacebookTetris.TetrisPieces.LeftL) {
                //BLUE
                returned = new bool[4][,];
                returned[0] = new bool[4, 4];
                returned[1] = new bool[4, 4];
                returned[2] = new bool[4, 4];
                returned[3] = new bool[4, 4];


                returned[0][0, 0] = true;
                returned[0][1, 0] = true;
                returned[0][1, 1] = true;
                returned[0][1, 2] = true;

                returned[1][0, 0] = true;
                returned[1][0, 1] = true;
                returned[1][1, 0] = true;
                returned[1][2, 0] = true;

                returned[2][0, 0] = true;
                returned[2][0, 1] = true;
                returned[2][0, 2] = true;
                returned[2][1, 2] = true;

                returned[3][0, 1] = true;
                returned[3][1, 1] = true;
                returned[3][2, 1] = true;
                returned[3][2, 0] = true;

            } else if (r == FacebookTetris.TetrisPieces.RightL) {
                returned = new bool[4][,];
                returned[0] = new bool[4, 4];
                returned[1] = new bool[4, 4];
                returned[2] = new bool[4, 4];
                returned[3] = new bool[4, 4];

                returned[0][0, 0] = true;
                returned[0][0, 1] = true;
                returned[0][0, 2] = true;
                returned[0][1, 0] = true;

                returned[1][0, 1] = true;
                returned[1][1, 1] = true;
                returned[1][2, 1] = true;
                returned[1][0, 0] = true;

                returned[2][0, 2] = true;
                returned[2][1, 0] = true;
                returned[2][1, 1] = true;
                returned[2][1, 2] = true;

                returned[3][0, 0] = true;
                returned[3][1, 0] = true;
                returned[3][2, 0] = true;
                returned[3][0, 1] = true;
            } else if (r == FacebookTetris.TetrisPieces.LeftZ) {
                returned = new bool[2][,];
                returned[0] = new bool[4, 4];
                returned[1] = new bool[4, 4];

                returned[0][0, 1] = true;
                returned[0][1, 1] = true;
                returned[0][1, 0] = true;
                returned[0][2, 0] = true;

                returned[1][1, 2] = true;
                returned[1][1, 1] = true;
                returned[1][0, 1] = true;
                returned[1][0, 0] = true;

            } else/*(r == FacebookTetris.TetrisPieces.RightZ)*/ {
                returned = new bool[2][,];
                returned[0] = new bool[4, 4];
                returned[1] = new bool[4, 4];

                returned[0][0, 0] = true;
                returned[0][1, 0] = true;
                returned[0][1, 1] = true;
                returned[0][2, 1] = true;

                returned[1][0, 2] = true;
                returned[1][0, 1] = true;
                returned[1][1, 1] = true;
                returned[1][1, 0] = true;

            }
          
            return returned;
        }


      

    }


    public partial class Form1 : Form
    {
        bool DEGUB_MODE = true;
        int LoadedObjects = 0;
        IntPtr BrowserHandle;
        FacebookTetris GameMap;
        TetrisAI AI;
        

        public void DumbScreenShot()//Testing only
        {
            Bitmap b = new Bitmap(WB.ClientSize.Width, WB.ClientSize.Height);
            Graphics g = Graphics.FromImage(b);
            g.CopyFromScreen(WB.Parent.PointToScreen(WB.Location), new Point(0, 0), WB.ClientSize);
            //The bitmap is ready. Do whatever you please with it!
            b.Save("C:\\bit.bmp");
            MessageBox.Show("Bitmap Saved!");


        }

        int R;// value of red for no block in space
        int G;// value of greddn for no block in space
        int B;// value of blue for no block in space

        public Form1() {
            InitializeComponent();
            timer1.Interval = (50);
            timer1.Start();
            //something about mode selection goes here
            //c.B == 248 & c.G == 244 && c.R == 243
            //tetris something or rather:::::::::::
            R = 243;
            G = 244;
            B = 248;
        }



        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
            // While the page has not yet loaded, set the text.
            this.Text = "Navigating";
        }

        private void webBrowser1_DocumentCompleted(object sender,
            WebBrowserDocumentCompletedEventArgs e) {
            // Better use the e parameter to get the url.
            // ... This makes the method more generic and reusable.
            this.Text = e.Url.ToString() + " loaded";
            LoadedObjects++;

        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32.Dll", EntryPoint = "SendMessageA", SetLastError = true)]
        public static extern bool SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        //        StringBuilder WindowName;
        //        // x, and y for start button
        public const int Startx = 362;
        public const int Starty = 295;

        private void StartGame() {   //lParam is 0127016A
            LeftClick(Startx, Starty);
        }

        private void LeftClick(int x, int y) {
            int lParam = y * 65535 + x + y;

            PostMessage(FlashWindow, (uint)WM.LBUTTONDOWN, (IntPtr)1, (IntPtr)lParam);
            PostMessage(FlashWindow, (uint)WM.LBUTTONUP, IntPtr.Zero, (IntPtr)lParam);
            //PostMessage(BrowserHandle, (uint)WM.LBUTTONDOWN, (IntPtr)1,(IntPtr) lParam);
            //Thread.Sleep(100);
            //PostMessage(BrowserHandle, (uint)WM.LBUTTONUP, (IntPtr)0, (IntPtr)lParam);
            //Thread.Sleep(100);
            //SendMessage(BrowserHandle, (uint)WM.LBUTTONDOWN, IntPtr.Zero, (IntPtr)lParam);
            //Thread.Sleep(100);
            //SendMessage(BrowserHandle, (uint)WM.LBUTTONUP, IntPtr.Zero, (IntPtr)lParam);
            //SendMessage(FlashWindow, (uint)WM.LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
            //SendMessage(FlashWindow, (uint)WM.LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
            //PostMessage(FlashWindow, (uint)WM.LBUTTONDOWN, IntPtr.Zero, (IntPtr)lParam);
            //PostMessage(FlashWindow, (uint)WM.LBUTTONUP, IntPtr.Zero, (IntPtr)lParam);
            //SendMessage(FlashWindow, (uint)WM.LBUTTONDOWN, IntPtr.Zero, (IntPtr)lParam);
            //SendMessage(FlashWindow, (uint)WM.LBUTTONUP, IntPtr.Zero, (IntPtr)lParam);

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) { }

        private void WB_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            if (e.KeyValue == (int)VK.F1) {
                Console.Write("DO STUFF NOW" + "\n");
                GameMap.UpdateGame();
                
                AI.Move(GameMap.Copy());
            }

        }


        private void Form1_Load_1(object sender, EventArgs e) {
            WB.Navigate("https://apps.facebook.com/tetrisfriends/play.php?ref=ts");
        }

        private void Form1_Shown(object sender, EventArgs e) {
            Application.DoEvents();
            while (WB.IsBusy && LoadedObjects > 6) {
                Application.DoEvents();
            }


        }
        long time = 0;
        bool GameStarted = false;
        private void timer1_Tick(object sender, EventArgs e) {
            time += 50;
            if (time == 10000) {
                LoadedSite();
            }
            else if (time == 10000 + 3000) {
                GameStarted = true;
                GameMap = new FacebookTetris(FlashWindow, R, G, B);
                AI = new TetrisAI(FlashWindow,10,20);
            }else if (GameStarted) {
                //if (GameMap.UpdateGame()) {
                    //AI.Move(GameMap.Copy());
                    //Console.Write("Game Update\n");
               // }
            }
        }
        IntPtr FlashWindow;
        private void LoadedSite() {

            Console.Write("Loaded Webpage. The Fun has been DOUBLED" + "\n");
            StringBuilder classname = new System.Text.StringBuilder(100);
            IntPtr ExplorerHandle = WB.Handle;
            GetClassName(ExplorerHandle, classname, classname.Capacity);

            while (!classname.ToString().Equals("Internet Explorer_Server")) {
                Console.Write(classname.ToString() + "\n");
                ExplorerHandle = GetWindow(ExplorerHandle, GetWindow_Cmd.GW_CHILD);
                GetClassName(ExplorerHandle, classname, classname.Capacity);

            }
            BrowserHandle = ExplorerHandle;
            FlashWindow = ExplorerHandle = GetWindow(ExplorerHandle, GetWindow_Cmd.GW_CHILD);
            StartGame();

        }



    }


}