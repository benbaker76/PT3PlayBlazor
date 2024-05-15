using Blazor.Extensions.Canvas.WebGL;
using System.Drawing;

namespace PT3PlayBlazor
{
    public class WebGL
    {
        public static async Task Initialize(WebGLContext context, int viewportWidth, int viewportHeight)
        {
            Context = context;

            await Context.ViewportAsync(0, 0, viewportWidth, viewportHeight);

            ProjMatrix = Mat4.Create();
            Mat4.Ortho(ProjMatrix, 0, viewportWidth, viewportHeight, 0, -1, 1);

            float[] cam = Mat4.Create();
            float[] position = new float[] { 0, 0, 0 };
            Mat4.Translate(cam, cam, position);

            ModelViewMatrix = Mat4.CloneIt(cam);
        }

        public static async Task<WebGLProgram> LoadShader(HttpClient httpClient, string vsFileName, string fsFileName)
        {
            string vsSource = await httpClient.GetStringAsync(vsFileName);
            string fsSource = await httpClient.GetStringAsync(fsFileName);

            WebGLProgram program = await InitProgramAsync(vsSource, fsSource);

            await Context.UseProgramAsync(program);

            var uProjMatrixLocation = await Context.GetUniformLocationAsync(program, "uProjMatrix");
            var uModelViewMatrixLocation = await Context.GetUniformLocationAsync(program, "uModelViewMatrix");

            await Context.UniformMatrixAsync(uProjMatrixLocation, false, ProjMatrix);
            await Context.UniformMatrixAsync(uModelViewMatrixLocation, false, ModelViewMatrix);

            return program;
        }

        public static async Task<WebGLProgram> InitProgramAsync(string vsSource, string fsSource)
        {
            var vertexShader = await LoadShaderAsync(ShaderType.VERTEX_SHADER, vsSource);
            var fragmentShader = await LoadShaderAsync(ShaderType.FRAGMENT_SHADER, fsSource);

            var program = await Context.CreateProgramAsync();
            await Context.AttachShaderAsync(program, vertexShader);
            await Context.AttachShaderAsync(program, fragmentShader);
            await Context.LinkProgramAsync(program);

            await Context.DeleteShaderAsync(vertexShader);
            await Context.DeleteShaderAsync(fragmentShader);

            if (!await Context.GetProgramParameterAsync<bool>(program, ProgramParameter.LINK_STATUS))
            {
                string info = await Context.GetProgramInfoLogAsync(program);
                throw new Exception("An error occurred while linking the program: " + info);
            }

            return program;
        }

        public static async Task<WebGLShader> LoadShaderAsync(ShaderType type, string source)
        {
            var shader = await Context.CreateShaderAsync(type);

            await Context.ShaderSourceAsync(shader, source);
            await Context.CompileShaderAsync(shader);

            if (!await Context.GetShaderParameterAsync<bool>(shader, ShaderParameter.COMPILE_STATUS))
            {
                string info = await Context.GetShaderInfoLogAsync(shader);
                await Context.DeleteShaderAsync(shader);
                throw new Exception("An error occurred while compiling the shader: " + info);
            }

            return shader;
        }

        public static async Task RenderBars(WebGLProgram program, short[] specLevels, uint[] specColors, Size screenSize)
        {
            await Context.UseProgramAsync(program);

            var uColorLocation = await Context.GetUniformLocationAsync(program, "uColor");

            float barWidth = (float)screenSize.Width / specLevels.Length;
            float barHeight = (float)screenSize.Height;

            for (int c = 0; c < specColors.Length; c++)
            {
                var color = specColors[c];
                
                List<float> vertices = new List<float>();

                for (int i = 0; i < specLevels.Length; i++)
                {
                    if (specColors[i] == color)
                    {
                        float x1 = i * barWidth;
                        float x2 = (i + 1) * barWidth;
                        float y1 = barHeight - ((float)specLevels[i] / PT3Play.SPEC_HEIGHT) * barHeight;
                        float y2 = barHeight;

                        vertices.AddRange(new float[]
                        {
                            x1, y1, 0.0f,
                            x2, y1, 0.0f,
                            x2, y2, 0.0f
                        });

                        vertices.AddRange(new float[]
                        {
                            x1, y1, 0.0f,
                            x2, y2, 0.0f,
                            x1, y2, 0.0f
                        });
                    }
                }

                await Context.UniformAsync(uColorLocation, new float[]
                {
                    ((color >> 16) & 0xFF) / 255.0f,
                    ((color >> 8) & 0xFF) / 255.0f,
                    (color & 0xFF) / 255.0f,
                    1.0f
                });

                await Context.BufferDataAsync(BufferType.ARRAY_BUFFER, vertices.ToArray(), BufferUsageHint.STATIC_DRAW);
                await Context.VertexAttribPointerAsync(0, 3, DataType.FLOAT, false, 0, 0);
                await Context.EnableVertexAttribArrayAsync(0);
                await Context.DrawArraysAsync(Primitive.TRIANGLES, 0, vertices.Count / 3);
            }
        }

        public static WebGLContext Context { get; set; }
        public static float[] ProjMatrix { get; set; }
        public static float[] ModelViewMatrix { get; set; }
    }
}
