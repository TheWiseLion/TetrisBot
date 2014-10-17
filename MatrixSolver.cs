using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Botten_Anna
{
    public partial class MatrixSolver : Form
    {
        public static bool[,] map1 = new bool[,] { { true, false, true, true, true, true, true, true, true, true }, { true, false, false, false, true, false, true, true, true, false }, { true, false, false, false, false, false, false, false, false, false }, { true, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false }, { false, false, false, false, false, false, false, false, false, false } };
       
        private void InitializeComponents() {   
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Text = "Fun with graphics";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint); 
        }
        
        private void MainForm_Paint(object sender, System.Windows.Forms.PaintEventArgs e) {
            //create a graphics object from the form
            Graphics g = this.CreateGraphics();
            MyMatrix.reDraw(g, this.Height, this.Width);
        }
        private void Form1_Resize(object sender, System.EventArgs e) {
            // Invalidate();  See ctor!
        }
        SolutionMatrix MyMatrix;
        public MatrixSolver(int m, int n) {
            
            InitializeComponent();
            InitializeComponents();
            MyMatrix = new SolutionMatrix(m, n, this.Height, this.Width);
          
            this.reDraw();
        }
        
        public void reDraw(){
            MyMatrix.reDraw(this.CreateGraphics(), this.Height, this.Width);
        }

        public void setMatrix(bool[,] inMatrix) {
            MyMatrix.setMatrix(inMatrix);
            this.reDraw();
        }

        private void MatrixSolver_Load(object sender, EventArgs e) {

        }

    }


    public class SolutionMatrix
    {
        bool[,] matrix;
        public readonly int m, n;
        private  int H, W;
        //m= width n= hieght
        public SolutionMatrix(int m, int n, int hieght, int width,bool WidthHight=true) {
            this.m = m;
            this.n = n;
            H = hieght;
            W = width;
            matrix = new bool[m, n];
            for (int i = 0; i < m; i++) {
                for (int z = 0; z < n; z++) {
                    matrix[i, z] = false;
                }
            }

        }


        public void setMatrix(bool[,] inMatrix) {
            if (inMatrix.Length == m * n) {
                for (int i = 0; i < m; i++) {
                    for (int z = 0; z < n; z++) {
                        matrix[i, z] = inMatrix[i, z];
                    }
                }
            }
            else {
                Console.Write("INVALID INPUT MATRIX\n");
            }
        }
        private Pen blackP = new Pen(Color.Black, 1);
        private Pen whiteP = new Pen(Color.White, 1);
        private Brush black = new SolidBrush(Color.Black);
        private Brush white = new SolidBrush(Color.White);
        private Brush blue;
        private Brush green;
        private Brush yellow;
        private Brush pink;
        private Brush teal;
        private Brush orange;

        public void reDraw(Graphics g,int hieght, int width) {
            H = hieght;
            W = width;
            if (H > 50 && W > 50) {
                int setSizeW = (W / m)-1;
                int setSizeH = (H / n)-1;
               
                //0,0 is TOP LEFT corner 
                //20,0 is Bottom LEFT
                //0,10 is top Right
                //20,10 is bottom Right
                int alignX = 3;
                int alignY = 3;
                    for (int i = 0; i < n; i++) { 

                        for (int z = 0; z < m; z++) { 
                            
                            g.DrawRectangle(blackP, new Rectangle(alignX-1, alignY-1, setSizeW+1,  setSizeH+1));
                            if (matrix[z, i]) {
                                g.FillRectangle(black, new Rectangle(alignX , alignY , setSizeW - 1, setSizeH - 1));
                            }else {
                                g.FillRectangle(white, new Rectangle(alignX+1 , alignY +1, setSizeW - 1, setSizeH - 1));
                            }
                            alignX += setSizeW;
                        }
                        alignY += setSizeH;
                        alignX = 3;
                    }

            }
        }


    }



}
