using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Sample1_Triangle
{
    public partial class MainForm : Form
    {
        int vaoHandle; // vertex array buffer handle
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Application.Idle += Application_Idle;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (GLControl.IsIdle)
                Render();
        }

        private void GLControl_Load(object sender, EventArgs e)
        {
            // set up the clear color
            GL.ClearColor(Color.Black);

            // set up the viewport
            GL.Viewport(0, 0, GLControl.Width, GLControl.Width);

            float width = (float)GLControl.Width;
            float height = (float)GLControl.Height;

            // set up the perspective projection matrix
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 perspective = 
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, width / height, 0.1f, 200.0f);
            GL.LoadMatrix(ref perspective);

            // set up the modelview matrix
            GL.MatrixMode(MatrixMode.Modelview);
            Matrix4 modelview = Matrix4.LookAt(5, 5, 0, 0, 0, 0, 0, 1, 0);
            GL.LoadMatrix(ref modelview);

            Render();
        }

        private void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            EnableShaders();

            GL.BindVertexArray(vaoHandle);
            GL.DrawArrays(BeginMode.Triangles, 0, 3);

            GLControl.SwapBuffers();
        }

        private void EnableShaders()
        {
            int vertex_shader;
            int fragment_shader;
            int program;

            // get source code
            string vertex_shader_source = Sample1_Triangle.Properties.Resources.basic_vert;
            string fragment_shader_source = Sample1_Triangle.Properties.Resources.basic_frag;

            // create shader objects
            vertex_shader = GL.CreateShader(ShaderType.VertexShader);
            if (vertex_shader == 0) throw new Exception("Error creating vertex shader.");
            fragment_shader = GL.CreateShader(ShaderType.FragmentShader);
            if (fragment_shader == 0) throw new Exception("Error creating fragment shader.");

            // copy the source code
            GL.ShaderSource(vertex_shader, vertex_shader_source);
            GL.ShaderSource(fragment_shader, fragment_shader_source);

            // compile shaders
            GL.CompileShader(vertex_shader);
            GL.CompileShader(fragment_shader);

            // verify the compilation status
            int result;
            string log;
            GL.GetShader(vertex_shader, ShaderParameter.CompileStatus, out result);
            if (result == 0)
            {
                log = GL.GetShaderInfoLog(vertex_shader);
                throw new Exception("Vertex shader compilation failed!\nVertex shader log: " + log);

            }
            GL.GetShader(fragment_shader, ShaderParameter.CompileStatus, out result);
            if (result == 0) 
            {
                log = GL.GetShaderInfoLog(fragment_shader);
                throw new Exception("Fragment shader compilation failed!\nFragment shader log: " + log); 
            }

            // create the program object
            program = GL.CreateProgram();
            if (program == 0) throw new Exception("Error creating program object.");

            // attach the shaders
            GL.AttachShader(program, vertex_shader);
            GL.AttachShader(program, fragment_shader);

            // create the data objects
            float[] positionData = { 
                                   -0.8f,   -0.8f,  0.0f,
                                   0.8f,    -0.8f,  0.0f,
                                   0.0f,    0.8f,   0.0f    };
            float[] colorData = {
                                    1.0f,   0.0f,   0.0f,
                                    0.0f,   1.0f,   0.0f,
                                    0.0f,   0.0f,   1.0f    };

            // create the buffer objects
            uint[] vboHandles = new uint[2];
            GL.GenBuffers(2, vboHandles);
            uint positionBufferHandle = vboHandles[0];
            uint colorBufferHandle = vboHandles[1];

            // populate the position buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(9 * sizeof(float)),
                positionData, BufferUsageHint.StaticDraw);
            // populate the color buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(9 * sizeof(float)),
                colorData, BufferUsageHint.StaticDraw);

            // create and set-up the vertex array object
            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            // enable the vertex attribute arrays
            GL.EnableVertexAttribArray(0); // vertex position
            GL.EnableVertexAttribArray(1); // vertex color

            // map index 0 to the position buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            // map index 1 to the color buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBufferHandle);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            // link the program
            GL.LinkProgram(program);

            // verify the link status
            int status;
            GL.GetProgram(program, ProgramParameter.LinkStatus, out status);
            if (status == 0)
            {
                log = GL.GetProgramInfoLog(program);
                throw new Exception("Failed to link shader program!\nProgram log: " + log);
            }
            else GL.UseProgram(program);
        }
    }
}
