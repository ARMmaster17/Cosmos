﻿//using Cosmos.HAL.BlockDevice;

using Cosmos.HAL.BlockDevice;
using Cosmos.System;
using System;
using System.Collections.Generic;
using System.IO;
using SentinelKernel.System.FileSystem.FAT;
using SentinelKernel.System.FileSystem.Listing;
using Console = global::System.Console;
using Directory = SentinelKernel.System.FileSystem.Listing.Directory;

namespace SentinelKernel.System.FileSystem.VFS
{
    [Serializable]
    public struct KVP<TKey, TValue>
    {
        private readonly TKey key;
        private readonly TValue value;

        public KVP(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        public TKey Key
        {
            get { return key; }
        }

        public TValue Value
        {
            get { return value; }
        }
    }

    public class SentinelVFS : VFSBase
    {
        private List<Partition> mPartitions;

        private List<KVP<string, FileSystem>> mFileSystems;

        protected virtual void InitializePartitions()
        {
            for (int i = 0; i < BlockDevice.Devices.Count; i++)
            {
                if (BlockDevice.Devices[i] is Partition)
                {
                    mPartitions.Add((Partition)BlockDevice.Devices[i]);
                    break;
                }
            }

            if (mPartitions.Count > 0)
            {
                for (int i = 0; i < mPartitions.Count; i++)
                {
                    Console.WriteLine("Partition #: " + (i + 1));
                    Console.WriteLine("Block Size: " + mPartitions[i].BlockSize + " bytes");
                    Console.WriteLine("Size: " + mPartitions[i].BlockCount * mPartitions[i].BlockSize / 1024 / 1024 + " MB");
                }
            }
            else
            {
                Console.WriteLine("No partitions found!");
            }
        }

        protected virtual void InitializeFileSystems()
        {
            for (int i = 0; i < mPartitions.Count; i++)
            {
                string xRootPath = string.Concat(i, VolumeSeparatorChar, DirectorySeparatorChar);
                FileSystem xFileSystem = null;
                switch (FileSystem.GetFileSystemType(mPartitions[i]))
                {
                    case FileSystemType.FAT:
                        xFileSystem = new FAT.FatFileSystem(mPartitions[i]);
                        mFileSystems.Add(new KVP<string, FileSystem>(xRootPath, xFileSystem));
                        break;
                    default:
                        Console.WriteLine("Unknown filesystem type!");
                        return;
                }

                Console.Write("i = ");
                Console.WriteLine(i.ToString());
                Console.Write("mFileSystems.Count = ");
                Console.WriteLine(mFileSystems.Count);
                var xEntry = mFileSystems[i];
                if (xEntry.Key == xRootPath)
                {
                    var xFatFS = (FAT.FatFileSystem)xEntry.Value;
                    Console.WriteLine("-------File System--------");
                    Console.WriteLine("Bytes per Cluster: " + xFatFS.BytesPerCluster);
                    Console.WriteLine("Bytes per Sector: " + xFatFS.BytesPerSector);
                    Console.WriteLine("Cluster Count: " + xFatFS.ClusterCount);
                    Console.WriteLine("Data Sector: " + xFatFS.DataSector);
                    Console.WriteLine("Data Sector Count: " + xFatFS.DataSectorCount);
                    Console.WriteLine("FAT Sector Count: " + xFatFS.FatSectorCount);
                    Console.WriteLine("FAT Type: " + xFatFS.FatType);
                    Console.WriteLine("Number of FATS: " + xFatFS.NumberOfFATs);
                    Console.WriteLine("Reserved Sector Count: " + xFatFS.ReservedSectorCount);
                    Console.WriteLine("Root Cluster: " + xFatFS.RootCluster);
                    Console.WriteLine("Root Entry Count: " + xFatFS.RootEntryCount);
                    Console.WriteLine("Root Sector: " + xFatFS.RootSector);
                    Console.WriteLine("Root Sector Count: " + xFatFS.RootSectorCount);
                    Console.WriteLine("Sectors per Cluster: " + xFatFS.SectorsPerCluster);
                    Console.WriteLine("Total Sector Count: " + xFatFS.TotalSectorCount);

                    //Console.WriteLine();
                    //Console.WriteLine("Mapping Drive C...");
                    //FatFileSystem.AddMapping("C", mFileSystem);
                    //SentinelKernel.System.Filesystem.FAT.Listing.FatDirectory dir = new Sys.Filesystem.FAT.Listing.FatDirectory(mFileSystem, "Sentinel");
                }
                else
                {
                    Console.WriteLine("No filesystem found.");
                }
            }

            /*
            Console.WriteLine("Mapping...");
            TheFatFileSystem.AddMapping("C", xFS);

            Console.WriteLine();
            Console.WriteLine("Root directory");

            var xListing = xFS.GetRoot();
            TheFatFile xRootFile = null;
            TheFatFile xKudzuFile = null;


            for (int j = 0; j < xListing.Count; j++)
            {
                var xItem = xListing[j];
                if (xItem is FAT.Listing.MyFatDirectory)
                {
                    //Detecting Dir in HDD
                    Console.WriteLine("<DIR> " + xListing[j].Name);
                }
                else if (xItem is FAT.Listing.MyFatFile)
                {
                    //Detecting File in HDD
                    Console.WriteLine("<FILE> " + xListing[j].Name + " (" + xListing[j].Size + ")");
                    if (xListing[j].Name == "Root.txt")
                    {
                        xRootFile = (TheFatFile)xListing[j];
                        Console.WriteLine("Root file found");
                    }
                    else if (xListing[j].Name == "Kudzu.txt")
                    {
                        xKudzuFile = (TheFatFile)xListing[j];
                        Console.WriteLine("Kudzu file found");
                    }
                }
            }

            try
            {
                Console.WriteLine();
                Console.WriteLine("StreamReader - Root File");
                if (xRootFile == null)
                {
                    Console.WriteLine("RootFile not found!");
                }
                var xStream = new TheFatStream(xRootFile);
                var xData = new byte[xRootFile.Size];
                var size = (int)xRootFile.Size;
                Console.WriteLine("Size: " + size);
                var sizeInt = (int)xRootFile.Size;
                xStream.Read(xData, 0, sizeInt);
                var xText = Encoding.ASCII.GetString(xData);
                Console.WriteLine(xText);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            if (xKudzuFile == null)
            {
                Console.WriteLine("KudzuFile not found!");
            }
            var xKudzuStream = new TheFatStream(xKudzuFile);
            var xKudzuData = new byte[xKudzuFile.Size];
            xKudzuStream.Read(xKudzuData, 0, (int)xKudzuFile.Size);

            var xFile = new System.IO.FileStream(@"c:\Root.txt", System.IO.FileMode.Open);


            Console.WriteLine("Complete...");
            */
        }

        protected FileSystem GetFileSystemFromPath(string aPath)
        {
            string xPath = Path.GetPathRoot(aPath);
            for (int i = 0; i < mFileSystems.Count; i++)
            {
                string xTest = mFileSystems[i].Key;
                if (String.Equals(xTest, xPath))
                {
                    return mFileSystems[i].Value;
                }
            }
            throw new Exception("Unable to determine filesystem for path: " + aPath);
        }

        public override void Initialize()
        {
            mPartitions = new List<Partition>();
            mFileSystems = new List<KVP<string, FileSystem>>();

            InitializePartitions();
            if (mPartitions.Count > 0)
            {
                InitializeFileSystems();
            }
        }

        public override Listing.Directory GetDirectory(string aPath)
        {
            var xFS = GetFileSystemFromPath(aPath);

            return DoGetDirectory(aPath, xFS);
        }

        private Directory DoGetDirectory(string aPath, FileSystem aFS)
        {
            if (aFS == null)
            {
                throw new ArgumentNullException("aFS");
            }
            string[] xPathParts = VFSManager.SplitPath(aPath);

            if (xPathParts.Length == 1)
            {
                return GetVolume(aFS, aPath);
            }

            Listing.Directory xBaseDirectory = null;

            // start at index 1, because 0 is the volume
            for (int i = 1; i < xPathParts.Length; i++)
            {
                var xPathPart = xPathParts[i];
                var xPartFound = false;
                var xListing = aFS.GetDirectoryListing(xBaseDirectory);

                for (int j = 0; j < xListing.Count; j++)
                {
                    var xListingItem = xListing[j];
                    if (String.Equals(xListingItem.Name, xPathPart, StringComparison.OrdinalIgnoreCase))
                    {
                        if (xListingItem is Listing.Directory)
                        {
                            xBaseDirectory = (Listing.Directory)xListingItem;
                            xPartFound = true;
                        }
                        else
                        {
                            throw new Exception("Path part '" + xPathPart + "' found, but not a directory!");
                        }
                    }
                }

                if (!xPartFound)
                {
                    throw new Exception("Path part '" + xPathPart + "' not found!");
                }
            }
            return xBaseDirectory;
        }

        public override List<Listing.Base> GetDirectoryListing(string aPath)
        {
            var xFS = GetFileSystemFromPath(aPath);
            var xDirectory = DoGetDirectory(aPath, xFS);
            return xFS.GetDirectoryListing(xDirectory);
        }

        public override List<Listing.Base> GetDirectoryListing(Listing.Directory aParentDirectory)
        {
            throw new NotImplementedException();
        }

        public override Listing.Directory GetVolume(string aVolume)
        {
            throw new NotImplementedException();
        }

        public override List<Listing.Directory> GetVolumes()
        {
            throw new NotImplementedException();
        }

        public Listing.Directory GetVolume(FileSystem filesystem, string name)
        {
            return filesystem.GetRootDirectory(name);
        }

        /*
        public override SentinelKernel.System.FileSystem.Listing.Directory GetVolume(string aVolume)
        {
            var xFS = GetFileSystemFromPath(aVolumeId);
            if (xFS != null)
            {
                return new FileSystemEntry()
                {
                    Name = aVolumeId.ToString(),
                    Filesystem = xFS,
                    IsDirectory = true,
                    IsReadonly = true,
                    Id = (ulong)aVolumeId
                };
            }

            return null;
        }

        public override SentinelKernel.System.FileSystem.Listing.Directory[] GetVolumes()
        {
            var xResult = new SentinelKernel.System.FileSystem.Listing.Directory[mFileSystems.Count];
            for (int i = 0; i < mFileSystems.Count; i++)
            {
                xResult[i] = GetVolume(mFileSystems[i].Key);
            }
            return xResult;
        }
        */

        /*
        protected void InitializeHardware()
        {
            Console.WriteLine();
            Console.WriteLine("Initializing hardware...");
            if (BlockDevice.Devices.Count > 0)
            {
                Console.WriteLine("Block devices found: " + BlockDevice.Devices.Count);
                InitializeATADevices();
                if (this.mATA != null)
                {
                    InitializePartitions();
                }
            }
            else
            {
                Console.WriteLine("No block devices found!");
            }
            Console.WriteLine("Complete...");
            Console.WriteLine("Press enter.");
            Console.ReadLine();
        }
        */

        /*
        protected void InitializeATADevices()
        {
            Console.WriteLine("Initializing ATA Dedices...");
            try
            {
                for (int i = 0; i < BlockDevice.Devices.Count; i++)
                {
                    if (BlockDevice.Devices[i] is AtaPio)
                    {
                        this.mATA = (AtaPio)BlockDevice.Devices[i];
                        break;
                    }
                }

                if (mATA != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("--------ATA Devices-------");
                    Console.WriteLine("Type: " + (mATA.DriveType == AtaPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
                    Console.WriteLine("Serial No: " + mATA.SerialNo);
                    Console.WriteLine("Firmware Rev: " + mATA.FirmwareRev);
                    Console.WriteLine("Model No: " + mATA.ModelNo);
                    Console.WriteLine("Block Size: " + mATA.BlockSize + " bytes");
                    Console.WriteLine("Size: " + mATA.BlockCount * mATA.BlockSize / 1024 / 1024 + " MB");
                    Console.WriteLine("--------------------------");
                }
                else
                {
                    Console.WriteLine("No ATA devices found!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            Console.WriteLine("Complete...");
            Console.WriteLine("Press enter.");
            Console.ReadLine();
        }
        */
    }
}
