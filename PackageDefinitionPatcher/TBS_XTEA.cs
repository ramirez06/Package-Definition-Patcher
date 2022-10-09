//All of this program have been made by The Big Shell.
//Big thanks to Notex and his RPKG Tool source code.
//Another big thanks to A.W. Stanley for his h6xtea source code.

//The Big Shell make the total conversion to C# code

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageDefinitionPatcher
{
	static class TBS_XTEA
	{
		static uint Delta = 0x61C88647;
		static uint Sum = 0xC6EF3720;

		static byte[] Headers = new byte[] { 0x22, 0x3D, 0x6F, 0x9A, 0xB3, 0xF8, 0xFE, 0xB6, 0x61, 0xD9, 0xCC, 0x1C, 0x62, 0xDE, 0x83, 0x41 };
		static int[] StaticKeys = new int[] { 0x30F95282, 0x1F48C419, 0x295F8548, 0x2A78366D };
		static int HeadersLength = 20;

		unsafe static void XTEA_EncryptBlock(int* A, int* B)
		{

			uint a = (uint)*A;
			uint b = (uint)*B;
			uint CurrSum = 0;

			for (int i = 0; i < 32; i++)
			{
				a += (uint)((int)(((b << 4) ^ (b >> 5)) + b) ^ ((int)(CurrSum) + StaticKeys[CurrSum & 3]));
				CurrSum -= Delta;
				b += (uint)((int)(((a << 4) ^ (a >> 5)) + a) ^ ((int)(CurrSum) + StaticKeys[(CurrSum >> 11) & 3]));
			}

			*A = (int)(a);
			*B = (int)(b);
		}

		unsafe static void XTEA_DecryptBlock(int *A, int *B)
		{

			uint a = (uint)*A;
			uint b = (uint)*B;
			uint CurrSum = Sum;

			for (int i = 0; i < 32; i++)
            {
				b -= (uint)((int)(((a << 4) ^ (a >> 5)) + a) ^ ((int)(CurrSum) + StaticKeys[(CurrSum >> 11) & 3]));
				CurrSum += Delta;
				a -= (uint)((int)(((b << 4) ^ (b >> 5)) + b) ^ ((int)(CurrSum) + StaticKeys[CurrSum & 3]));
			}

			*A = (int)(a);
			*B = (int)(b);
		}

		unsafe static byte[] XTEA_EncryptBytes(byte[] DecryptedBytes)
		{
			while ((DecryptedBytes.Length % 8) != 0)
			{
				DecryptedBytes = (DecryptedBytes.Concat(new byte[] { 0x0 })).ToArray();
			}

			int blockCount = (DecryptedBytes.Length) / 8;

			byte[] resultbytes = new byte[DecryptedBytes.Length];

			BinaryReader sourcebr = new BinaryReader(new MemoryStream(DecryptedBytes));
			BinaryWriter destbw = new BinaryWriter(new MemoryStream(resultbytes));

			int a = 0;
			int b = 0;

			for (int i = 0; i < blockCount; i++)
			{
				a = sourcebr.ReadInt32();
				b = sourcebr.ReadInt32();
				XTEA_EncryptBlock(&a, &b);
				destbw.Write(a);
				destbw.Write(b);
			}

			sourcebr.Close();
			destbw.Close();

			return resultbytes;
		}

		unsafe static byte[] XTEA_DecryptBytes(byte[] EncryptedBytes)
        {
			byte[] resultbytes = new byte[EncryptedBytes.Length];
			BinaryReader sourcebr = new BinaryReader(new MemoryStream(EncryptedBytes));
			BinaryWriter destbw = new BinaryWriter(new MemoryStream(resultbytes));

			int blockCount = (EncryptedBytes.Length) / 8;

			int a = 0;
			int b = 0;

			for (int i = 0; i < blockCount; i++)
            {
				a = sourcebr.ReadInt32();
				b = sourcebr.ReadInt32();
				XTEA_DecryptBlock(&a, &b);
				destbw.Write(a);
				destbw.Write(b);
            }

			sourcebr.Close();
			destbw.Close();

			int resultlength = resultbytes.Length;
			int tcnt = 1;
			for (tcnt = 1; tcnt < resultlength; tcnt++)
            {
				if (resultbytes[resultlength - tcnt] != 0x00)
                {
					tcnt--;
					break;
				}
            }

			return resultbytes.Take((resultlength - tcnt)).ToArray();
        }

		public static byte[] XTEA_GetEncryptedBytes(byte[] Source)
		{
			byte[] newbytes = XTEA_EncryptBytes(Source);

			uint crc32res = Crc32.Compute(Source);
			byte[] Checksum = new byte[] { ((byte)crc32res), ((byte)(crc32res >> 8)), ((byte)(crc32res >> 16)), ((byte)(crc32res >> 24)) };

			byte[] result = new byte[Headers.Length + Checksum.Length + newbytes.Length];

			BinaryWriter bw1 = new BinaryWriter(new MemoryStream(result, 0, result.Length));

			bw1.Write(Headers, 0, Headers.Length);
			bw1.Write(Checksum, 0, Checksum.Length);
			bw1.Write(newbytes, 0, newbytes.Length);

			bw1.Close();

			return result;
		}

		public static byte[] XTEA_GetDecryptedBytes(byte[] Source)
        {
			byte[] newbytes = XTEA_DecryptBytes((Source.Skip(HeadersLength)).ToArray());

			return newbytes;
        }

		public static void XTEA_EncryptFile(string SourcePath, string DestinationPath)
        {
			File.WriteAllBytes(DestinationPath, XTEA_GetEncryptedBytes(File.ReadAllBytes(SourcePath)));
        }

		public static void XTEA_DecryptFile(string SourcePath, string DestinationPath)
        {
			File.WriteAllBytes(DestinationPath, XTEA_GetDecryptedBytes(File.ReadAllBytes(SourcePath)));
		}
	}
}
