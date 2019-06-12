using System;
using System.IO;
using System.Collections.Generic;
public static class ReaderExt{

    public static UInt16 ReverseBytes(this UInt16 value)
    {
        return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
    }
    public static UInt32 ReverseBytes(this UInt32 value)
    {
        return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 | (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
    }
    public static UInt64 ReverseBytes(this UInt64 value)
    {
        return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }

    public static string ReadCString(this BinaryReader reader)
    {
        List<byte> bytes = new List<byte>();
        do
        {
            bytes.Add(reader.ReadByte());
        } while (bytes[bytes.Count - 1] != (byte)0);
        
        return System.Text.Encoding.UTF8.GetString(bytes.GetRange(0, bytes.Count - 1).ToArray());
    }

    public static string ReadUTF8String(this BinaryReader reader, int length){
        List<byte> bytes = new List<byte>();
        for(int i=0;i<length;i++){
            bytes.Add(reader.ReadByte());
        }
        return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }
    public static long ReadUntilData(this BinaryReader reader){
        byte b;
        long startPos=reader.BaseStream.Position;
        do{
            b=reader.ReadByte();
        }while(b==(byte)0);
        long skipped=reader.BaseStream.Position-startPos;
        reader.BaseStream.Seek(-1,SeekOrigin.Current);
        return skipped-1;
    }
    public static void ScanUntilFound(this BinaryReader reader, uint target){
        //1952540531
        uint data;
        try{
            do{
                data=reader.ReadUInt32();
            }while(data!=target);
        }catch(System.IO.EndOfStreamException eof){
            Console.WriteLine("Couldn't find target:"+target);
        }
        reader.BaseStream.Seek(-4,SeekOrigin.Current);
    }

    public static string ToByteString(this string s){
        char[] scar=s.ToCharArray();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for(int i = 0;i<scar.Length;i++){
            sb.Append(Convert.ToUInt32(scar[i]).ToString("X"));
        }
        return sb.ToString();
    }
}