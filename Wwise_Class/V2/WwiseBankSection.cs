using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BNKManager
{
    public class BankSection
    {
        public string sectionName;
        public long dataStartOffset;
        public byte[] sectionData;
        protected byte[] GetSectionBytes(byte[] sectionData)
        {
            byte[] sectionBytes = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    bw.Write(Encoding.ASCII.GetBytes(this.sectionName));
                    bw.Write((uint)sectionData.Length);
                    bw.Write(sectionData);
                }
                sectionBytes = mStream.ToArray();
            }
            return sectionBytes;
        }
        public virtual byte[] GetBytes()
        {
            return this.GetSectionBytes(this.sectionData);
        }
        public BankSection(string sectionName, long dataStartOffset, byte[] sectionData)
        {
            this.dataStartOffset = dataStartOffset;
            this.sectionName = sectionName;
            this.sectionData = sectionData;
        }
    }
    public class BKHDSection : BankSection
    {
        public uint soundbankVersion;
        public uint soundbankId;
        public uint zero1;
        public uint zero2;
        public byte[] unknown;
        public BKHDSection(BinaryReader br, uint length) : base("BKHD", br.BaseStream.Position, null)
        {
            this.soundbankVersion = br.ReadUInt32();
            this.soundbankId = br.ReadUInt32();
            this.zero1 = br.ReadUInt32();
            this.zero2 = br.ReadUInt32();
            if (length > 16)
            {
                this.unknown = br.ReadBytes((int)length - 16);
            }
        }
        public override byte[] GetBytes()
        {
            byte[] sectionData = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    bw.Write(this.soundbankVersion);
                    bw.Write(this.soundbankId);
                    bw.Write(this.zero1);
                    bw.Write(this.zero2);
                    bw.Write(this.unknown);
                }
                sectionData = mStream.ToArray();
            }
            return base.GetSectionBytes(sectionData);
        }
    }
    public class HIRCSection : BankSection
    {
        public List<WwiseObject> objects = new List<WwiseObject>();
        public HIRCSection(BinaryReader br) : base("HIRC", br.BaseStream.Position, null)
        {
            uint objectCount = br.ReadUInt32();
            for (uint i = 0; i < objectCount; i++)
            {
                if (br.BaseStream.Position >= br.BaseStream.Length)
                {
                    break;
                }
                WwiseObjectType objType = (WwiseObjectType)br.ReadByte();
                uint objLength = br.ReadUInt32();
                switch (objType)
                {
                    case WwiseObjectType.Sound_SFX__Sound_Voice:
                        this.objects.Add(new SoundSFXVoiceWwiseObject(br, objLength));
                        break;
                    case WwiseObjectType.Event_Action:
                        this.objects.Add(new EventActionWwiseObject(br, objLength));
                        break;
                    //本家WoTのpckファイルを読み込むとクラッシュするため廃止
                    case WwiseObjectType.Event:
                        this.objects.Add(new EventWwiseObject(br));
                        break;
                    default:
                        //this.objects.Add(new WwiseObject(objType, br.ReadBytes((int)objLength)));
                        break;
                }
            }
        }
        public override byte[] GetBytes()
        {
            byte[] sectionData = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    bw.Write((uint)this.objects.Count);
                    foreach (WwiseObject obj in this.objects)
                    {
                        bw.Write(obj.GetBytes());
                    }
                }
                sectionData = mStream.ToArray();
            }
            return base.GetSectionBytes(sectionData);
        }
    }
    public class STIDSection : BankSection
    {
        uint unknown;
        List<ReferencedSoundbank> refSoundbanks = new List<ReferencedSoundbank>();
        public STIDSection(BinaryReader br) : base("STID", br.BaseStream.Position, null)
        {
            this.unknown = br.ReadUInt32();
            uint soundbanksCount = br.ReadUInt32();
            for (uint i = 0; i < soundbanksCount; i++)
            {
                this.refSoundbanks.Add(new ReferencedSoundbank(br));
            }
        }
        public override byte[] GetBytes()
        {
            byte[] sectionData = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    bw.Write(this.unknown);
                    bw.Write((uint)this.refSoundbanks.Count);
                    foreach (ReferencedSoundbank refSoundbank in this.refSoundbanks)
                    {
                        refSoundbank.Write(bw);
                    }
                }
                sectionData = mStream.ToArray();
            }
            return base.GetSectionBytes(sectionData);
        }
        public class ReferencedSoundbank
        {
            public uint ID;
            public string name;
            public ReferencedSoundbank(BinaryReader br)
            {
                this.ID = br.ReadUInt32();
                this.name = Encoding.ASCII.GetString(br.ReadBytes(br.ReadByte()));
            }
            public void Write(BinaryWriter bw)
            {
                bw.Write(this.ID);
                bw.Write((byte)this.name.Length);
                bw.Write(Encoding.ASCII.GetBytes(this.name));
            }
        }
    }
    public class DIDXSection : BankSection
    {
        public List<EmbeddedWEM> embeddedWEMFiles = new List<EmbeddedWEM>();
        public DIDXSection(BinaryReader br, uint length) : base("DIDX", br.BaseStream.Position, null)
        {
            uint filesCount = length / 12;
            for (uint i = 0; i < filesCount; i++)
            {
                this.embeddedWEMFiles.Add(new EmbeddedWEM(br));
            }
        }
        public EmbeddedWEM GetEmbeddedWEM(uint fileID)
        {
            return this.embeddedWEMFiles.Find(x => x.ID == fileID);
        }
        public override byte[] GetBytes()
        {
            byte[] sectionData = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    foreach (EmbeddedWEM embeddedFile in embeddedWEMFiles)
                    {
                        embeddedFile.Write(bw);
                    }
                }
                sectionData = mStream.ToArray();
            }
            return base.GetSectionBytes(sectionData);
        }
        public class EmbeddedWEM
        {
            public uint ID;
            public uint offset;
            public uint length;
            public EmbeddedWEM(BinaryReader br)
            {
                this.ID = br.ReadUInt32();
                this.offset = br.ReadUInt32();
                this.length = br.ReadUInt32();
            }
            public void Write(BinaryWriter bw)
            {
                bw.Write(this.ID);
                bw.Write(this.offset);
                bw.Write(this.length);
            }
        }
    }
    public class DATASection : BankSection
    {
        public List<WEMFile> wemFiles = new List<WEMFile>();
        public DIDXSection dataIndex;
        public DATASection(BinaryReader br, uint length, DIDXSection dataIndex) : base("DATA", br.BaseStream.Position, null)
        {
            this.dataIndex = dataIndex;
            long offset = br.BaseStream.Position;
            foreach (DIDXSection.EmbeddedWEM embWEM in dataIndex.embeddedWEMFiles)
            {
                this.wemFiles.Add(new WEMFile(br, offset, embWEM));
            }
            br.BaseStream.Seek(offset + length, SeekOrigin.Begin);
        }
        public override byte[] GetBytes()
        {
            byte[] sectionBytes = null;
            using (MemoryStream mStream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mStream))
                {
                    bw.Write(Encoding.ASCII.GetBytes(this.sectionName));
                    bw.Write((int)0);
                    foreach (WEMFile embWEM in this.wemFiles)
                    {
                        bw.Seek((int)embWEM.info.offset + 8, SeekOrigin.Begin);
                        bw.Write(embWEM.data);
                    }
                    uint length = (uint)(bw.BaseStream.Position - 8);
                    bw.BaseStream.Seek(4, SeekOrigin.Begin);
                    bw.Write(length);
                }
                sectionBytes = mStream.ToArray();
            }
            return sectionBytes;
        }
        public class WEMFile
        {
            public DIDXSection.EmbeddedWEM info;
            public byte[] data;
            public WEMFile(BinaryReader br, long DATASectionOffset, DIDXSection.EmbeddedWEM info)
            {
                this.info = info;
                br.BaseStream.Seek(DATASectionOffset + info.offset, SeekOrigin.Begin);
                this.data = br.ReadBytes((int)info.length);
            }
        }
    }
}