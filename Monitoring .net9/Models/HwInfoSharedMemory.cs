using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Monitoring_net9.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HwInfoSharedMemHeader
    {
        public uint Signature;

        public uint Version;

        public uint Revision;

        public long PollTime;

        public uint SensorSectionOffset;

        public uint SensorElementSize;

        public uint SensorElementCount;

        public uint ReadingSectionOffset;

        public uint ReadingElementSize;

        public uint ReadingElementCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct HwInfoReadingElement
    {
        public uint ReadingType;

        public uint SensorIndex;

        public uint ReadingID;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string LabelOrig;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string LabelUser;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Unit;

        public double Value;

        public double ValueMin;

        public double ValueMax;

        public double ValueAvg;
    }
}