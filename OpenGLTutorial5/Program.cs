using System;
using Tao.FreeGlut;
using OpenGL;
using System.Collections.Generic;
using System.Linq;

namespace OpenGLTutorial5
{
    class Program
    {
        private static int width = 1280, height = 600;
        private static ShaderProgram program;
        private static VBO<Vector3> pyramid, cube, sphere;
        private static VBO<Vector3> pyramidColor, cubeColor, sphereColor;
        private static VBO<uint> pyramidTriangles, cubeQuads, sphereTriangles;
        private static System.Diagnostics.Stopwatch watch;
        private static float angle;
        static double lheight = Math.Sqrt(3) / 3;
        static List<Vector3> points = new List<Vector3>();
        static List<Vector3> points1 = new List<Vector3>();
        static List<Vector3> normals = new List<Vector3>();
        static List<Vector2> texCoordinates = new List<Vector2>();


        static void GetPoints()
        {
            points.Clear();
            normals.Clear();
            texCoordinates.Clear();

            var radius=2.0f;
            var sectorCount=30;
            var stackCount=30;
            var smooth=false;
            //std::vector<float>().swap(vertices);
            //std::vector<float>().swap(normals);
            //std::vector<float>().swap(texCoords);
            
            float x, y, z, xy;                              // vertex position
            float nx, ny, nz, lengthInv = 1.0f / radius;    // vertex normal
            float s, t;                                     // vertex texCoord

            float sectorStep = (float)(2 * Math.PI / sectorCount);
            float stackStep = (float)Math.PI / stackCount;
            float sectorAngle, stackAngle;

            
            for (int i = 0; i <= stackCount; ++i)
            {
                stackAngle = (float)Math.PI / 2 - i * stackStep;        // starting from pi/2 to -pi/2
                xy = radius * (float)Math.Cos(stackAngle);             // r * cos(u)
                z = radius * (float)Math.Sin(stackAngle);              // r * sin(u)

                // add (sectorCount+1) vertices per stack
                // the first and last vertices have same position and normal, but different tex coords
                for (int j = 0; j <= sectorCount; ++j)
                {
                    sectorAngle = j * sectorStep;           // starting from 0 to 2pi

                    // vertex position (x, y, z)
                    x = xy * (float)Math.Cos(sectorAngle);             // r * cos(u) * cos(v)
                    y = xy * (float)Math.Sin(sectorAngle);             // r * cos(u) * sin(v)
                    if (Math.Abs(z) < 0.000001)
                    {
                        z = 0;
                    }
                    if (Math.Abs(x) < 0.000001)
                    {
                        x = 0;
                    }
                    if (Math.Abs(y) < 0.000001)
                    {
                        y = 0;
                    }
                    points.Add(new Vector3(x,y,z));
                    points=points.Distinct().ToList();
                    
                    // normalized vertex normal (nx, ny, nz)
                    nx = x * lengthInv;
                    ny = y * lengthInv;
                    nz = z * lengthInv;
                    normals.Add(new Vector3(nx,ny,nz));
                    

                    // vertex tex coord (s, t) range between [0, 1]
                    s = (float)j / sectorCount;
                    t = (float)i / stackCount;
                    texCoordinates.Add(new Vector2(s,t));
                  
                }
            }

            var pointArray = points.ToArray();
            int pp = 1;
            for (int i = 0; i < stackCount; ++i)
            {
                int p = 0;
                if (i == 0)
                {
                    points1.AddRange(new[] { pointArray[0], pointArray[0], pointArray[sectorCount], pointArray[1] });
                    for (int k = 1; k < sectorCount; k++)
                    {
                        points1.AddRange(new[]{pointArray[0],pointArray[0],pointArray[k],pointArray[k+1]});
                    }
                }
                else if (i == stackCount-1)
                {
                    points1.AddRange(new[] { pointArray[(i) * sectorCount], pointArray[(i - 1) * sectorCount + 1], pointArray[pointArray.Length-1], pointArray[pointArray.Length - 1] });
                    for (int j = 1; j < sectorCount; ++j)
                    {
                        points1.AddRange(new[] { pointArray[(i - 1) * sectorCount + j], pointArray[(i - 1) * sectorCount + j + 1], pointArray[pointArray.Length - 1], pointArray[pointArray.Length - 1] });

                    }
                }
                else
                {
                    points1.AddRange(new[] { pointArray[(i) * sectorCount ], pointArray[(i-1) * sectorCount+1 ], pointArray[(i) * sectorCount + 1], pointArray[(i+1) * sectorCount ] });
                    for (int j = 1; j < sectorCount; ++j)
                    {
                        points1.AddRange(new[] { pointArray[(i-1) * sectorCount + j], pointArray[(i - 1) * sectorCount + j + 1], pointArray[(i) * sectorCount + j + 1], pointArray[(i) * sectorCount + j] });

                    }
                }
            }
        }



        

        static void Main(string[] args)
        {
            // create an OpenGL window
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("OpenGL Tutorial");

            // provide the Glut callbacks that are necessary for running this tutorial
            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);

            Glut.glutKeyboardFunc(OnKeyboardDown);
            Glut.glutKeyboardUpFunc(OnKeyboardUp);

            Glut.glutCloseFunc(OnClose);



            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);

            // compile the shader program
            program = new ShaderProgram(VertexShader, FragmentShader);

            // set the view and projection matrix, which are static throughout this tutorial
            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, new Vector3(0, 1, 0)));


            var p1 = new Vector3(0, 0, Math.Sqrt(6) / 3);
            var p2 = new Vector3(0, lheight, 0.0);
            var p3 = new Vector3(Math.Sin(Math.PI / 3) * lheight, -Math.Cos(Math.PI / 3) * lheight, 0.0);
            var p4 = new Vector3(-Math.Sin(Math.PI / 3) * lheight, -Math.Cos(Math.PI / 3) * lheight, 0.0);

            // create a pyramid with vertices and colors
            pyramid = new VBO<Vector3>(new Vector3[] {
                p1,p2,p3,
                p1,p2,p4,
                p2,p3,p4,
                p1,p3,p4
                });   // left face
            pyramidColor = new VBO<Vector3>(new Vector3[] {
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(0, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 1),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0) });
            pyramidTriangles = new VBO<uint>(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, BufferTarget.ElementArrayBuffer);

            // create a cube with vertices and colors
            cube = new VBO<Vector3>(new Vector3[] {
                new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1),
                new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1),
                new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),
                new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1),
                new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
                new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1) });
            cubeColor = new VBO<Vector3>(new Vector3[] {
                new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), 
                new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), 
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), 
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), 
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) });
            cubeQuads = new VBO<uint>(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, BufferTarget.ElementArrayBuffer);

            watch = System.Diagnostics.Stopwatch.StartNew();

            GetPoints();

            //sphere = new VBO<Vector3>(points.ToArray());   // left face
            //sphereColor = new VBO<Vector3>(points.Select(t=>new Vector3(1,0,0)).ToArray());
            //sphereTriangles = new VBO<uint>(Enumerable.Range(0, points.Count * 3).Select(t => (uint)t).ToArray(), BufferTarget.ElementArrayBuffer);
            int a =0, b = 60000;
            //a *= 4;
            b *= 4;

            //points= new Vector3[] {
            //    new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1),
            //    new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1),
            //    new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),
            //    new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1),
            //    new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
            //    new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1) }.ToList();


            sphere = new VBO<Vector3>(points1.Skip(a).Take(b).ToArray());
            var colors=new List<Vector3>();
            for (int i = 0; i < points1.Count; i+=4)
            {
                var v=new Vector3(Math.Abs(points1[i].X+ points1[i+1].X + points1[i+2].X + points1[i+3].X )/4,
                    Math.Abs((points1[i].Y + points1[i + 1].Y+ points1[i + 2].Y + points1[i + 3].Y) / 4),
                        Math.Abs((points1[i].Z + points1[i + 1].Z + points1[i + 2].Z + points1[i + 3].Z) / 4));
                colors.AddRange(new []{v,v,v,v});
            }
            sphereColor = new VBO<Vector3>(colors.Skip(a).Take(b).ToArray());
            sphereTriangles = new VBO<uint>(Enumerable.Range(0, points1.Skip(a).Take(b).Count() * 3).Select(t => (uint)t).ToArray(), BufferTarget.ElementArrayBuffer);



            Glut.glutMainLoop();
        }
        public static void glDraw()
        {
            //GL.gluSphere(this.quadratic, 1.3f, 32, 32);
            Glut.glutSolidSphere(10,50,50);
        }
        private static void OnClose()
        {
            // dispose of all of the resources that were created
            pyramid.Dispose();
            pyramidColor.Dispose();
            pyramidTriangles.Dispose();
            cubeColor.Dispose();
            cubeQuads.Dispose();
            program.DisposeChildren = true;
            program.Dispose();
            sphere.Dispose();
            sphereColor.Dispose();
            sphereTriangles.Dispose();
        }

        private static void OnDisplay()
        {

        }


        private static bool left, right, up, down;
        private static void OnKeyboardDown(byte key, int x, int y)
        {
            if (key == 'w') up = true;
            else if (key == 's') down = true;
            else if (key == 'd') right = true;
            else if (key == 'a') left = true;
            else if (key == 27) Glut.glutLeaveMainLoop();
        }


        private static void OnKeyboardUp(byte key, int x, int y)
        {
            if (key == 'w') up = false;
            else if (key == 's') down = false;
            else if (key == 'd') right = false;
            else if (key == 'a') left = false;
            //else if (key == ' ') autoRotate = !autoRotate;
            //else if (key == 'l') lighting = !lighting;
            //else if (key == 'f')
            //{
            //    fullscreen = !fullscreen;
            //    if (fullscreen) Glut.glutFullScreen();
            //    else
            //    {
            //        Glut.glutPositionWindow(0, 0);
            //        Glut.glutReshapeWindow(1280, 720);
            //    }
            //}
        }
        private static void OnRenderFrame()
        {

           
            // calculate how much time has elapsed since the last frame
            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();

            if (right) yangle += deltaTime;
            if (left) yangle -= deltaTime;
            if (up) xangle -= deltaTime;
            if (down) xangle += deltaTime;


            // use the deltaTime to adjust the angle of the cube and pyramid
            angle += deltaTime;

            // set up the OpenGL viewport and clear both the color and depth bits
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // use our shader program
            Gl.UseProgram(program);
            GC.Collect();






            program["model_matrix"].SetValue(Matrix4.CreateRotationY(yangle) * Matrix4.CreateRotationX(xangle) * Matrix4.CreateTranslation(new Vector3(-1.5f, 0, 0)));
            Gl.BindBufferToShaderAttribute(sphere, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(sphereColor, program, "vertexColor");
            Gl.BindBuffer(sphereTriangles);
            Gl.DrawElements(BeginMode.Quads, sphereTriangles.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);




            //program["model_matrix"].SetValue(Matrix4.CreateRotationY(yangle) * Matrix4.CreateRotationX(xangle) * Matrix4.CreateTranslation(new Vector3(-1.5f, 0, 0)));
            //Gl.BindBufferToShaderAttribute(pyramid, program, "vertexPosition");
            //Gl.BindBufferToShaderAttribute(pyramidColor, program, "vertexColor");
            //Gl.BindBuffer(pyramidTriangles);
            //Gl.DrawElements(BeginMode.Triangles, pyramidTriangles.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            //program["model_matrix"].SetValue(Matrix4.CreateRotationY(yangle) * Matrix4.CreateRotationX(xangle) * Matrix4.CreateTranslation(new Vector3(1.5f, 0, 0)));
            //Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
            //Gl.BindBufferToShaderAttribute(cubeColor, program, "vertexColor");
            //Gl.BindBuffer(cubeQuads);
            //Gl.DrawElements(BeginMode.Quads, cubeQuads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            Glut.glutSwapBuffers();
        }

        public static string VertexShader = @"
#version 130

in vec3 vertexPosition;
in vec3 vertexColor;

out vec3 color;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main(void)
{
    color = vertexColor;
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";

        public static string FragmentShader = @"
#version 130

in vec3 color;

out vec4 fragment;

void main(void)
{
    fragment = vec4(color, 1);
}
";

        private static float yangle;
        private static float xangle;
    }
}
