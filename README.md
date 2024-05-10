# PT3PlayBlazor

This is a Pro Tracker 3.x format chip tune music player ported to C#. It plays [AY-3-8910](https://en.wikipedia.org/wiki/General_Instrument_AY-3-8910) chip tune music found on the ZX Spectrum.

## Demo

This demo runs in Blazor and uses the [Blazor.WebAudio](https://github.com/KristofferStrube/Blazor.WebAudio) library by [Kristoffer Strube](https://github.com/KristofferStrube). I want to extend my gratitude to Kirstoffer for his work on Blazor.WebAudio and specifically adding the AudioWorklet APIs required for this demo to render audio in real-time.

[Live Demo](https://baker76.com/apps/pt3play/)

## Controls

Use the buttons in the browser to control the music and SFX.

## Screenshot

![](/images/pt3play.png)

## License

The source code is released under the MIT license.

## Credits

Special thanks to the following individuals and projects for their contributions:

- The original PT3Play and AY Emulator code was written by [Sergey Bulba](mailto:svbulba@gmail.com) ([link](https://bulba.untergrund.net/vortex_e.htm)) and contains modified code from [ayfly](https://github.com/l29ah/ayfly).
- The AY FX player code is based on [AYFX Editor v0.6](https://shiru.untergrund.net/software.shtml) by [Shiru](mailto:shiru@mail.ru).
- The spectrum analyzer code is from the [ESPboy_PT3Play](https://github.com/ESPboy-edu/ESPboy_PT3Play) project by Shiru
- The demo music is by Shiru ([link](https://shiru.untergrund.net/software.shtml)).
- Also thanks to authors of the sound effects that are included in the library.
- [Blazor.WebAudio](https://github.com/KristofferStrube/Blazor.WebAudio) by [Kristoffer Strube](https://github.com/KristofferStrube)
