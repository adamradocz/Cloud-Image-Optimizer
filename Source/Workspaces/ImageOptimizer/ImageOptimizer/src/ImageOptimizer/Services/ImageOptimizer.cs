using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ImageOptimizer.Models;

namespace ImageOptimizer.Services
{
    public class ImageOptimizer
    {
        private const int DefaultJpgQuality = 75;
        private const int DefaultWebPQuality = 75;
        private const int DefaultJxrQuality = 40;
        private const int DefaultJp2Quality = 40;
        private const string PluginFolder = "plugins";
        private const string JpgFolder = "jpg";
        private const string PngFolder = "png";
        private const string GifFolder = "gif";
        private const string WebPFolder = "webp";
        private const string JxrFolder = "jxr";
        private const string ApngFolder = "apng";
        private const string OthersFolder = "others";
        
        private readonly string _pluginFolderPath;
        private string _imageFolderPath;
        
        public ImageOptimizer(IHostingEnvironment hostingEnvironment)
        {
            _pluginFolderPath = Path.Combine(hostingEnvironment.WebRootPath, PluginFolder);
        }

        public void OptimizeImage(ImageServiceModel image)
        {
            _imageFolderPath = Path.GetDirectoryName(image.FilePath);
            var optimizationStart = DateTime.Now;

            if (string.Equals("image/jpeg", image.FileType, StringComparison.OrdinalIgnoreCase))
            {
                OptimizeJpg(image, DefaultJpgQuality);
            }
            else if (string.Equals("image/png", image.FileType, StringComparison.OrdinalIgnoreCase))
            {
                OptimizePng(image);
            }
            else if (string.Equals("image/gif", image.FileType, StringComparison.OrdinalIgnoreCase))
            {
                OptimizeGif(image);
            }
            else if (string.Equals("image/webp", image.FileType, StringComparison.OrdinalIgnoreCase))
            {
                OptimizeWebP(image, DefaultWebPQuality);
            }
            else if (string.Equals("image/vnd.ms-photo", image.FileType, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals("image/jxr", image.FileType, StringComparison.OrdinalIgnoreCase))
            {
                OptimizeJxr(image, DefaultJxrQuality);
            }
            
            image.OptimizedSize = new FileInfo(image.FilePath).Length;
            image.OptimizationTime = DateTime.Now - optimizationStart;

            if (image.IsConvert)
            {
                ConvertImage(image);
            }
        }

        private void ConvertImage(ImageServiceModel image, bool forceConvert = false)
        {
            var convertedImages = new List<ConvertedImageServiceModel>();

            if (string.Equals("image/jpeg", image.FileType, StringComparison.OrdinalIgnoreCase))
            {
                // Always convert lossy file types as lossy
                image.IsLossless = false; 

                // Convert Jpg to WebP
                var optimizationStart = DateTime.Now;
                var convertedImagePath = ConvertToWebP(image, DefaultWebPQuality);
                var convertedImage = new ConvertedImageServiceModel(convertedImagePath, image)
                {
                    OptimizationTime = DateTime.Now - optimizationStart,
                    FileType = "image/webp"
                };
                HandleConvertedImage(image, forceConvert, convertedImage, convertedImages);

                // Convert Jpg to Jxr
                optimizationStart = DateTime.Now;
                convertedImagePath = ConvertToJxr(image, DefaultJxrQuality);
                convertedImage = new ConvertedImageServiceModel(convertedImagePath, image)
                {
                    OptimizationTime = DateTime.Now - optimizationStart,
                    FileType = "image/vnd.ms-photo"
                };
                HandleConvertedImage(image, forceConvert, convertedImage, convertedImages);
            }
            else if (string.Equals("image/png", image.FileType, StringComparison.OrdinalIgnoreCase))
            {
                // Convert Png to WebP
                var optimizationStart = DateTime.Now;
                var convertedImagePath = ConvertToWebP(image, DefaultWebPQuality);
                var convertedImage = new ConvertedImageServiceModel(convertedImagePath, image)
                {
                    OptimizationTime = DateTime.Now - optimizationStart,
                    FileType = "image/webp"
                };
                HandleConvertedImage(image, forceConvert, convertedImage, convertedImages);

                // Convert Png to Jxr
                optimizationStart = DateTime.Now;
                convertedImagePath = ConvertToJxr(image, DefaultJxrQuality);
                convertedImage = new ConvertedImageServiceModel(convertedImagePath, image)
                {
                    OptimizationTime = DateTime.Now - optimizationStart,
                    FileType = "image/jxr"
                };
                HandleConvertedImage(image, forceConvert, convertedImage, convertedImages);
            }
            else if (string.Equals("image/gif", image.FileType, StringComparison.OrdinalIgnoreCase))
            {
                // Convert Gif to WebP
                var optimizationStart = DateTime.Now;
                var convertedImagePath = ConvertGifToWebP(image);
                var convertedImage = new ConvertedImageServiceModel(convertedImagePath, image)
                {
                    OptimizationTime = DateTime.Now - optimizationStart,
                    FileType = "image/webp"
                };
                HandleConvertedImage(image, forceConvert, convertedImage, convertedImages);
                
                // Convert Gif to Apng
                optimizationStart = DateTime.Now;
                convertedImagePath = ConvertGifToApng(image);
                convertedImage = new ConvertedImageServiceModel(convertedImagePath, image)
                {
                    OptimizationTime = DateTime.Now - optimizationStart,
                    FileType = "image/apng"
                };
                HandleConvertedImage(image, forceConvert, convertedImage, convertedImages);
            }

            if (convertedImages.Count > 0)
                image.ConvertedImages = convertedImages;
        }

        #region Jpg

        private void OptimizeJpg(ImageServiceModel image, int quality)
        {
            if (!image.IsLossless)
            {
                JpegOptim(image, quality);
            }

            JpegTranMozjpeg(image);

            if (image.OptimizationLevel >= (int) OptimizationLevel.High)
            {
                JpegStripper(image);
            }
        }

        #region Jpg helpers
        // -s, --strip-all   strip all markers from output file
        // --strip-none      do not strip any markers
        // -o, --overwrite   overwrite target file even if it exists
        // -q, --quiet       quiet mode
        // -m<quality>, --max=<quality> set maximum image quality factor
        //                   (disables lossless optimization mode, which is by default on)
        //                   Valid quality values: 0 - 100
        // --all-normal      force all output files to be non-progressive
        // --all-progressive force all output files to be progressive
        private void JpegOptim(ImageServiceModel image, int quality)
        {
            var tempImagePath = CreateTempImage(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, JpgFolder, "jpegoptim.exe");
            string arguments;

            if (image.OptimizationLevel < (int)OptimizationLevel.Insane)
            {
                arguments = image.IsLossless
                    ? $"-s -o -q \"{tempImagePath}\""
                    : $"-s --max={quality} -o -q \"{tempImagePath}\"";
                Optimize(image.FilePath, tempImagePath, processPath, arguments);
            }
            else
            {
                // all-normal
                arguments = image.IsLossless
                    ? $"-s -o -q --all-normal \"{tempImagePath}\""
                    : $"-s --max={quality} -o -q --all-normal \"{tempImagePath}\"";
                Optimize(image.FilePath, tempImagePath, processPath, arguments);

                // all-progressive
                arguments = image.IsLossless
                    ? $"-s -o -q --all-progressive \"{tempImagePath}\""
                    : $"-s --max={quality} -o -q --all-progressive \"{tempImagePath}\"";
                Optimize(image.FilePath, tempImagePath, processPath, arguments);
            }
        }

        // -copy none     Copy no extra markers from source file
        // -copy all      Copy all extra markers
        // -optimize      Optimize Huffman table (smaller file, but slow compression, enabled by default)
        // -progressive   Create progressive JPEG file (enabled by default)
        // -fastcrush     Disable progressive scan optimization
        // -outfile name  Specify name for output file
        private void JpegTranMozjpeg(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImagePath(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, JpgFolder, "jpegtran-mozjpeg.exe");
            var arguments = $"-copy none -optimize -outfile \"{tempImagePath}\" \"{image.FilePath}\"";
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }

        // -q      Be quiet
        // -y      Forced writing APP14 YCCK marker to CMYK JPEG file
        private void JpegStripper(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImage(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, JpgFolder, "jpegstripper.exe");
            var arguments = $"-y -q \"{tempImagePath}\"";
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }
        #endregion

        #endregion

        #region Png

        // Optimization order:
        // 1. Color quantization (lossy) - PngQuant (paletted), TruePng(color quantization/lossy averaging filter)
        // 2. Reduction (lossless) - TruePng (better than OptiPng)
        // 3. Compression (lossless) - PngWolf-7-Zip, PngWolf-Zopfli, ZopfliPng
        // (7-Zip vs Zopfli -> Zopfil has 80x longer compression-time, but ~4-8% improvement in file size compared with 7-Zip)
        // 4. Deflate stream optimization (DeflOpt, Defluff)
        private void OptimizePng(ImageServiceModel image)
        {
            TruePng(image);
            if (image.OptimizationLevel == (int)OptimizationLevel.Normal)
            {
                PngWolf(image);
            }
            else if (image.OptimizationLevel >= (int)OptimizationLevel.High)
            {
                PngWolfZopfli(image);
            }
            else if (image.OptimizationLevel >= (int)OptimizationLevel.Best)
            {
                ZopfliPng(image);
            }
            DeflOpt(image);
            Defluff(image);
        }

        #region Png helpers

        // --force           overwrite existing output files (synonym: -f)
        // --skip-if-larger  only save converted files if they're smaller than original
        // --output file     destination file path to use instead of --ext (synonym: -o)
        // --speed N         speed/quality trade-off. 1=slow, 3=default, 11=fast & rough
        private void PngQuant(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImagePath(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, PngFolder, "pngquant.exe");
            var arguments = $"--speed 1 --skip-if-larger -f -o \"{tempImagePath}\" 256 \"{image.FilePath}\"";
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }

        // /y      confirm overwriting file
        // /quiet  quiet mode
        // /l      PNG lossy
        //              q=<quality> in range 0..255 (default q=4)
        //              m=<method> in range 0..1 (default m=0)
        //              for example: /l q = 10 m=1
        //
        // /o1 or /o fast = /zc9 /zm8-9 /zs0,1,3 /f0,5 (default)
        // /o2 or /o good = /zc9 /zm8-9 /zs0,1,3 /f0-5
        // /o3 or /o best = /zc9 /zm1-9 /zs0,1,3 /fe
        // /o4 or /o max  = /zc9 /zm1-9 /zs0,1,3 /fe /a1 /i0 /md remove all
        private void TruePng(ImageServiceModel image)
        {
            var optimizedImagePath = CreateTempImage(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, PngFolder, "TruePNG.exe");
            string arguments;

            if (image.OptimizationLevel == (int)OptimizationLevel.Low)
            {
                arguments = image.IsLossless
                    ? $"/o1 /quiet /y \"{optimizedImagePath}\""
                    : $"/o1 /l /quiet /y \"{optimizedImagePath}\"";
                Optimize(image.FilePath, optimizedImagePath, processPath, arguments);
            }
            else if (image.OptimizationLevel == (int)OptimizationLevel.Normal)
            {
                arguments = image.IsLossless
                    ? $"/o2 /quiet /y \"{optimizedImagePath}\""
                    : $"/o2 /l /quiet /y \"{optimizedImagePath}\"";
                Optimize(image.FilePath, optimizedImagePath, processPath, arguments);
            }
            else if (image.OptimizationLevel == (int)OptimizationLevel.High)
            {
                arguments = image.IsLossless
                    ? $"/o3 /quiet /y \"{optimizedImagePath}\""
                    : $"/o3 /l /quiet /y \"{optimizedImagePath}\"";
                Optimize(image.FilePath, optimizedImagePath, processPath, arguments);
            }
            else if (image.OptimizationLevel >= (int)OptimizationLevel.Best)
            {
                arguments = image.IsLossless
                    ? $"/o4 /quiet /y \"{optimizedImagePath}\""
                    : $"/o4 /l /quiet /y \"{optimizedImagePath}\"";
                Optimize(image.FilePath, optimizedImagePath, processPath, arguments);
            }
        }

        // --max-stagnate-time=<seconds>  Give up if no improvement is found (d: 5)
        // --7zip-mpass=<int|auto>        7zip passes 0..15 (d: 2; > ~ slower, smaller)
        // --in=<path.png>                The PNG input image
        // --out=<path.png>               The PNG output file(defaults to not saving!)
        private void PngWolf(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImagePath(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, PngFolder, "pngwolf.exe");
            var arguments = $"--in=\"{image.FilePath}\" --out=\"{tempImagePath}\"";
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }

        // --max-stagnate-time=<seconds>  Give up if no improvement is found (d: 5)
        // --in=<path.png>                The PNG input image
        // --out=<path.png>               The PNG output file(defaults to not saving!)
        private void PngWolfZopfli(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImagePath(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, PngFolder, "pngwolf-zopfli.exe");
            string arguments;
            if (image.OptimizationLevel <= (int)OptimizationLevel.High)
            {
                arguments = $"--in=\"{image.FilePath}\" --out=\"{tempImagePath}\"";
            }
            else // OptimizationLevel Best and Insane 
            {
                arguments = $"--max-stagnate-time=15 --in=\"{image.FilePath}\" --out=\"{tempImagePath}\"";
            }
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }

        // -m: compress more: use more iterations (depending on file size) and use block split strategy 3
        // -y: do not ask about overwriting files.
        private void ZopfliPng(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImagePath(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, PngFolder, "zopflipng.exe");
            var arguments = $"-m -y \"{image.FilePath}\" \"{tempImagePath}\"";
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }

        private void DeflOpt(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImage(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, PngFolder, "DeflOpt.exe");
            var arguments = $"-b -s \"{tempImagePath}\"";
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }

        private void Defluff(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImagePath(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, PngFolder, "defluff.bat");
            var defluffPath = Path.Combine(_pluginFolderPath, PngFolder, "defluff.exe");
            var arguments = $"{defluffPath} {image.FilePath} {tempImagePath}";
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }
        #endregion

        #endregion

        #region Gif

        private void OptimizeGif(ImageServiceModel image)
        {            
            Gifsicle(image);
        }

        // -w, --no-warnings                            Don't report warnings.
        // -o, --output FILE                            Write output to FILE.
        // --no-comments, --no-names, --no-extensions   Remove comments(names, extensions) from input.
        // -O, --optimize[=LEVEL]                       Optimize output GIFs.
        private void Gifsicle(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImagePath(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, GifFolder, "gifsicle.exe");
            var arguments = $"-w --no-comments --no-names --no-extensions --optimize=3 -o \"{tempImagePath}\" \"{image.FilePath}\"";
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }

        #endregion

        #region WebP

        // Convert PNG, JPEG, TIFF, GIF, BMP to WebP
        private string ConvertToWebP(ImageServiceModel image, int quality)
        {
            var outImageName = image.Name + ".webp";
            var outImagePath = Path.Combine(_imageFolderPath, outImageName);
            CWebP(image, outImagePath, quality);
            return outImagePath;
        }

        private void OptimizeWebP(ImageServiceModel image, int quality)
        {
            var tempImagePath = CreateTempImage(image.FilePath);
            CWebP(image, tempImagePath, quality);
            HandleOptimizedImage(image.FilePath, tempImagePath);
        }

        // -q <float> ............. quality factor (0:small..100:big)
        // -alpha_q <int> ......... transparency-compression quality (0..100)
        // -preset <string> ....... preset setting, one of:
        //                          default, photo, picture, drawing, icon, text
        //                          -preset must come first, as it overwrites other parameters
        // -z <int> ............... activates lossless preset with given
        //                          level in [0:fast, ..., 9:slowest]
        // -m <int> ............... compression method(0=fast, 6=slowest)
        // -size <int> ............ target size (in bytes)
        // -mt .................... use multi-threading if available
        // -alpha_filter <string> . predictive filtering for alpha plane,
        //                          one of: none, fast(default) or best
        // -lossless .............. encode image losslessly
        //                          The -q quality parameter will in this case control the amount of processing time
        //                          spent trying to make the output file as small as possible.
        // -metadata <string> ..... comma separated list of metadata to copy from the input to the output if present.
        //                          Valid values: all, none(default), exif, icc, xmp
        // -quiet ................. don't print anything
        private void CWebP(ImageServiceModel image, string outImagePath, int quality)
        {
            var processPath = Path.Combine(_pluginFolderPath, WebPFolder, "cwebp.exe");
            var arguments = image.IsLossless
                ? $"-m 6 -q 100 -alpha_q 100 -alpha_filter best -lossless -mt -quiet \"{image.FilePath}\" -o \"{outImagePath}\""
                : $"-m 6 -q {quality} -alpha_q {quality} -alpha_filter best -mt -quiet \"{image.FilePath}\" -o \"{outImagePath}\"";
            RunProcess(processPath, arguments);
        }

        private string ConvertGifToWebP(ImageServiceModel image)
        {
            var outImageName = image.Name + ".webp";
            var outImagePath = Path.Combine(_imageFolderPath, outImageName);
            Gif2WebP(image, outImagePath);
            return outImagePath;
        }

        // -lossy ................. encode image using lossy compression
        // -q <float> ............. quality factor (0:small..100:big)
        // -m <int> ............... compression method (0=fast, 6=slowest)
        // -metadata <string> ..... comma separated list of metadata to copy from the input to the output if present
        //                          Valid values: all, none, icc, xmp(default)
        // -mt .................... use multi-threading if available
        // -quiet ................. don't print anything
        private void Gif2WebP(ImageServiceModel image, string outImagePath)
        {
            var processPath = Path.Combine(_pluginFolderPath, WebPFolder, "gif2webp.exe");
            var arguments = image.IsLossless
                ? $"-m 6 -q 100 -metadata none -mt \"{image.FilePath}\" -o \"{outImagePath}\""
                : $"-m 6 -lossy -metadata none -mt \"{image.FilePath}\" -o \"{outImagePath}\"";
            RunProcess(processPath, arguments);
        }

        #endregion

        #region Jpeg XR

        private void OptimizeJxr(ImageServiceModel image, int quality)
        {
            var tempImagePath = CreateTempImage(image.FilePath);
            var tifImagePath = ConvertToTif(tempImagePath);
            JxrEncApp(tifImagePath, tempImagePath, quality, image.IsLossless);
            File.Delete(tifImagePath);
            HandleOptimizedImage(image.FilePath, tempImagePath);
        }

        private string ConvertToJxr(ImageServiceModel image, int quality)
        {
            var outImageName = image.Name + ".jxr";
            var outImagePath = Path.Combine(_imageFolderPath, outImageName);

            // Checking the input image extension
            if (string.Equals(".tif", image.FileExtension, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".tiff", image.FileExtension, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".bmp", image.FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                JxrEncApp(image.FilePath, outImagePath, quality, image.IsLossless);
            }
            else
            {
                var tifImagePath = ConvertToTif(image.FilePath);
                JxrEncApp(tifImagePath, outImagePath, quality, image.IsLossless);
                File.Delete(tifImagePath);
            }

            return outImagePath;
        }

        // Convert BMP, TIFF to JXR
        // -i input.bmp/tif/hdr         Input image file name
        //                              bmp: <=8bpc, BGR
        //                              tif: >=8bpc, RGB
        //                              hdr: 24bppRGBE only
        // -o output.jxr                Output JPEG XR file name
        // -q quality                   [0.0 - 1.0) Default = 1.0, lossless
        // or quantization              [1   - 255] Default = 1, lossless
        // -Q quantization for alpha    [1 - 255] Default = 1, lossless
        // -d chroma sub-sampling       0: Y-only
        //                              1: YCoCg 4:2:0
        //                              2: YCoCg 4:2:2
        //                              3: YCoCg 4:4:4 (default)
        // (if not set is 4:4:4 for quality >= 0.5 or 4:2:0 for quality< 0.5)
        private void JxrEncApp(string tiffImagePath, string outImagePath, int quality, bool isLossless)
        {
            var processPath = Path.Combine(_pluginFolderPath, JxrFolder, "JXREncApp.exe");
            var qualityFloat = ((float)quality / 100).ToString().Replace(',', '.');
            var arguments = isLossless
                ? $"-i \"{tiffImagePath}\" -o \"{outImagePath}\" -q 1.0 -d 3"
                : $"-i \"{tiffImagePath}\" -o \"{outImagePath}\" -q {qualityFloat} -d 1";
            RunProcess(processPath, arguments);
        }

        #endregion

        #region Apng

        private void OptimizeApng(ImageServiceModel image)
        {
            ApngOpt(image);
            Defluff(image);
        }

        private string ConvertGifToApng(ImageServiceModel image)
        {
            var outImageName = image.Name + ".png";
            var outImagePath = Path.Combine(_imageFolderPath, outImageName);

            // Get better result if optimize the cinverted apng with ApngOpt, rather than Gif2Apng
            Gif2Apng(image.FilePath, outImagePath);
            OptimizeApng(image);
            return outImagePath;
        }

        // -z0  : zlib compression
        // -z1  : 7zip compression(default)
        // -z2  : zopfli compression
        private void Gif2Apng(string imagePath, string outImagePath)
        {
            var processPath = Path.Combine(_pluginFolderPath, ApngFolder, "gif2apng.exe");
            var arguments = $"-z0 \"{imagePath}\" \"{outImagePath}\"";

            RunProcess(processPath, arguments);
        }

        // -z0  : zlib compression
        // -z1  : 7zip compression(default)
        // -z2  : zopfli compression
        private void ApngOpt(ImageServiceModel image)
        {
            var tempImagePath = CreateTempImagePath(image.FilePath);
            var processPath = Path.Combine(_pluginFolderPath, ApngFolder, "apngopt.exe");
            string arguments = $"-z2 \"{image.FilePath}\" \"{tempImagePath}\"";
            Optimize(image.FilePath, tempImagePath, processPath, arguments);
        }

        #endregion

        #region Others

        private string ConvertToTif(string imagePath)
        {
            var outImageName = Path.GetFileName(imagePath) + ".tif";
            var outImagePath = Path.Combine(_imageFolderPath, outImageName);
            
            NConvert(imagePath, outImagePath, "tiff");
            return outImagePath;
        }
        
        private void NConvert(string imagePath, string outImagePath, string outFormat)
        {
            var processPath = Path.Combine(_pluginFolderPath, OthersFolder, "nconvert.exe");
            var arguments = $"-out {outFormat} -c 0 -rmeta -overwrite -quiet -o \"{outImagePath}\" \"{imagePath}\"";

            RunProcess(processPath, arguments);
        }

        #endregion

        #region Helpers
        private string CreateTempImage(string sourceImagePath)
        {
            var tempImageName = Path.GetFileNameWithoutExtension(sourceImagePath) + "-temp" + Path.GetExtension(sourceImagePath);
            var tempImagePath = Path.Combine(_imageFolderPath, tempImageName);
            File.Copy(sourceImagePath, tempImagePath, true);
            return tempImagePath;
        }

        private string CreateTempImagePath(string sourceImagePath)
        {
            var tempImageName = Path.GetFileNameWithoutExtension(sourceImagePath) + "-temp" + Path.GetExtension(sourceImagePath);
            return Path.Combine(_imageFolderPath, tempImageName);
        }

        private static void Optimize(string imagePath, string tempImagePath, string processPath, string arguments)
        {
            RunProcess(processPath, arguments);
            HandleOptimizedImage(imagePath, tempImagePath);
        }

        private static void RunProcess(string processPath, string arguments, bool createNoWindow = false, bool useShellExecute = false, bool redirectStandardOutput = false)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = processPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.CreateNoWindow = createNoWindow;
                process.StartInfo.UseShellExecute = useShellExecute;
                process.StartInfo.RedirectStandardOutput = redirectStandardOutput;
                process.Start();
                process.WaitForExit();
            }            
        }

        private static void HandleOptimizedImage(string sourceImagePath, string tempImagePath)
        {
            if (!File.Exists(tempImagePath))
                return;

            var sourceImageSize = new FileInfo(sourceImagePath).Length;
            var optimizedImageSize = new FileInfo(tempImagePath).Length;

            if ((sourceImageSize > optimizedImageSize) && (optimizedImageSize > 0))
            {
                File.Delete(sourceImagePath);
                File.Move(tempImagePath, sourceImagePath);
            }
            else
            {
                File.Delete(tempImagePath);
            }
        }

        private static void HandleConvertedImage(ImageServiceModel image, bool forceConvert, ConvertedImageServiceModel convertedImage, ICollection<ConvertedImageServiceModel> convertedImages)
        {
            if ((convertedImage.OptimizedSize < image.OptimizedSize || forceConvert) &&
                convertedImage.OptimizedSize > 0)
            {                
                convertedImages.Add(convertedImage);
            }
        }
        #endregion

    }
}
