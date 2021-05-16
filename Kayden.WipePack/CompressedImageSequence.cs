using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public static class CompressedImageSequence
    {
        static int bitIndex = 0;
        static byte currentVal;

        static BinaryReader reader;
        static BinaryWriter writer;
        public static IEnumerable<Texture2D> GetSequence(BinaryReader r)
        {
            reader = r;

            int len = reader.ReadInt16();
            int width = reader.ReadInt16(), height = reader.ReadInt16();

            bitIndex = 0;
            currentVal = r.ReadByte();

            for (int i = 0; i < len; ++i)
            {
                yield return DecompressTexture(width, height, reader);
            }

            reader = null;
            yield break;
        }
        public static void SaveSequence(string path, IEnumerable<Texture2D> textures)
        {
            currentVal = 0;
            bitIndex = 0;
            using (var w = new BinaryWriter(File.OpenWrite(path)))
            {
                List<Texture2D> texList = new List<Texture2D>(textures);
                w.Write((ushort)texList.Count);
                w.Write((ushort)texList[0].Width);
                w.Write((ushort)texList[0].Height);

                int width = texList[0].Width, height = texList[0].Height;

                writer = w;
                foreach (var tex in textures)
                {
                    if (tex.Width != width || tex.Height != height)
                        throw new System.Exception();
                    CompressTexture(tex);
                }
                writer = null;
            }
        }

        private static Texture2D DecompressTexture(int width, int height, BinaryReader input)
        {
            Texture2D texture = new Texture2D(Engine.Graphics.GraphicsDevice, width, height);

            byte[] array = new byte[(width * height) << 2];

            int index = 0;
            do
            {
                byte value = (byte)SignedInt();
                uint len;

                if (value < 2)
                {
                    len = UnsignedInt();
                    len = Math.Min(len, (uint)(array.Length - index));

                    if (value == 1)
                        value = 255;
                }
                else
                {
                    len = 1;
                    value--;
                }

                while (len-- > 0)
                {
                    array[index + 0] = value;
                    array[index + 1] = value;
                    array[index + 2] = value;
                    array[index + 3] = value;
                    index += 4;
                }

            } while (index < array.Length);

            texture.SetData(array);

            return texture;
        }
        private static bool GetBoolean()
        {
            bool value = (currentVal & 1) > 0;
            currentVal >>= 1;

            if (bitIndex++ == 8)
            {
                bitIndex = 0;
                if (reader.BaseStream.CanRead)
                    currentVal = reader.ReadByte();

            }

            return value;
        }
        private static uint UnsignedInt()
        {
            int length = 1;

            while (GetBoolean())
                ++length;

            uint l = (uint)(1 << length) - 2;
            uint v = 0;
            for (int i = 0; i < length; ++i)
                if (GetBoolean())
                    v |= (uint)(1 << i);

            return l + v;
        }
        private static int SignedInt()
        {
            return (int)UnsignedInt();
        }

        private static void CompressTexture(Texture2D texture)
        {
            byte[] buffer = new byte[(texture.Width * texture.Height) << 2];

            texture.GetData(buffer);

            int i = 0;

            while (i < buffer.Length)
            {
                if (buffer[i] == 0 || buffer[i] == 255)
                {
                    byte val = buffer[i];
                    AddUInt((uint)(val & 0x1));
                    int len = 0;

                    while (buffer[i] == val)
                    {
                        ++len;
                        i += 4;
                    }
                    AddUInt((uint)len);
                }
                else
                {
                    byte val = buffer[i];

                    AddUInt((uint)(buffer[i] + 1));
                }
            }
        }
        private static void AddBit(bool _value)
        {
            currentVal |= (byte)((_value ? 1 : 0) << (bitIndex++));
            if (bitIndex == 8)
            {

                bitIndex = 0;
            }
        }
        private static void AddUInt(uint _value)
        {
            // Compress the number + 2 to allow values 0 and 1
            _value += 2;

            uint ex = _value;
            int len = 0;
            while (ex > 1)
            {
                ex >>= 1;
                ++len;
            }

            uint l = (uint)(1 << len), v = _value - l;
            l -= 2;

            for (int i = len - 1; i >= 0; --i)
            {
                AddBit((l & (1 << i)) != 0);
            }
            for (int i = 0; i < len; ++i)
            {
                AddBit((v & (1 << i)) != 0);
            }
        }
    }
}