using System;
using System.IO;
using System.Text;
using System.Drawing;

namespace DM.image2h
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine("Image -> .c/.h source snippet");
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine("Command: image2h <input file>");
                Console.WriteLine("Example: 'image2h d:\\tmp\\smile.png' creates c code to d:\\tmp\\ico32_smile.h");
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine("(c)2023 dm");
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("Missing file argument");
                return;
            }

            string imageFile = args[0];

            if (!File.Exists(imageFile))
            {
                Console.WriteLine($"File {imageFile} not found");
                return;
            }

            try
            {
                string name = Path.GetFileNameWithoutExtension(imageFile);
                int size;
                byte[] res;
                string codeFile;

                using (Bitmap bmp = new Bitmap(imageFile))
                {
                    if (bmp.Width % 8 != 0)
                    {
                        throw new Exception("Image width must be divisible by 8");
                    }
                    size = bmp.Width;

                    codeFile = Path.GetFullPath(imageFile)
                        .Replace(Path.GetFileName(imageFile), String.Format(
                            $"ico{size}_{name}.h"));

                    bool inversion = bmp.GetPixel(0, 0).GetBrightness() > 0.5f;
                    int pixel = inversion ? 0 : 1;
                    int space = inversion ? 1 : 0;

                    res = new byte[size * size / 8];
                    int r = 0;
                    res[r] = 0;
                    int b = 0;

                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            int bb = y * size + x;
                            res[r] |= (byte)(bmp.GetPixel(x, y).GetBrightness() > 0 ? pixel : space);
                            b++;
                            if (b < 8)
                            {
                                res[r] <<= 1;
                            }
                            else
                            {
                                b = 0;
                                r++;
                            }
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.Append($"const uint8_t ico{size}_{name}[] PROGMEM = {{\r\n\t");
                for (int i = 0; i < res.Length; i++)
                {
                    if (i == res.Length - 1)
                        sb.Append($"0x{res[i]:x2} }};");
                    else
                        sb.Append($"0x{res[i]:x2}, ");
                    if ((i + 1) % 8 == 0)
                        sb.Append("\r\n\t");
                }
                File.WriteAllText(codeFile, sb.ToString());
                Console.WriteLine($"File {codeFile} was created successfully :)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }
    }
}
