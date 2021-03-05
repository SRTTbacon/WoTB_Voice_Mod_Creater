using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BNKManager
{
    public class WPKSoundBank : LoLSoundBank
    {
        public uint version;
        public List<WPKSoundBankWEMFile> wemFiles = new List<WPKSoundBankWEMFile>();
        public WPKSoundBank(string fileLocation) : base(fileLocation)
        {
            using (BinaryReader br = new BinaryReader(File.Open(fileLocation, FileMode.Open)))
            {
                Read(br);
            }
        }
        public override void Save()
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(fileLocation, FileMode.Create)))
            {
                Write(bw);
            }
        }
        public override void Save(string fileLocation)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(fileLocation, FileMode.Create)))
            {
                Write(bw);
            }
        }
        void Read(BinaryReader br)
        {
            string header = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (header != "r3d2")
            {
                throw new Exception("選択されたファイルはPCKファイルではありません。");
            }
            version = br.ReadUInt32();
            uint fileCount = br.ReadUInt32();
            for (uint i = 0; i < fileCount; i++)
            {
                wemFiles.Add(new WPKSoundBankWEMFile(br));
            }
        }
        void Write(BinaryWriter bw)
        {
            bw.Write(Encoding.ASCII.GetBytes("r3d2"));
            bw.Write(version);
            bw.Write(wemFiles.Count);
            foreach (WPKSoundBankWEMFile wemFile in this.wemFiles)
            {
                bw.Write(wemFile.fileInfoOffset);
            }
            uint lastOffset = wemFiles[0].dataOffset;
            foreach (WPKSoundBankWEMFile wemFile in this.wemFiles)
            {
                bw.Seek((int)wemFile.fileInfoOffset, SeekOrigin.Begin);
                wemFile.dataOffset = lastOffset;
                wemFile.Write(bw);
                lastOffset += (uint)wemFile.data.Length;
            }
        }
        public class WPKSoundBankWEMFile
        {
            public uint ID;
            public uint fileInfoOffset;
            public uint dataOffset;
            public byte[] data;
            public WPKSoundBankWEMFile(BinaryReader br)
            {
                fileInfoOffset = br.ReadUInt32();
                long nextFileOffset = br.BaseStream.Position;
                br.BaseStream.Seek((int)fileInfoOffset, SeekOrigin.Begin);
                dataOffset = br.ReadUInt32();
                uint dataLength = br.ReadUInt32();
                uint idCharsCount = br.ReadUInt32();
                string idStr = Encoding.Unicode.GetString(br.ReadBytes(2 * (int)idCharsCount));
                ID = Convert.ToUInt32(idStr.Split('.')[0]);
                br.BaseStream.Seek((int)dataOffset, SeekOrigin.Begin);
                data = br.ReadBytes((int)dataLength);
                br.BaseStream.Seek(nextFileOffset, SeekOrigin.Begin);
            }
            public void Write(BinaryWriter bw)
            {
                bw.Write(dataOffset);
                bw.Write(data.Length);
                string idStr = ID + ".wem";
                bw.Write((uint)idStr.Length);
                bw.Write(Encoding.Unicode.GetBytes(idStr));
                long nextFileOffset = bw.BaseStream.Position;
                bw.Seek((int)this.dataOffset, SeekOrigin.Begin);
                bw.Write(data);
                bw.Seek((int)nextFileOffset, SeekOrigin.Begin);
            }
        }
    }
}