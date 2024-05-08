using Blazor.Extensions;
using Blazor.Extensions.Canvas.WebGL;
using KristofferStrube.Blazor.WebAudio;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Drawing;

namespace PT3PlayBlazor
{
    public partial class WebGLComponent : ComponentBase, IAsyncDisposable
    {
        private WebGLContext _context;

        protected BECanvasComponent _canvasReference;

        protected PullAudioWorkletProcessor.Resolution _resolution = PullAudioWorkletProcessor.Resolution.Byte;

        private System.Timers.Timer _timer;

        [Inject]
        private HttpClient HttpClient { get; set; }

        [Inject]
        private IJSInProcessRuntime JsRuntime { get; set; }

        private int _canvasWidth;
        private int _canvasHeight;

        private WebGLBuffer _vertexBuffer;

        private WebGLProgram _specProgram;

        AudioContext? _audioContext;
        AudioDestinationNode? _destination;
        GainNode? _gainNode;
        PullAudioWorkletProcessor? _processor;

        private int _currentSong = 0;
        private int _currentSfx = 0;

        private string[] _songArray =
        {
            "music/199Xnostalgy.pt3",
            "music/autumn_colors.pt3",
            "music/a_little_journey.pt3",
            "music/buhanidvebatonki.pt3",
            "music/chinesewatch.pt3",
            "music/durkadablues.pt3",
            "music/enchanted_woods.pt3",
            "music/hard.pt3",
            "music/itscomefromthedark.pt3",
            "music/kakvsegda.pt3",
            "music/megamix.pt3",
            "music/mehalanholia.pt3",
            "music/moonlight.pt3",
            "music/oldlove.pt3",
            "music/proper_summer.pt3",
            "music/p_pp.pt3",
            "music/snowball_game.pt3",
            "music/spring_came.pt3",
            "music/summer.pt3",
            "music/timeup.pt3",
            "music/under_the_sun.pt3",
            "music/vozhidaniitepla.pt3"
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this._context = await this._canvasReference.CreateWebGLAsync(new WebGLContextAttributes
                {
                    PowerPreference = WebGLContextAttributes.POWER_PREFERENCE_HIGH_PERFORMANCE
                });

                _canvasWidth = (int)_canvasReference.Width;
                _canvasHeight = (int)_canvasReference.Height;

                await WebGL.Initialize(this._context, _canvasWidth, _canvasHeight);

                _specProgram = await WebGL.LoadShader(HttpClient, "shaders/spec.vs", "shaders/spec.fs");

                _vertexBuffer = await this._context.CreateBufferAsync();
                await this._context.BindBufferAsync(BufferType.ARRAY_BUFFER, _vertexBuffer);

                //byte[] musicBytes = await HttpClient.GetByteArrayAsync(_songArray[0]);
                //PT3Play.MusicPlay(ref musicBytes);

                byte[] sfxBytes = await HttpClient.GetByteArrayAsync("sfx/streetsofrage_2.afb");
                AYFx.SFX_BankLoad(sfxBytes);

                _timer = new System.Timers.Timer(1000.0 / PT3Play.FRAME_RATE);
                _timer.Elapsed += async (sender, e) => await UpdateAndRender();
                _timer.Start();
            }
        }

        public async Task InitAudioContext()
        {
            if (_audioContext != null)
                return;

            AudioContextOptions audioContextOptions = new()
            {
                SampleRate = PT3Play.SAMPLE_RATE,
                LatencyHint = AudioContextLatencyCategory.Interactive
            };

            // Initialize audio context.
            _audioContext = await AudioContext.CreateAsync(JsRuntime, audioContextOptions);

            // Create and register node.
            _processor = await PullAudioWorkletProcessor.CreateAsync(_audioContext, new()
            {
                Resolution = _resolution,
                LowTide = 10,
                HighTide = 50,
                BufferRequestSize = 10,
                ProduceStereo = () =>
                {
                    short music_l, music_r;
                    short sfx_l, sfx_r;

                    PT3Play.EmulateSample(out music_l, out music_r);
                    AYFx.EmulateSample(out sfx_l, out sfx_r);

                    int sample_l = (music_l + sfx_l) / 2;
                    int sample_r = (music_r + sfx_r) / 2;

                    if (sample_l > 32767)
                        sample_l = 32767;
                    if (sample_r > 32767)
                        sample_r = 32767;

                    double out_l = ((double)sample_l - 16384) / 16384.0;
                    double out_r = ((double)sample_r - 16384) / 16384.0;

                    return (out_l, out_r);
                }
            });

            _destination = await _audioContext.GetDestinationAsync();
            _gainNode = await GainNode.CreateAsync(JsRuntime, _audioContext, new() { Gain = 1.0f });
            await _processor.Node.ConnectAsync(_gainNode);
            await _gainNode.ConnectAsync(_destination);
        }

        private async Task UpdateAndRender()
        {
            await this._context.BeginBatchAsync();

            await this._context.ClearColorAsync(0, 0, 0, 1);
            await this._context.ClearAsync(BufferBits.COLOR_BUFFER_BIT);

            await this._context.EnableAsync(EnableCap.BLEND);
            await this._context.BlendFuncAsync(BlendingMode.SRC_ALPHA, BlendingMode.ONE_MINUS_SRC_ALPHA);

            await WebGL.RenderBars(_specProgram, PT3Play.spec_levels, PT3Play.spec_colors, new Size(_canvasWidth, _canvasHeight));

            await this._context.EndBatchAsync();

            //StateHasChanged();
        }

        public async void PlaySong()
        {
            await InitAudioContext();

            byte[] musicBytes = await HttpClient.GetByteArrayAsync(_songArray[_currentSong]);
            PT3Play.MusicPlay(ref musicBytes);
        }

        public async void PreviousSong()
        {
            if (--_currentSong < 0)
                _currentSong = _songArray.Length - 1;

            byte[] musicBytes = await HttpClient.GetByteArrayAsync(_songArray[_currentSong]);
            PT3Play.MusicPlay(ref musicBytes);
        }

        public async void NextSong()
        {
            if (++_currentSong >= _songArray.Length)
                _currentSong = 0;

            byte[] musicBytes = await HttpClient.GetByteArrayAsync(_songArray[_currentSong]);
            PT3Play.MusicPlay(ref musicBytes);
        }

        public void PreviousSFX()
        {
            if (--_currentSfx < 0)
                _currentSfx = AYFx.AllEffect - 1;

            AYFx.SFX_Play(_currentSfx);
        }

        public void NextSFX()
        {
            if (++_currentSfx >= AYFx.AllEffect)
                _currentSfx = 0;

            AYFx.SFX_Play(_currentSfx);
        }

        public async void PlaySFX()
        {
            await InitAudioContext();

            AYFx.SFX_Play(_currentSfx);
        }

        public int CanvasWidth { get { return _canvasWidth; } }
        public int CanvasHeight { get { return _canvasHeight; } }

        public bool GridOn { get; set; }

        public async ValueTask DisposeAsync()
        {
            _timer.Stop();
            _timer.Dispose();

            if (_audioContext is not null)
                await _audioContext.DisposeAsync();

            if (_gainNode is not null)
                await _gainNode.DisposeAsync();

            if (_destination is not null)
                await _destination.DisposeAsync();

            if (_processor is not null)
                await _processor.DisposeAsync();
        }
    }
}
