using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BNKManager
{
    public class WwiseBank : LoLSoundBank
    {
        public List<BankSection> bankSections = new List<BankSection>();
        public WwiseBank(string fileLocation) : base(fileLocation)
        {
            using (BinaryReader br = new BinaryReader(File.Open(fileLocation, FileMode.Open)))
                this.Read(br);
        }
        public override void Save()
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(this.fileLocation, FileMode.Create)))
                this.Write(bw);
        }
        public override void Save(string fileLocation)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(fileLocation, FileMode.Create)))
                this.Write(bw);
        }
        private void Read(BinaryReader br)
        {
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                string sectionName = Encoding.ASCII.GetString(br.ReadBytes(4));
                uint sectionLength = br.ReadUInt32();
                if (sectionName == "HIRC")
                    break;
                switch (sectionName)
                {
                    case "BKHD":
                        this.bankSections.Add(new BKHDSection(br, sectionLength));
                        break;
                    case "HIRC":
                        this.bankSections.Add(new HIRCSection(br));
                        break;
                    case "STID":
                        this.bankSections.Add(new STIDSection(br));
                        break;
                    case "DIDX":
                        this.bankSections.Add(new DIDXSection(br, sectionLength));
                        break;
                    case "DATA":
                        this.bankSections.Add(new DATASection(br, sectionLength, (DIDXSection)this.GetSection("DIDX")));
                        break;
                    default:
                        this.bankSections.Add(new BankSection(sectionName, br.BaseStream.Position, br.ReadBytes((int)sectionLength)));
                        break;
                }
            }
        }
        public BankSection GetSection(string sectionName)
        {
            return this.bankSections.Find(x => x.sectionName == sectionName);
        }
        public uint GetID()
        {
            BKHDSection headerSection = (BKHDSection)this.GetSection("BKHD");
            if (headerSection == null)
                return 0;
            else
                return headerSection.soundbankId;
        }
        private void Write(BinaryWriter bw)
        {
            foreach (BankSection bnkSection in this.bankSections)
            {
                bnkSection.dataStartOffset = bw.BaseStream.Position + 8;
                bw.Write(bnkSection.GetBytes());
            }
        }
    }
}