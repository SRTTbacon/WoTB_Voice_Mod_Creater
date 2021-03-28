using PortableDeviceApiLib;
using System;
using System.Collections.Generic;
using System.IO;

// Portable Device Folder
namespace PortableDevices
{
    public class PortableDeviceFolder : PortableDeviceObject
    {
        public PortableDeviceFolder(string id, string name) : base(id, name)
        {
            this.Files = new List<PortableDeviceObject>();
        }

        public IList<PortableDeviceObject> Files { get; set; }

        /**
         * Device must be connected!
         * 
         * Returns the id of the new folder.
         */
        public PortableDeviceFolder CreateDir (PortableDevice device, string folderName)
        {
            PortableDeviceFolder result = null;

            try
            {
                String newFolderId = "";

                // Already got folderName?
                result = FindDir (folderName);
                if (null == result)
                {
                    IPortableDeviceContent content = device.getContents();

                    // Get the properties of the object
                    IPortableDeviceProperties properties;
                    content.Properties(out properties);

                    IPortableDeviceValues createFolderValues = GetRequiredCreateDirPropertiesForContentType(Id, folderName);

                    content.CreateObjectWithPropertiesOnly(createFolderValues, ref newFolderId);

                    // Last, add to list of files.
                    result = new PortableDeviceFolder(newFolderId, folderName);

                    this.Files.Add(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = null;
            }

            return result;

        }

        private IPortableDeviceValues GetRequiredCreateDirPropertiesForContentType (string parentFolderId, string folderName)
        {
            IPortableDeviceValues values = new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;

            var WPD_OBJECT_PARENT_ID = new _tagpropertykey();
            WPD_OBJECT_PARENT_ID.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_PARENT_ID.pid = 3;
            values.SetStringValue(ref WPD_OBJECT_PARENT_ID, parentFolderId);

            var WPD_OBJECT_ORIGINAL_FILE_NAME = new _tagpropertykey();
            WPD_OBJECT_ORIGINAL_FILE_NAME.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_ORIGINAL_FILE_NAME.pid = 12;
            values.SetStringValue(WPD_OBJECT_ORIGINAL_FILE_NAME, Path.GetFileName(folderName));

            var WPD_OBJECT_NAME = new _tagpropertykey();
            WPD_OBJECT_NAME.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_NAME.pid = 4;
            values.SetStringValue(WPD_OBJECT_NAME, Path.GetFileName(folderName));

            var WPD_OBJECT_CONTENT_TYPE = new _tagpropertykey();
            WPD_OBJECT_CONTENT_TYPE.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_CONTENT_TYPE.pid = 7;
            Guid WPD_CONTENT_TYPE_FOLDER = new Guid(0x27E2E392, 0xA111, 0x48E0, 0xAB, 0x0C, 0xE1, 0x77, 0x05, 0xA0, 0x5F, 0x85);
            values.SetGuidValue(WPD_OBJECT_CONTENT_TYPE, WPD_CONTENT_TYPE_FOLDER);

            var WPD_OBJECT_FORMAT = new _tagpropertykey();
            WPD_OBJECT_FORMAT.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_FORMAT.pid = 6;
            Guid WPD_OBJECT_FORMAT_PROPERTIES_ONLY = new Guid(0x30010000, 0xAE6C, 0x4804, 0x98, 0xBA, 0xC5, 0x7B, 0x46, 0x96, 0x5F, 0xE7);
            values.SetGuidValue(WPD_OBJECT_FORMAT, WPD_OBJECT_FORMAT_PROPERTIES_ONLY);

            return values;
        }

        public void DeleteFile (PortableDevice device, String name)
        {
            PortableDeviceFile fileToDelete = null;
            try
            {
                foreach (var file in Files)
                {                    
                    if (file is PortableDeviceFile && name.Equals(file.Name))
                    {
                        fileToDelete = (PortableDeviceFile)file;
                        break;                        
                    }                    
                }

                // Got file?
                if (null != fileToDelete) {
                    device.DisconnectConnect();
                    device.DeleteFile(fileToDelete);
                    Files.Remove(fileToDelete);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                device.Disconnect();
            }
        }

        /**
         * CopyFolderToPhone
         */
        public void CopyFolderToPhone (PortableDevice device, String pcPath, String phonePath)
        {
            String errors = "";

            // Get all the files in the dir.
            string[] files = Directory.GetFiles (pcPath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var fn in files)
            {
                try
                {
                    device.TransferContentToDevice (this, fn);
                }
                catch (Exception ex)
                {
                    errors += @"\n\n" + ex.Message;
                }
            }

            // Got errors?
            if (!String.IsNullOrEmpty(errors))
            {
                throw new Exception(errors);
            }
        }

        public void CopyFolderToPC (PortableDevice device, String pcPath)
        {
            String errors = "";

            // Go thru all the files on the Phone.
            foreach (var file in Files)
            {
                try
                {
                    if (file is PortableDeviceFile)
                    {
                        device.TransferContentFromDevice ((PortableDeviceFile)file, pcPath, file.Name);
                    }
                    else
                    {
                        errors += @"Could not find file:" + file.Name;
                    }
                }
                catch (Exception ex)
                {
                    errors += " " + ex.Message + " ";
                }
            }

            // Got errors?
            if (!String.IsNullOrEmpty(errors))
            {
                throw new Exception(errors);
            }
        }

        /**
         * FindDir - look in dir passed in and also in any subdirs.
         */
        public PortableDeviceFolder FindDir (String path)
        {
            PortableDeviceFolder result = null;
            try
            {
                foreach (var word in path.Split('\\'))
                {
                    if (Name.Equals(word))
                    {
                        result = (PortableDeviceFolder)this;
                        break;
                    }

                    else
                    {
                        foreach (var sub_data in Files)
                        {
                            if (sub_data is PortableDeviceFolder) {
                                if (sub_data.Name.Equals(word))
                                {
                                    String subPath = last(path, word);
                                    result = ((PortableDeviceFolder)sub_data).FindDir(subPath);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
        }

        public PortableDeviceFile FindFile (String fileName)
        {
            // First look for file in dir passed in.
            PortableDeviceFile result = null;
            if (null == result)
            {
                // Now look in subdirs.
                try
                {
                    foreach (var sub_data in Files)
                    {
                        if (sub_data.Name.Equals(fileName))
                        {
                            result = (PortableDeviceFile)sub_data;
                            break;
                        }
                        else
                        {
                            String matchStr = matching(sub_data.Name, fileName);
                            if (matchStr.Length > 0)
                            {
                                result = (PortableDeviceFile)sub_data;
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return result;
        }    

        /**
         * Return the string that matches both from beginning.
         */
        public static String matching (String str1, String str2)
        {
            String result = "";
            for (int pos = 0; pos < str1.Length; pos++)
            {
                if (str1[pos].Equals(str2[pos]))
                {
                    result = str1.Substring(0, pos);
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        /**
         * Return the rest of the string after the target.
         */
        public static String last (String path, String target)
        {
            String result = "";
            int start = 0;
            if (path.Equals(target))
            {
                result = target;
            }
            else if (path.Length > target.Length)
            {
                start = path.LastIndexOf(target);
                if (start >= 0)
                {
                    result = path.Substring(start + target.Length);
                }
            }

            // Skip any leading slashes.
            if (result[0].Equals('\\'))
            {
                result = result.Substring(1);
            }
            return result;
        }
    }
}